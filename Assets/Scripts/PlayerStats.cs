using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Config/PlayerStats", order = 1)]
public class PlayerStats : ScriptableObject
{
    [Header("Movimiento")]
    [Tooltip("Fuerza aplicada al salto (AddForce)")]
    public float fuerzaSalto = 425f;

    [Tooltip("Velocidad en suelo")]
    public float velocidadSuelo = 5f;

    [Tooltip("Multiplicador de velocidad en aire")]
    public float multiplicadorAire = 0.5f;

    [Header("Powerups - movimiento")]
    [Tooltip("Multiplicador de movimiento mientras se monta la escoba")]
    public float escobaMoveMultiplier = 4.5f;

    [Tooltip("Multiplicador de movimiento mientras se monta Buckbeak")]
    public float buckbeakMoveMultiplier = 5f;
}