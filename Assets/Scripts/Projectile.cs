using UnityEngine;

/// <summary>
/// Componente genérico para proyectiles (actualizado para pooling):
/// - No destruye el objeto; en su lugar lo desactiva para ser reutilizado por PlayerCombat.
/// - Mantiene Init(direction) para lanzar.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [Tooltip("Velocidad del proyectil (unidades/segundo)")]
    [SerializeField] private float speed = 10f;

    [Tooltip("Tiempo de vida en segundos (auto-destrucción)")]
    [SerializeField] private float lifeTime = 5f;

    [Tooltip("Si true, el proyectil usará AddForce en lugar de velocity (no recomendado)")]
    [SerializeField] private bool useForce = false;

    private Rigidbody2D rb;
    private Vector2 direction = Vector2.right;
    private float spawnTime;
    private bool initialized = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    void OnEnable()
    {
        spawnTime = Time.time;
        IgnoreTagsCollision("Pared");
        IgnoreTagsCollision("Harry");
        IgnoreTagsCollision("Vidas");
    }

    void Start()
    {
        if (initialized)
        {
            Launch();
        }
        else
        {
            Launch();
        }
    }

    void Update()
    {
        // Vida limitada -> en lugar de Destroy, desactivamos para pooling
        if (Time.time - spawnTime >= lifeTime)
        {
            DeactivateSelf();
            return;
        }

        Vector3 pos = transform.position;
        if (Mathf.Abs(pos.x) > 12f || Mathf.Abs(pos.y) > 12f)
        {
            DeactivateSelf();
        }
    }

    /// <summary>
    /// Inicializa dirección del proyectil (normalizada internamente).
    /// Llamar inmediatamente después de Instantiate.
    /// </summary>
    public void Init(Vector2 dir)
    {
        if (dir.sqrMagnitude <= 0.0001f) return;
        direction = dir.normalized;
        initialized = true;
        if (rb != null && enabled)
        {
            Launch();
        }
    }

    private void Launch()
    {
        if (rb == null) return;
        if (useForce)
        {
            rb.AddForce(direction * speed, ForceMode2D.Impulse);
        }
        else
        {
            rb.velocity = direction * speed;
        }
    }

    private void IgnoreTagsCollision(string tag)
    {
        GameObject[] list = GameObject.FindGameObjectsWithTag(tag);
        if (list == null || list.Length == 0) return;

        Collider2D myCol = GetComponent<Collider2D>();
        foreach (var go in list)
        {
            Collider2D other = go.GetComponent<Collider2D>();
            if (other != null && myCol != null)
            {
                Physics2D.IgnoreCollision(other, myCol);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Dementor"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.score++;
                if (GameManager.Instance.audiohit != null)
                {
                    GameManager.Instance.audiohit.SetActive(true);
                }
            }

            // destruir el dementor, pero desactivar el proyectil para reutilización
            Destroy(collision.gameObject);
            DeactivateSelf();
            return;
        }

        // colisión con otros colliders -> desactivar para pooling
        DeactivateSelf();
    }

    private void DeactivateSelf()
    {
        if (rb != null) rb.velocity = Vector2.zero;
        initialized = false;
        gameObject.SetActive(false);
    }
}
