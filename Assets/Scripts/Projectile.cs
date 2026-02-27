using UnityEngine;

/// <summary>
/// Componente genérico para proyectiles (reemplaza HechizoL/HechizoR/HechizoU).
/// - Inicializar con Init(direction) o configurar la dirección en inspector.
/// - Se encarga de mover, detectar colisiones con Dementor y autodestruirse.
/// - Evita lógica repetida (IgnoreCollision) y heavy operations en Update.
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
        // Aseguramos que el proyectil no empuje a los enemigos por física:
        // Preferimos manejar la destrucción en OnCollisionEnter2D.
        if (rb != null)
        {
            // modo cinemático evita que fuerzas empujen otros rigidbodies,
            // pero mantiene colisiones para triggers / contactos y permite velocity control.
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    void OnEnable()
    {
        spawnTime = Time.time;
        // Ignorar colisiones con objetos del jugador/pared/vidas: hacemos esto una vez al enable
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
            // si no se inicializó explícitamente, usar la dirección por defecto (derecha)
            Launch();
        }
    }

    void Update()
    {
        // Vida limitada
        if (Time.time - spawnTime >= lifeTime)
        {
            Destroy(gameObject);
            return;
        }

        // límites de pantalla (seguridad similar a original). Ajustar si hace falta.
        Vector3 pos = transform.position;
        if (Mathf.Abs(pos.x) > 12f || Mathf.Abs(pos.y) > 12f)
        {
            Destroy(gameObject);
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
        // Si Start ya pasó, lanzar ahora:
        if (rb != null && enabled) Launch();
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
            // Usamos velocity directo para un movimiento determinista y evitar empujes fuertes
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
        // cuando choca con Dementor, destruir proyectil y el dementor sin depender de la física
        if (collision.gameObject.CompareTag("Dementor"))
        {
            // Evitar empujar: aplicar la destrucción directa del enemigo y del proyectil.
            // Además actualizamos score y reproducimos audio si existe en GameManager.
            if (GameManager.Instance != null)
            {
                GameManager.Instance.score++;
                if (GameManager.Instance.audiohit != null)
                {
                    GameManager.Instance.audiohit.SetActive(true);
                }
            }

            // Destruir el enemigo de forma inmediata
            Destroy(collision.gameObject);

            // Destruir el proyectil
            Destroy(gameObject);
            return;
        }

        // Si choca con otros colliders (muros, suelo...), destruimos el proyectil para evitar rebotes
        // pero sin aplicar fuerzas que puedan empujar objetos dinámicos.
        // Permitimos pasar por tags ignoradas ya en OnEnable.
        Destroy(gameObject);
    }
}
