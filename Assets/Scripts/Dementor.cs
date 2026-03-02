using UnityEngine;

public class Dementor : MonoBehaviour
{
    public float speed;
    private Transform target;
    private Animator anim;

    public int kill = 1;
    public int kcount;
    public bool Hit = false;
 
    // Start is called before the first frame update
    void Start()
    {
        // Preferir referencia desde GameManager (evita FindGameObjectWithTag)
        if (GameManager.Instance != null && GameManager.Instance.harry != null)
        {
            target = GameManager.Instance.harry;
        }
        else
        {
            var harryObj = GameObject.FindGameObjectWithTag("Harry");
            if (harryObj != null) target = harryObj.transform;
        }

        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        }

        if (anim != null)
        {
            anim.SetBool("goL", false);
            if (target != null)
            {
                anim.SetBool("goL", target.position.x < transform.position.x);
            }
        }

        var gm = GameManager.Instance;
        
        if (gm != null && (gm.reset == true || gm.CurrentState == GameState.GameOver || gm.vidas <= 0))
        {
            Destroy(gameObject);
            return;
        }

        if (transform.position.x < -12 || transform.position.x > 12)
        {
            Destroy(gameObject);
            return;
        }

        if (transform.position.y < -10 || transform.position.y > 10)
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        var gm = GameManager.Instance;

        if (collision.gameObject.CompareTag("Hechizo"))
        {
            GameManager.Instance.score++;
        }

        if (collision.gameObject.CompareTag("Harry") && gm != null && gm.powerupbuckbeakbool == true)
        {
            gm.score++;
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("Hit");
        }

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        // Desregistrar del spawner para mantener listas limpias
        EnemySpawner.Instance?.Unregister(gameObject);
    }

    void OnDisable()
    {
        // También intentar desregistrar al desactivar (por pooling u otros)
        EnemySpawner.Instance?.Unregister(gameObject);
    }
}
