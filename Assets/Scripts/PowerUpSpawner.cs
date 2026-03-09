using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Responsable de decidir y crear power-ups.
/// - Prefabs serializados en el inspector.
/// - Método público SpawnPowerup() para invocar desde GameManager.
/// - Mantiene un registro en memoria de powerups instanciados para evitar búsquedas por tag.
/// - Singleton robusto con limpieza incremental por pasos (stepwise) cada frame del cleanTimer.
/// </summary>
public class PowerUpSpawner : MonoBehaviour
{
    [Header("PowerUp Prefabs")]
    [SerializeField] private GameObject powerupVida;
    [SerializeField] private GameObject powerupEscoba;
    [SerializeField] private GameObject powerupPatronus;
    [SerializeField] private GameObject powerupBuckbeak;

    // Tags usados por el proyecto (coinciden con la lógica de GameManager.DestroyActivePowerups)
    private const string TagVida = "Vidas";
    private const string TagEscoba = "Escoba";
    private const string TagPatronus = "Patronus";
    private const string TagBuckbeak = "Buckbeak";

    // Registro en memoria de powerups activos (instanciados)
    private readonly List<GameObject> _activePowerups = new List<GameObject>();
    public IReadOnlyList<GameObject> ActivePowerups => _activePowerups.AsReadOnly();

    // Singleton robusto: persiste entre escenas si es necesario
    public static PowerUpSpawner Instance { get; private set; }

    // Limpieza incremental (stepwise): en lugar de recorrer toda la lista cada CleanInterval,
    // avanzamos un índice por Update, distribuyendo el coste entre frames.
    private float _cleanTimer;
    private const float CleanIntervalSeconds = 2f;
    private int _cleanIndex; // índice actual del paso de limpieza incremental

    void Awake()
    {
        // Singleton robusto: si ya hay instancia, destruir el duplicado sin afectar la escena
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        // Limpieza incremental: distribuye el trabajo en múltiples frames.
        // Cada vez que el timer expira, procesamos un lote de entradas para no saturar un frame.
        _cleanTimer -= Time.deltaTime;
        if (_cleanTimer <= 0f)
        {
            StepCleanNullReferences();
            _cleanTimer = CleanIntervalSeconds;
        }
    }

    // Lógica pública para decidir qué y cuándo spawnear
    public void SpawnPowerup()
    {
        // Construimos candidatos con filtros para evitar duplicados en escena
        var candidates = new List<int>();
        // vida siempre permitida
        candidates.Add(0);

        var powerUpHandler = PowerUpHandler.Instance;

        // Si cualquiera de los powerups de "movimiento" está activo en el juego, no añadir
        bool movementPowerupActive = powerUpHandler != null &&
            (powerUpHandler.PowerupEscobaBool || powerUpHandler.PowerupBuckbeakBool);

        // Evitar spawnear Escoba/BuckBeak/Patronus si ya hay una instancia registrada en la lista.
        // Recorrido único de la lista para calcular los tres flags a la vez.
        bool escobaInScene = false;
        bool buckInScene = false;
        bool patronusInScene = false;
        for (int i = 0; i < _activePowerups.Count; i++)
        {
            var go = _activePowerups[i];
            if (go == null) continue;
            if (!escobaInScene && go.CompareTag(TagEscoba)) escobaInScene = true;
            else if (!buckInScene && go.CompareTag(TagBuckbeak)) buckInScene = true;
            else if (!patronusInScene && go.CompareTag(TagPatronus)) patronusInScene = true;
            if (escobaInScene && buckInScene && patronusInScene) break;
        }

        if (!movementPowerupActive && !escobaInScene && !buckInScene)
        {
            candidates.Add(1); // escoba
            candidates.Add(3); // buckbeak
        }

        // Si no hay Patronus activo ni uno ya cayendo, permitir Patronus
        bool superPatronusActive = GameManager.Instance != null &&  powerUpHandler.PowerUpPatronusBool;
        if (!superPatronusActive && !patronusInScene)
        {
            candidates.Add(2); // patronus
        }

        // Seguridad: aseguramos al menos una opción
        if (candidates.Count == 0)
        {
            candidates.Add(0);
        }

        int choice = candidates[Random.Range(0, candidates.Count)];
        float x = Random.Range(-9f, 9f);
        Vector3 pos = new Vector3(x, 6.5f, 0);

        GameObject spawned = null;

        switch (choice)
        {
            case 0:
                if (powerupVida != null) spawned = Instantiate(powerupVida, pos, Quaternion.identity);
                break;
            case 1:
                if (powerupEscoba != null) spawned = Instantiate(powerupEscoba, pos, Quaternion.identity);
                break;
            case 2:
                if (powerupPatronus != null) spawned = Instantiate(powerupPatronus, pos, Quaternion.identity);
                break;
            case 3:
                if (powerupBuckbeak != null) spawned = Instantiate(powerupBuckbeak, pos, Quaternion.identity);
                break;
        }

        if (spawned != null)
        {
            // Registrar para permitir consultas rápidas sin FindByTag
            _activePowerups.Add(spawned);
            // Resetear índice de limpieza para que empiece desde el principio en el próximo ciclo
            _cleanIndex = 0;
        }
    }

    /// <summary>
    /// Permite que powerups se desregistren explícitamente al recogerse o destruirse.
    /// Exclusivamente a través del Singleton: nunca usar FindObjectOfType.
    /// </summary>
    public void Unregister(GameObject go)
    {
        if (go == null)
        {
            // limpieza de referencias nulas directa cuando no hay objeto concreto
            _activePowerups.RemoveAll(item => item == null);
            _cleanIndex = 0;
            return;
        }

        _activePowerups.Remove(go);
    }

    /// <summary>
    /// Elimina y limpia todos los powerups registrados (uso desde GameManager en GameOver).
    /// </summary>
    public void DestroyAllActivePowerups()
    {
        // Evitar modificar colección mientras iteramos directamente; creamos copia
        var snapshot = _activePowerups.ToArray();
        foreach (var go in snapshot)
        {
            if (go != null)
            {
                Destroy(go);
            }
        }

        _activePowerups.Clear();
        _cleanIndex = 0;
    }

    /// <summary>
    /// Limpieza incremental (stepwise): procesa un lote de entradas por llamada
    /// en lugar de recorrer toda la lista de golpe, distribuyendo el coste de CPU.
    /// Lote máximo: 5 entradas por ciclo (ajustable según tamaño esperado de la lista).
    /// </summary>
    public void CleanNullReferences()
    {
        // Mantener compatibilidad con llamadas externas: limpiar todo de una vez
        _activePowerups.RemoveAll(item => item == null);
        _cleanIndex = 0;
    }

    private void StepCleanNullReferences()
    {
        if (_activePowerups.Count == 0)
        {
            _cleanIndex = 0;
            return;
        }

        // Lote máximo por ciclo: evitamos recorrer listas grandes en un solo frame
        const int batchSize = 5;
        int processed = 0;

        // Recorremos hacia atrás desde _cleanIndex para no invalidar índices al eliminar
        while (_cleanIndex < _activePowerups.Count && processed < batchSize)
        {
            if (_activePowerups[_cleanIndex] == null)
            {
                // Swap con el último y eliminar O(1) en lugar de RemoveAt O(n)
                int last = _activePowerups.Count - 1;
                _activePowerups[_cleanIndex] = _activePowerups[last];
                _activePowerups.RemoveAt(last);
                // No avanzar _cleanIndex: el elemento movido aún no fue revisado
            }
            else
            {
                _cleanIndex++;
            }

            processed++;
        }

        // Si llegamos al final, reiniciamos para el siguiente ciclo del timer
        if (_cleanIndex >= _activePowerups.Count)
        {
            _cleanIndex = 0;
        }
    }
}