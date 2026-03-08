using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour // Se encargara de generar enemigos en el juego, se le asignara un prefab de enemigo y se encargara de instanciarlo cada cierto tiempo
{
    [Header("Settings")]
    public GameObject dementorPrefab;
    public Transform playerTransform;

    [Header("Spawn Area")]
    public float minDistanceToPlayer = 3f;
    public float maxDistanceX = 10f;
    public float minY = 0.05f;
    public float maxY = 5f;

    private Coroutine spawnCoroutine;
    private float currentDelay = 2000f; // En milisegundos para ser compatible con tu lógica anterior

    // Registro de enemigos activos para evitar búsquedas por Tag
    private readonly List<GameObject> _activeEnemies = new List<GameObject>();
    public IReadOnlyList<GameObject> ActiveEnemies => _activeEnemies.AsReadOnly();

    // Singleton ligero para permitir Unregister desde Dementor sin Find
    public static EnemySpawner Instance { get; private set; }

    // Limpieza incremental: distribuye el recorrido de la lista entre frames
    private float _cleanTimer;
    private const float CleanIntervalSeconds = 2f;
    private int _cleanIndex; // índice actual del paso de limpieza incremental

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
        // Limpieza incremental: en lugar de RemoveAll en un único frame,
        // procesamos un lote pequeño por ciclo del timer.
        _cleanTimer -= Time.deltaTime;
        if (_cleanTimer <= 0f)
        {
            StepCleanNullReferences();
            _cleanTimer = CleanIntervalSeconds;
        }
    }

    // El GameManager llamará a esto para empezar el nivel
    public void StartSpawning(float initialDelay)
    {
        currentDelay = initialDelay;
        if (spawnCoroutine == null)
            spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    // El GameManager llamará a esto en el GameOver o Pausa
    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    // Esta función actualiza el delay basándose en tu lógica de dificultad
    public void UpdateSpawnRate(float newDelay)
    {
        currentDelay = newDelay;
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            SpawnEnemy();
            // Convertimos ms a segundos para WaitForSeconds
            yield return new WaitForSeconds(currentDelay / 1000f);
        }
    }

    private void SpawnEnemy()
    {
        if (dementorPrefab == null || playerTransform == null) return;

        // Aplicamos tu lógica de rango aleatorio pero más limpia
        float spawnDirection = Random.value > 0.5f ? 1f : -1f;
        float randomX = playerTransform.position.x + (spawnDirection * Random.Range(minDistanceToPlayer, maxDistanceX));
        float randomY = Random.Range(minY, maxY);

        Vector3 spawnPosition = new Vector3(randomX, randomY, 0);
        GameObject go = Instantiate(dementorPrefab, spawnPosition, Quaternion.identity);
        if (go != null)
        {
            _activeEnemies.Add(go);
            // Resetear el índice para que el próximo ciclo empiece desde el principio
            _cleanIndex = 0;
        }
    }

    /// <summary>
    /// Permite que Dementor desregistre su GameObject al destruirse.
    /// Siempre via Singleton: nunca usar FindObjectOfType.
    /// </summary>
    public void Unregister(GameObject go)
    {
        if (go == null)
        {
            _activeEnemies.RemoveAll(item => item == null);
            _cleanIndex = 0;
            return;
        }

        _activeEnemies.Remove(go);
    }

    /// <summary>
    /// Limpieza completa de referencias nulas (compatibilidad con llamadas externas).
    /// </summary>
    public void CleanNullReferences()
    {
        _activeEnemies.RemoveAll(item => item == null);
        _cleanIndex = 0;
    }

    /// <summary>
    /// Limpieza incremental (stepwise): procesa un lote de entradas por llamada
    /// en lugar de recorrer toda la lista de golpe, distribuyendo el coste de CPU.
    /// Usa swap con el último elemento para lograr eliminación O(1).
    /// </summary>
    private void StepCleanNullReferences()
    {
        if (_activeEnemies.Count == 0)
        {
            _cleanIndex = 0;
            return;
        }

        const int batchSize = 5;
        int processed = 0;

        while (_cleanIndex < _activeEnemies.Count && processed < batchSize)
        {
            if (_activeEnemies[_cleanIndex] == null)
            {
                // Swap con el último elemento y eliminar: O(1) vs O(n) de RemoveAt normal
                int last = _activeEnemies.Count - 1;
                _activeEnemies[_cleanIndex] = _activeEnemies[last];
                _activeEnemies.RemoveAt(last);
                // No avanzar _cleanIndex: el elemento movido aún no fue revisado
            }
            else
            {
                _cleanIndex++;
            }

            processed++;
        }

        // Reiniciar al llegar al final para el próximo ciclo del timer
        if (_cleanIndex >= _activeEnemies.Count)
        {
            _cleanIndex = 0;
        }
    }
}
