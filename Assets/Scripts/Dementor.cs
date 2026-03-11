using UnityEngine;

public class Dementor : MonoBehaviour
{
    public float speed;
    private Transform target;
    private Animator anim;

    public int kill = 1;
    public int kcount = 0;
    public bool Hit = false;

    // ?? Animator hash (static: se calcula una única vez para todas las instancias) ??
    private static readonly int IdGoL = Animator.StringToHash(GameConstants.AnimatorParams.GoL);

    // Start is called before the first frame update
    void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.harry != null)
            target = GameManager.Instance.harry;
        else
        {
            var harryObj = GameObject.FindGameObjectWithTag(GameConstants.Tags.Harry);
            if (harryObj != null) target = harryObj.transform;
        }

        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        var gm = GameManager.Instance;

        if (gm != null && (gm.reset || gm.CurrentState == GameState.GameOver || gm.vidas <= 0))
        {
            Destroy(gameObject);
            return;
        }

        if (target != null)
            transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (anim != null && target != null)
            anim.SetBool(IdGoL, target.position.x < transform.position.x);

        if (transform.position.x < -12f || transform.position.x > 12f)
        {
            Destroy(gameObject);
            return;
        }

        if (transform.position.y < -10f || transform.position.y > 10f)
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        var gm = GameManager.Instance;
        var powerUpHandler = PowerUpHandler.Instance;

        if (collision.gameObject.CompareTag(GameConstants.Tags.Hechizo))
        {
            if (gm != null) gm.score++;
        }

        if (collision.gameObject.CompareTag(GameConstants.Tags.Harry)
            && powerUpHandler != null
            && powerUpHandler.PowerupBuckbeakBool)
        {
            if (gm != null) gm.score++;
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(GameConstants.Audio.Hit);
        }

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        EnemySpawner.Instance?.Unregister(gameObject);
    }

    void OnDisable()
    {
        EnemySpawner.Instance?.Unregister(gameObject);
    }
}
