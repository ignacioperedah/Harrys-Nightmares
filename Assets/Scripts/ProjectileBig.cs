using UnityEngine;

/// <summary>
/// Proyectil del Patronus Grande: se desplaza hacia la derecha a velocidad constante
/// destruyendo dementores a su paso sin verse afectado por la física de colisión.
/// Requiere Rigidbody2D en modo Kinematic y Collider2D con "Is Trigger = true".
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class ProjectileBig : MonoBehaviour
{
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
        if (other.CompareTag("Dementor"))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.score++;

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX("Hit");

            Destroy(other.gameObject);
            // El patronus NO se destruye: sigue su camino
        }
    }
}
