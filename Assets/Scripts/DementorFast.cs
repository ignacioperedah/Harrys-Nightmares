using UnityEngine;

/// <summary>
/// Variante de Dementor: velocidad aumentada al instanciarse.
/// Asigna este componente en lugar de Dementor en el Prefab "DementorFast".
/// </summary>
public class DementorFast : Dementor
{
    [Tooltip("Multiplicador sobre la velocidad base del Dementor")]
    [SerializeField] private float speedMultiplier = 1.8f;

    new void Start()
    {
        // Llamar al Start del padre primero para inicializar target y anim
        base.Start();
        speed *= speedMultiplier;
    }
}