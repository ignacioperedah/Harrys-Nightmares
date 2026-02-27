using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Responsable de decidir y crear power-ups.
/// - Prefabs serializados en el inspector.
/// - Método público SpawnPowerup() para invocar desde GameManager.
/// - Filtra tipos que ya están cayendo en escena por tag para evitar duplicados.
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

    // Lógica pública para decidir cuándo y qué spawnear
    public void SpawnPowerup()
    {
        // Construimos candidatos con filtros para evitar duplicados en escena
        var candidates = new List<int>();
        // vida siempre permitida
        candidates.Add(0);

        // Si cualquiera de los powerups de "movimiento" está activo en el juego, no añadir
        bool movementPowerupActive = GameManager.Instance != null && (GameManager.Instance.powerupescobabool || GameManager.Instance.powerupbuckbeakbool);

        // Evitar spawnear Escoba si ya hay una Escoba en escena
        bool escobaInScene = GameObject.FindGameObjectsWithTag(TagEscoba).Length > 0;
        bool buckInScene = GameObject.FindGameObjectsWithTag(TagBuckbeak).Length > 0;
        bool patronusInScene = GameObject.FindGameObjectsWithTag(TagPatronus).Length > 0;

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

        switch (choice)
        {
            case 0:
                if (powerupVida != null) Instantiate(powerupVida, pos, Quaternion.identity);
                break;
            case 1:
                if (powerupEscoba != null) Instantiate(powerupEscoba, pos, Quaternion.identity);
                break;
            case 2:
                if (powerupPatronus != null) Instantiate(powerupPatronus, pos, Quaternion.identity);
                break;
            case 3:
                if (powerupBuckbeak != null) Instantiate(powerupBuckbeak, pos, Quaternion.identity);
                break;
        }
    }
}