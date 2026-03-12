using UnityEngine;

/// <summary>
/// Variante de Dementor: movimiento en zigzag vertical mientras persigue al jugador.
/// Asigna este componente en lugar de Dementor en el Prefab "DementorZigzag".
/// </summary>
public class DementorZigzag : Dementor
{
    [Tooltip("Amplitud del zigzag en unidades")]
    [SerializeField] private float amplitude = 1.5f;

    [Tooltip("Frecuencia del zigzag (ciclos por segundo)")]
    [SerializeField] private float frequency = 2f;

    private float _timeOffset;

    new void Start()
    {
        base.Start();
        // Offset aleatorio para que no todos los zigzag estén sincronizados
        _timeOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    new void Update()
    {
        // El movimiento base hacia el jugador lo maneja Dementor.Update()
        base.Update();

        // Añadir desplazamiento vertical sinusoidal encima del movimiento base
        float offsetY = Mathf.Sin(Time.time * frequency + _timeOffset) * amplitude * Time.deltaTime;
        transform.position += new Vector3(0f, offsetY, 0f);
    }
}