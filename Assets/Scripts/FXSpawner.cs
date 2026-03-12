using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawner de efectos de partículas con pool por tipo.
/// Permite reproducir el mismo efecto simultáneamente en múltiples posiciones.
/// Usa isStopped (no isPlaying) para detectar instancias disponibles de forma fiable.
/// </summary>
public class FXSpawner : MonoBehaviour
{
    public static FXSpawner Instance { get; private set; }

    [Header("Prefabs de Partículas")]
    [SerializeField] private ParticleSystem dementorDeathFXPrefab;
    [SerializeField] private ParticleSystem spellSpawnFXPrefab;

    [Header("Tamaño inicial del pool")]
    [SerializeField] private int dementorDeathPoolSize = 5;
    [SerializeField] private int spellSpawnPoolSize    = 3;

    private readonly List<ParticleSystem> _dementorDeathPool = new List<ParticleSystem>();
    private readonly List<ParticleSystem> _spellSpawnPool    = new List<ParticleSystem>();

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        WarmPool(dementorDeathFXPrefab, _dementorDeathPool, dementorDeathPoolSize);
        WarmPool(spellSpawnFXPrefab,    _spellSpawnPool,    spellSpawnPoolSize);
    }

    // ── API pública ───────────────────────────────────────────────────────────

    /// <summary>Reproduce el efecto de muerte de dementor en la posición indicada.</summary>
    public void PlayDementorDeath(Vector3 position)
    {
        PlayFXFromPool(_dementorDeathPool, dementorDeathFXPrefab, position);
    }

    /// <summary>Reproduce el efecto de spawn de proyectil en la posición indicada.</summary>
    public void PlaySpellSpawn(Vector3 position)
    {
        PlayFXFromPool(_spellSpawnPool, spellSpawnFXPrefab, position);
    }

    // ── Pool helpers ──────────────────────────────────────────────────────────

    private void WarmPool(ParticleSystem prefab, List<ParticleSystem> pool, int size)
    {
        if (prefab == null) return;
        for (int i = 0; i < size; i++)
            pool.Add(CreateInstance(prefab));
    }

    private ParticleSystem CreateInstance(ParticleSystem prefab)
    {
        ParticleSystem inst = Instantiate(prefab, transform);

        // IMPORTANTE: Stop Action debe ser "None" en el Prefab.
        // Lo forzamos por código para garantizarlo independientemente
        // de cómo esté configurado el asset en el Editor.
        var main = inst.main;
        main.stopAction = ParticleSystemStopAction.None;
        main.loop       = false;
        main.playOnAwake = false;

        inst.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        return inst;
    }

    private void PlayFXFromPool(List<ParticleSystem> pool, ParticleSystem prefab, Vector3 position)
    {
        if (prefab == null) return;

        ParticleSystem fx = GetAvailable(pool);

        // Pool exhausto: expandir con una nueva instancia
        if (fx == null)
        {
            fx = CreateInstance(prefab);
            pool.Add(fx);
            Debug.Log($"[FXSpawner] Pool expandido → {pool.Count} instancias para '{prefab.name}'.");
        }

        fx.transform.position = position;
        fx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        fx.Play();
    }

    /// <summary>
    /// Devuelve la primera instancia disponible del pool.
    /// Una instancia está disponible cuando isStopped == true,
    /// lo que ocurre tras terminar su duración o llamar a Stop().
    /// </summary>
    private static ParticleSystem GetAvailable(List<ParticleSystem> pool)
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (pool[i] != null && pool[i].isStopped)
                return pool[i];
        }
        return null;
    }

    // ── Test (Play Mode, clic derecho sobre el componente) ────────────────────

    [ContextMenu("Test → DementorDeath en origen")]
    private void TestDementorDeath() => PlayDementorDeath(Vector3.zero);

    [ContextMenu("Test → SpellSpawn en origen")]
    private void TestSpellSpawn() => PlaySpellSpawn(Vector3.zero);
}