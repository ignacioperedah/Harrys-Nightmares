using UnityEngine;

public class Dementor : MonoBehaviour
{
    public float speed;
    private Rigidbody2D rb;
    private Transform target;
    private Animator anim;

    public int kill = 1;
    public int kcount;
    public bool Hit = false;
 
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        target = GameObject.FindGameObjectWithTag("Harry").GetComponent<Transform>();
        anim = GetComponent<Animator>();
    }

    public void OnEnable()
    {
        GameObject[] noCollision = GameObject.FindGameObjectsWithTag("Dementor");
        GameObject[] noCollision2 = GameObject.FindGameObjectsWithTag("Pared");
        GameObject[] noCollision3 = GameObject.FindGameObjectsWithTag("Vidas");

        foreach (GameObject Dementor in noCollision)
        {
            Physics2D.IgnoreCollision(Dementor.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }

        foreach (GameObject Dementor in noCollision2)
        {
            Physics2D.IgnoreCollision(Dementor.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }

        foreach (GameObject Dementor in noCollision3)
        {
            Physics2D.IgnoreCollision(Dementor.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }

    }
    // Update is called once per frame
    void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        anim.SetBool("goL", false);
        GameManager puntos = GameObject.FindGameObjectWithTag("GM").GetComponent<GameManager>();
        GameManager reset = GameObject.FindGameObjectWithTag("GM").GetComponent<GameManager>();

        if (Hit == true)
        {
            Destroy(gameObject);
        }
        
        if (reset.reset == true || reset.CurrentState == GameState.GameOver || reset.vidas <= 0)
        {
            Destroy(gameObject);
        }

        if(target.position.x > transform.position.x)
        {
            anim.SetBool("goL", false);
        }

        if (target.position.x < transform.position.x)
        {
            anim.SetBool("goL", true);
        }

        if (transform.position.x < -12 || transform.position.x > 12)
        {
            Destroy(gameObject);
        }

        if (transform.position.y < -10 || transform.position.y > 10)
        {
            Destroy(gameObject);
        }

        GameObject[] noCollision = GameObject.FindGameObjectsWithTag("Dementor");
        GameObject[] noCollision2 = GameObject.FindGameObjectsWithTag("Pared");
        GameObject[] noCollision3 = GameObject.FindGameObjectsWithTag("Vidas");

        foreach (GameObject Dementor in noCollision)
        {
            Physics2D.IgnoreCollision(Dementor.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }

        foreach (GameObject Dementor in noCollision2)
        {
            Physics2D.IgnoreCollision(Dementor.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }

        foreach (GameObject Dementor in noCollision3)
        {
            Physics2D.IgnoreCollision(Dementor.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        GameManager puntos = GameObject.FindGameObjectWithTag("GM").GetComponent<GameManager>();

        if (collision.gameObject.CompareTag("Hechizo") || collision.gameObject.CompareTag("Harry"))
        {
            Hit = true;
        }

        if(collision.gameObject.CompareTag("Harry") && puntos.powerupbuckbeakbool == true)
        {
            puntos.score++;
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("Hit");
        }
    }
    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Hechizo"))
        {
            Hit = false;
        }
    }
}
