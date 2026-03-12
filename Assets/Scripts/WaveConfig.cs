using UnityEngine;
using System;

[Serializable]
public struct WaveDefinition
{
    [Tooltip("Número de dementores en esta oleada")]
    public int count;

    [Tooltip("Delay entre spawns dentro de la oleada (ms)")]
    public int intraWaveDelayMs;

    [Tooltip("Pausa antes de la siguiente oleada (segundos)")]
    public float restSeconds;
}

[CreateAssetMenu(fileName = "WaveConfig", menuName = "Config/WaveConfig", order = 3)]
public class WaveConfig : ScriptableObject
{
    [Tooltip("Definiciones de oleada en orden. La última se repite indefinidamente.")]
    public WaveDefinition[] waves = new WaveDefinition[]
    {
        new WaveDefinition { count = 2, intraWaveDelayMs = 1500, restSeconds = 4f },
        new WaveDefinition { count = 3, intraWaveDelayMs = 1200, restSeconds = 3f },
        new WaveDefinition { count = 5, intraWaveDelayMs = 900,  restSeconds = 2f },
        new WaveDefinition { count = 7, intraWaveDelayMs = 600,  restSeconds = 1.5f },
    };
}