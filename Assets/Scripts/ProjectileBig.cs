using System;
using UnityEngine;

/// <summary>
/// Proyectil del Patronus Grande: se desplaza hacia la derecha a velocidad constante
/// destruyendo dementores a su paso sin verse afectado por la física de colisión.
/// Requiere Rigidbody2D en modo Kinematic y Collider2D con "Is Trigger = true".
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class ProjectileBig : MonoBehaviour
{
    /// <summary>Se dispara cada vez que el Patronus destruye un Dementor.</summary>
    public static event Action OnDementorKilled;

    [Tooltip("Velocidad de desplazamiento hacia la derecha (unidades/segundo)")]
    [SerializeField] private float speed = 8f;

    [Tooltip("Límite en X a partir del cual se destruye el GameObject")]
    [SerializeField] private float destroyAtX = 12f;

    void Awake()
    {
        // Configurar Rigidbody2D como kinematic: se mueve sin física
        var rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Update()
    {
        // Movimiento constante hacia la derecha, independiente de la física
        transform.Translate(Vector2.right * speed * Time.deltaTime, Space.World);

        // Auto-destrucción al salir del área de juego
        if (transform.position.x > destroyAtX)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(GameConstants.Tags.Dementor))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.score++;

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(GameConstants.Audio.Hit);

            // Spawnea el efecto de muerte del Dementor
            if (FXSpawner.Instance != null)
                FXSpawner.Instance.PlayDementorDeath(other.transform.position);

            Destroy(other.gameObject);
        }
    }
}
