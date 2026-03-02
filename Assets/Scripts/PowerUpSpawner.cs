using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Responsable de decidir y crear power-ups.
/// - Prefabs serializados en el inspector.
/// - Método público SpawnPowerup() para invocar desde GameManager.
/// - Mantiene un registro en memoria de powerups instanciados para evitar búsquedas por tag.
/// - Implementa Singleton y limpieza periódica de referencias nulas.
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

    // Singleton
    public static PowerUpSpawner Instance { get; private set; }

    // Limpieza periódica
    private float cleanTimer;
    private const float CleanIntervalSeconds = 2f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        // Ejecutar limpieza periódica de referencias nulas para no sobrecargar la CPU.
        cleanTimer -= Time.deltaTime;
        if (cleanTimer <= 0f)
        {
            CleanNullReferences();
            cleanTimer = CleanIntervalSeconds;
        }
    }

    // Lógica pública para decidir qué y cuándo spawnear
    public void SpawnPowerup()
    {
        // Construimos candidatos con filtros para evitar duplicados en escena
        var candidates = new List<int>();
        // vida siempre permitida
        candidates.Add(0);

        // Si cualquiera de los powerups de "movimiento" está activo en el juego, no añadir
        bool movementPowerupActive = GameManager.Instance != null && (GameManager.Instance.powerupescobabool || GameManager.Instance.powerupbuckbeakbool);

        // Evitar spawnear Escoba/BuckBeak/Patronus si ya hay una instancia registrada en la lista
        bool escobaInScene = _activePowerups.Exists(go => go != null && go.CompareTag(TagEscoba));
        bool buckInScene = _activePowerups.Exists(go => go != null && go.CompareTag(TagBuckbeak));
        bool patronusInScene = _activePowerups.Exists(go => go != null && go.CompareTag(TagPatronus));

        if (!movementPowerupActive && !escobaInScene && !buckInScene)
        {
            candidates.Add(1); // escoba
            candidates.Add(3); // buckbeak
        }

        // Si no hay Patronus activo ni uno ya cayendo, permitir Patronus
        bool superPatronusActive = GameManager.Instance != null && GameManager.Instance.superpatronus;
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
        }
    }

    /// <summary>
    /// Permite que powerups se desregistren explícitamente (por ejemplo, si el powerup tiene un script
    /// que llama a Unregister en OnDestroy/OnDisable).
    /// </summary>
    public void Unregister(GameObject go)
    {
        if (go == null)
        {
            // limpieza de referencias nulas
            _activePowerups.RemoveAll(item => item == null);
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
    }

    /// <summary>
    /// Limpieza rápida de referencias nulas en el registro.
    /// </summary>
    public void CleanNullReferences()
    {
        _activePowerups.RemoveAll(item => item == null);
    }
}