using UnityEngine;
using System;

[Serializable]
public struct DifficultyStep
{
    public int threshold;
    public int delay;
}

[CreateAssetMenu(fileName = "DifficultyConfig", menuName = "Config/DifficultyConfig", order = 2)]
public class DifficultyConfig : ScriptableObject
{
    [Tooltip("Escalones de dificultad, ordenados de mayor a menor threshold.")]
    public DifficultyStep[] steps = new DifficultyStep[]
    {
        new DifficultyStep { threshold = 100, delay = 113 },
        new DifficultyStep { threshold = 90, delay = 150 },
        new DifficultyStep { threshold = 80, delay = 200 },
        new DifficultyStep { threshold = 70, delay = 267 },
        new DifficultyStep { threshold = 60, delay = 356 },
        new DifficultyStep { threshold = 50, delay = 475 },
        new DifficultyStep { threshold = 40, delay = 633 },
        new DifficultyStep { threshold = 30, delay = 844 },
        new DifficultyStep { threshold = 20, delay = 1125 },
        new DifficultyStep { threshold = 10, delay = 1500 }
    };
}