using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Harry : MonoBehaviour
{
    public float fuerzaSalto;
    public float fuerzaMovimientoDerecha;
    public float fuerzaMovimientoIzquierda;
    public float fuerzamovimientoabajo;
    public float fuerzamovimientoarriba;
    public GameManager gameManager;

    public Controller JS;

    public int numvidas = 1;

    public bool powerupescoba = false;
    public bool powerupbuckbeak = false;

    public Animator anim;
    public Rigidbody2D rb;
    public Transform tran;

    // Start is called before the first frame update
    public void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        tran = GetComponent<Transform>();

    }

    // Update is called once per frame
    public void Update()
    {
        // Sustituido el booleano eliminado 'start' por la comprobación del estado del GameManager.
        if (gameManager != null && gameManager.CurrentState == GameState.Playing)
        {
            if (Input.GetKeyDown(KeyCode.Space) && transform.position.y < 0 && powerupescoba == false && powerupbuckbeak == false)
            {
                rb.AddForce(new Vector2(0, fuerzaSalto));
            }

            if (transform.position.y > 0 && powerupescoba == false && powerupbuckbeak == false)
            {
                anim.SetBool("salto", true);
                fuerzaMovimientoDerecha = 5;
                fuerzaMovimientoIzquierda = -5;
            }

            if (transform.position.y < 0 && powerupescoba == false && powerupbuckbeak == false)
            {
                anim.SetBool("salto", false);
                fuerzaMovimientoDerecha = 10;
                fuerzaMovimientoIzquierda = -10;
            }

            if ((Input.GetKey(KeyCode.RightArrow) || JS.prueba > 0) && Input.GetMouseButton(0))
            {
                anim.SetBool("goLeft", false);
                anim.SetBool("runRight", true);

            }
            else
            {
                anim.SetBool("runRight", false);
            }

            if (Input.GetKeyDown(KeyCode.Z) || (gameManager != null && gameManager.buttonSpell == true))
            {
                anim.SetBool("disparoR", true);
            }
            else
            {
                anim.SetBool("disparoR", false);
            }

            if ((Input.GetKey(KeyCode.LeftArrow) || JS.prueba < 0) && Input.GetMouseButton(0))
            {
                anim.SetBool("goLeft", true);
                anim.SetBool("runLeft", true);
            }

            else
            {
                anim.SetBool("runLeft", false);
            }

            if ((Input.GetKeyDown(KeyCode.Z) || (gameManager != null && gameManager.buttonSpell == true)))
            {
                anim.SetBool("disparoL", true);
            }
            else
            {
                anim.SetBool("disparoL", false);
            }

            if (numvidas <= 0)
            {
                // usamos valores del GameManager (asegurando que no sea null)
                if (gameManager != null)
                {
                    numvidas = gameManager.vidas;
                    if (gameManager.cancelvideo == true)
                    {
                        // Reemplaza el booleano eliminado 'gameOver' forzando el estado GameOver
                        gameManager.CurrentState = GameState.GameOver;
                        numvidas = 1;
                        gameManager.contador = 30;
                        gameManager.powerupescobabool = false;
                        powerupescoba = false;
                    }
                }
            }

            if (powerupescoba == true)
            {
                anim.SetBool("escoba", true);
                transform.localScale = new Vector2(4f, 4f);
                rb.gravityScale = 0f;
                fuerzaMovimientoDerecha = 0f;
                fuerzaMovimientoIzquierda = 0f;
                rb.velocity = new Vector3(0f, 0f, 0f);
                transform.Translate(JS.JSmove.Horizontal *0.1f, JS.JSmove.Vertical * 0.1f, 0f);
                
            }

            if (gameManager != null && gameManager.powerupescobabool == false && gameManager.powerupbuckbeakbool == false)
            {
                powerupescoba = false;
                powerupbuckbeak = false;
                if (gameManager.audioEscoba != null) gameManager.audioEscoba.SetActive(false);
            }

            if (powerupbuckbeak == true)
            {
                anim.SetBool("buckbeak", true);
                transform.localScale = new Vector2(2.25f, 2.25f);
                rb.gravityScale = 0f;
                fuerzaMovimientoDerecha = 0f;
                fuerzaMovimientoIzquierda = 0f;
                rb.velocity = new Vector3(0f,0f,0f);
                transform.Translate(JS.JSmove.Horizontal*0.15f, JS.JSmove.Vertical* 0.15f, 0f);
                
            }

            if (powerupbuckbeak == false && powerupescoba == false)
            {
                anim.SetBool("buckbeak", false);
                anim.SetBool("escoba", false);
                transform.localScale = new Vector2(5f, 5f);
                rb.gravityScale = 1f;
                fuerzaMovimientoDerecha = 10f;
                fuerzaMovimientoIzquierda = -10f;
            }
        } 
    }
    public void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.CompareTag("Dementor") && powerupbuckbeak == false)
        {
            numvidas--;
            if (gameManager != null)
            {
                gameManager.vidas--;
                if (gameManager.audiovidamenos != null) gameManager.audiovidamenos.SetActive(true);
            }
        }

        else
        {
            if (gameManager != null && gameManager.audiovidamenos != null) gameManager.audiovidamenos.SetActive(false);
        }

        if (collision.gameObject.CompareTag("Vidas") && numvidas < 3)
        {
            numvidas++;
            if (gameManager != null) gameManager.vidas++;
        }

        if(collision.gameObject.CompareTag("Escoba"))
        {
            if (gameManager != null)
            {
                gameManager.powerupescobabool = true;
                powerupescoba = true;
                gameManager.Escoba();
                if (gameManager.audioEscoba != null) gameManager.audioEscoba.SetActive(true);
            }
        }

        if (collision.gameObject.CompareTag("Patronus"))
        {
            if (gameManager != null) gameManager.Patronus();
        }

        if (collision.gameObject.CompareTag("Buckbeak"))
        {
            if (gameManager != null)
            {
                gameManager.powerupbuckbeakbool = true;
                powerupbuckbeak = true;
                if (gameManager.audioEscoba != null) gameManager.audioEscoba.SetActive(true);
                gameManager.Buckbeak();
            }
        }
    }
}
