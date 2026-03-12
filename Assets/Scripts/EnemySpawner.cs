using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // ── Evento desacoplado ────────────────────────────────────────────────────
    /// <summary>Notifica el número de oleada actual (1-based).</summary>
    public static event Action<int> OnWaveStarted;

    [Header("Settings")]
    public GameObject[] dementorPrefabs; // índice 0 = normal, 1 = rápido, 2 = zigzag
    public Transform playerTransform;

    [Header("Wave Config")]
    [SerializeField] private WaveConfig waveConfig;

    [Header("Spawn Area")]
    public float minDistanceToPlayer = 3f;
    public float maxDistanceX        = 10f;
    public float minY = 0.05f;
    public float maxY = 5f;

    private Coroutine _spawnCoroutine;

    // Registro de enemigos activos
    private readonly List<GameObject> _activeEnemies = new List<GameObject>();
    public IReadOnlyList<GameObject> ActiveEnemies => _activeEnemies.AsReadOnly();

    public static EnemySpawner Instance { get; private set; }

    private float _cleanTimer;
    private const float CleanIntervalSeconds = 2f;
    private int _cleanIndex;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

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
        _cleanTimer -= Time.deltaTime;
        if (_cleanTimer <= 0f)
        {
            StepCleanNullReferences();
            _cleanTimer = CleanIntervalSeconds;
        }
    }

    // ── API pública ───────────────────────────────────────────────────────────

    public void StartSpawning(float initialDelay)
    {
        if (_spawnCoroutine == null)
            _spawnCoroutine = StartCoroutine(WaveRoutine());
    }

    public void StopSpawning()
    {
        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }
    }

    /// <summary>Compatibilidad con GameManager: se ignora en modo oleadas.</summary>
    public void UpdateSpawnRate(float newDelay) { }

    // ── Rutina de oleadas ─────────────────────────────────────────────────────

    private IEnumerator WaveRoutine()
    {
        if (waveConfig == null || waveConfig.waves == null || waveConfig.waves.Length == 0)
        {
            // Fallback: spawn continuo simple
            while (true)
            {
                SpawnEnemy(0);
                yield return new WaitForSeconds(2f);
            }
        }

        int waveIndex = 0;

        while (true)
        {
            // Fijar la oleada actual (la última se repite)
            int clampedIndex = Mathf.Min(waveIndex, waveConfig.waves.Length - 1);
            WaveDefinition wave = waveConfig.waves[clampedIndex];

            OnWaveStarted?.Invoke(waveIndex + 1);

            for (int i = 0; i < wave.count; i++)
            {
                // Seleccionar prefab según probabilidad: más oleadas → más variedad
                int prefabIndex = SelectPrefabIndex(waveIndex);
                SpawnEnemy(prefabIndex);
                yield return new WaitForSeconds(wave.intraWaveDelayMs / 1000f);
            }

            // Descanso entre oleadas
            yield return new WaitForSeconds(wave.restSeconds);

            waveIndex++;
        }
    }

    /// <summary>
    /// Selecciona el prefab a usar. Con más oleadas aumenta la probabilidad de variantes.
    /// Requiere que dementorPrefabs tenga al menos 1 elemento.
    /// </summary>
    private int SelectPrefabIndex(int waveIndex)
    {
        if (dementorPrefabs == null || dementorPrefabs.Length <= 1) return 0;

        // Oleada 0-1: solo normal
        if (waveIndex < 2) return 0;

        // Oleada 2-3: normal 70% / rápido 30%
        if (waveIndex < 4)
            return UnityEngine.Random.value < 0.7f ? 0 : Mathf.Min(1, dementorPrefabs.Length - 1);

        // Oleada 4+: normal 50% / rápido 30% / zigzag 20%
        float roll = UnityEngine.Random.value;
        if (roll < 0.5f) return 0;
        if (roll < 0.8f) return Mathf.Min(1, dementorPrefabs.Length - 1);
        return Mathf.Min(2, dementorPrefabs.Length - 1);
    }

    // ── Spawn ─────────────────────────────────────────────────────────────────

    private void SpawnEnemy(int prefabIndex)
    {
        if (dementorPrefabs == null || dementorPrefabs.Length == 0) return;
        if (playerTransform == null) return;

        int idx = Mathf.Clamp(prefabIndex, 0, dementorPrefabs.Length - 1);
        GameObject prefab = dementorPrefabs[idx];
        if (prefab == null) return;

        float dir    = UnityEngine.Random.value > 0.5f ? 1f : -1f;
        float randomX = playerTransform.position.x + dir * UnityEngine.Random.Range(minDistanceToPlayer, maxDistanceX);
        float randomY = UnityEngine.Random.Range(minY, maxY);

        GameObject go = Instantiate(prefab, new Vector3(randomX, randomY, 0f), Quaternion.identity);
        if (go == null) return;

        _activeEnemies.Add(go);
        _cleanIndex = 0;
    }

    // ── Registro ──────────────────────────────────────────────────────────────

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

    public void CleanNullReferences()
    {
        _activeEnemies.RemoveAll(item => item == null);
        _cleanIndex = 0;
    }

    private void StepCleanNullReferences()
    {
        if (_activeEnemies.Count == 0) { _cleanIndex = 0; return; }

        const int batchSize = 5;
        int processed = 0;

        while (_cleanIndex < _activeEnemies.Count && processed < batchSize)
        {
            if (_activeEnemies[_cleanIndex] == null)
            {
                int last = _activeEnemies.Count - 1;
                _activeEnemies[_cleanIndex] = _activeEnemies[last];
                _activeEnemies.RemoveAt(last);
            }
            else
            {
                _cleanIndex++;
            }
            processed++;
        }

        if (_cleanIndex >= _activeEnemies.Count) _cleanIndex = 0;
    }
}
