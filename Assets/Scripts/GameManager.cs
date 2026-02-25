using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Advertisements;

public class GameManager : MonoBehaviour
{
    public Transform harry;
    public GameObject patronumL;
    public GameObject patronumR;
    public GameObject patronumU;
    public GameObject dementor;
    public GameObject menuInicio;
    public GameObject menuGameOver;
    public GameObject menuOptions;
    public GameObject menuPause;
    public GameObject menuUI;
    public GameObject SCORE;
    public GameObject BY;
    public GameObject enjuego;
    public GameObject enmenus;
    public GameObject powerupvida;
    public GameObject powerupescoba;
    public GameObject poweruppatronus;
    public GameObject powerupbuckbeak;
    public GameObject counterescoba;
    public GameObject harrypatronus;
    public GameObject patronusgrande;
    public GameObject buttonsUI;
    public GameObject bJump;
    public GameObject bSpell;
    public GameObject MenuVideo;
    public GameObject player;

    public Text contadorescoba;

    private Text Kcount;
    public Text scoremuerte;
    public Text Hscore;

    public GameObject vida1;
    public GameObject vida2;
    public GameObject vida3;

    public AudioSource audioPatronus;

    public GameObject audiovidamenos;
    public GameObject audioEscoba;
    public Animator animHarry;
    public Rigidbody2D rbHarry;
    public SpriteRenderer RendHarry;

    public Controller JS;

    public GameObject audiohit;

    public bool start = false;
    public bool gameOver = false;
    public bool exit = false;
    public bool reset = false;
    public bool enpausa = false;
    public bool powerupescobabool = false;
    public bool powerupbuckbeakbool = false;
    public bool cancelvideo = false;
    public bool repeatvideo = false;
    public bool superpatronus = false;

    bool goLeft = true;
    bool goRight = false;
    bool Up = false;
    bool salto = false;

    public bool buttonJump = false;
    public bool buttonSpell = false;

    public byte ActDemen = 0;
    public int ActPowerUp = 0;
    int delay = 2000;
    public int highscore;
    public int highS;
    public Vector3 vector;

    private int _score = 0;

    public int score
    {
        get => _score;
        set
        {
            _score = value;
            UpdateDifficulty();
            
        }

    }


    public int vidas = 1;
    public float contador = 20.0f;
    public float contadorbuckbeak = 15.0f;
    public float contadorpatronus = 10.0f;
    public float transparenteHarry = 1;

    // Tabla de dificultad: umbral -> valor de delay (ms)
    private readonly (int threshold, int delay)[] difficultySteps = new (int, int)[]
    {
        (100, 113),
        (90, 150),
        (80, 200),
        (70, 267),
        (60, 356),
        (50, 475),
        (40, 633),
        (30, 844),
        (20, 1125),
        (10, 1500)
    };

    public void Start()
    {
        animHarry = GameObject.FindGameObjectWithTag("Harry").GetComponent<Animator>();
        rbHarry = GameObject.FindGameObjectWithTag("Harry").GetComponent<Rigidbody2D>();
        RendHarry = GameObject.FindGameObjectWithTag("Harry").GetComponent<SpriteRenderer>();

        highscore = PlayerPrefs.GetInt("highscore", 0);
        highS = highscore;

        Time.timeScale = 1f;
    }

    public void StartGame()
    {
        start = true;
    }

    public void Exit()
    {
        exit = true;
    }

    public void Restart()
    {
        MenuVideo.SetActive(false);
        reset = true;

    }

    public void Options()
    {
        menuOptions.SetActive(true);
        menuInicio.SetActive(false);
    }

    public void OptionsBack()
    {
        menuOptions.SetActive(false);
        menuInicio.SetActive(true);
    }

    public void ResetHS()
    {
        PlayerPrefs.SetInt("highscore", 0);
    }

    public void Pause()
    {
        menuPause.SetActive(true);
        menuUI.SetActive(false);
        Time.timeScale = 0f;
        enpausa = true;
    }

    public void Resume()
    {
        menuPause.SetActive(false);
        menuUI.SetActive(true);
        buttonsUI.SetActive(true);
        Time.timeScale = 1f;
        enpausa = false;
    }

    public void Home()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Escoba()
    {
        counterescoba.SetActive(true);
        contadorescoba.enabled = true;
        contadorescoba.text = $"{contador}";
        StartCoroutine(EscobaTimer());
    }

    public void Buckbeak()
    {
        counterescoba.SetActive(true);
        contadorescoba.enabled = true;
        contadorescoba.text = $"{contadorbuckbeak}";
        StartCoroutine(BuckbeakTimer());
    }
    
    public void Patronus()
    {
        harrypatronus.SetActive(true);
        StartCoroutine(PatronusTimer());
        audioEscoba.SetActive(true);
        audioPatronus.volume = 0.75f;
        superpatronus = true;
    }

    public void PatronusExit()
    {
        harrypatronus.SetActive(false);
        audioEscoba.SetActive(false);
        superpatronus = false;
        contadorpatronus = 10.0f;
    }

    public void Salto()
    {
        buttonJump = true;
        if (salto == false && powerupescobabool == false)
        {
            rbHarry.AddForce(new Vector2(0, 400));
        }
        
    }

    public void saltoNot()
    {
        buttonJump = false;
    }

    public void Hechizo()
    {
        buttonSpell = true;

        if (powerupescobabool == false && powerupbuckbeakbool == false)
        {
            //Disparo a la izquierda
            if (buttonSpell == true && goLeft == true && Up == false)
            {
                Instantiate(patronumL, harry.transform.position + new Vector3(-0.6f, -0.1f, 0), Quaternion.identity);
            }

            //Disparo a la derecha
            if (buttonSpell == true && goRight == true && Up == false)
            {
                Instantiate(patronumR, harry.transform.position + new Vector3(0.6f, -0.1f, 0), Quaternion.identity);
            }

            //Disparo arriba por la derecha
            if (buttonSpell == true && goRight == true && Up == true && salto == false)
            {
                Instantiate(patronumU, harry.transform.position + new Vector3(0.2f, 0.7f, 0), Quaternion.identity);
            }

            //Disparo arriba a la izquierda
            if (buttonSpell == true && goLeft == true && Up == true && salto == false)
            {
                Instantiate(patronumU, harry.transform.position + new Vector3(-0.2f, 0.7f, 0), Quaternion.identity);
            }
        }

        if (powerupescobabool == true)
        {
            //Disparo a la izquierda
            if (buttonSpell == true && goLeft == true && Up == false)
            {
                Instantiate(patronumL, harry.transform.position + new Vector3(-1.6f, 0.1f, 0), Quaternion.identity);
            }

            //Disparo a la derecha
            if (buttonSpell == true && goRight == true && Up == false)
            {
                Instantiate(patronumR, harry.transform.position + new Vector3(1.6f, 0.1f, 0), Quaternion.identity);
            }
        }
    }

    public void hechizoNot()
    {
        buttonSpell = false;
    }

    public void CancelVideo()
    {
        cancelvideo = true;
        MenuVideo.SetActive(false);
        player.SetActive(true);
        menuUI.SetActive(true);
    }

    public void VideoReward()
    {
        MenuVideo.SetActive(false);
        player.SetActive(true);
        menuUI.SetActive(true);
        Resume();
        repeatvideo = true;
        vidas = 1;
        harry.SetPositionAndRotation(vector, Quaternion.identity);
    }

    //Velocidad de aparicion dementores
    void UpdateDifficulty()
    {
        // valor por defecto si no se alcanza ningún umbral
        delay = 2000;

        foreach (var step in difficultySteps)
        {
            if (score > step.threshold)
            {
                delay = step.delay;
                break;
            }
        }
    }

    // Update is called once per frame
    public async void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            StartGame();
        }

        
        if (gameOver == true)
        {
            enjuego.SetActive(false);
            menuUI.SetActive(false);
            menuGameOver.SetActive(true);
            PatronusExit();

            buttonJump = false;
            buttonSpell = false;

            Up = false;
            animHarry.SetBool("up", false);
            audioPatronus.volume = 0;

            repeatvideo = false;

            if (score > highS)
            {
                Hscore = GameObject.FindGameObjectWithTag("HS").GetComponent<Text>();
                Hscore.text = $"{score}";
                PlayerPrefs.SetInt("highscore", score);
            }
            
            else
            {
                Hscore = GameObject.FindGameObjectWithTag("HS").GetComponent<Text>();
                Hscore.text = $"{highS}";
            } 

            if(Input.GetKeyDown(KeyCode.Escape))
            {
                Home();
            }

            scoremuerte = GameObject.FindGameObjectWithTag("ScoreMuerte").GetComponent<Text>();
            scoremuerte.text = $"{score}";

            if (reset == true || Input.GetKeyDown(KeyCode.Return))
            {
                score = 0;
                vidas = 1;
                enjuego.SetActive(true);
                counterescoba.SetActive(false);
                MenuVideo.SetActive(false);
                player.SetActive(true);
                buttonsUI.SetActive(true);
                cancelvideo = false;
                enpausa = false;
                start = true;
                gameOver = false;
                menuGameOver.SetActive(false);
                harry.SetPositionAndRotation(vector, Quaternion.identity);
                delay = 2000;
                contador = 20;
                powerupescobabool = false;
            }
        }

        if (enpausa == true)
        {
            menuUI.SetActive(false);
            buttonsUI.SetActive(false);

            if(Input.GetKeyDown(KeyCode.Return))
            {
                Resume();
            }

            if(Input.GetKeyDown(KeyCode.Escape))
            {
                Home();
            }
        }

        if (exit == true || Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (harry.transform.position.y > 0)
        {
            salto = true;
        }

        if (harry.transform.position.y < 0)
        {
            salto = false;
        }

        if (start == true && gameOver == false)
        {

            menuInicio.SetActive(false);
            enmenus.SetActive(false);
            SCORE.SetActive(true);
            enjuego.SetActive(true);
            menuUI.SetActive(true);
            reset = false;
            Kcount = GameObject.FindGameObjectWithTag("Score").GetComponent<Text>();

            Kcount.text = $"Score: {score}";
            
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                Pause();
            }

            // Defino si va para la izquierda o derecha
            if (Input.GetKey(KeyCode.LeftArrow) || JS.JSmove.Horizontal < 0)
            {
                goLeft = true;
                goRight = false;
            }

            if (Input.GetKey(KeyCode.RightArrow) || JS.JSmove.Horizontal > 0)
            {
                goRight = true;
                goLeft = false;
            }

            if ((Input.GetKey(KeyCode.UpArrow) || JS.JSmove.Vertical > 0.95f) && harry.transform.position.y < 0)
            {
                Up = true;
                animHarry.SetBool("up", true);
            }

            if (Input.GetKeyUp(KeyCode.UpArrow) || JS.JSmove.Vertical < 0.95f || harry.transform.position.y > 0)
            {
                Up = false;
                animHarry.SetBool("up", false);
            }

            //Powerups con diferentes condiciones para que no aparezcan al tiempo en que estan siendo usados

            if (score % 10 == 0 && ActPowerUp == 0 && score != 0)
            {
                ActPowerUp++;

                if (powerupbuckbeakbool == false)
                {
                    if (powerupescobabool == false)
                    {
                        if (superpatronus == true)
                        {
                            byte rnd = (byte)Random.Range(0, 2.99f);

                            if (rnd == 0)
                            {
                                Instantiate(powerupvida, new Vector3(Random.Range(-9, 9), 6.5f, 0), Quaternion.identity);
                            }

                            if (rnd == 1)
                            {
                                Instantiate(powerupescoba, new Vector3(Random.Range(-9, 9), 6.5f, 0), Quaternion.identity);
                            }

                            if (rnd == 2)
                            {
                                Instantiate(powerupbuckbeak, new Vector3(Random.Range(-9, 9), 6.5f, 0), Quaternion.identity);
                            }
                        }

                        else
                        {
                            byte rnd = (byte)Random.Range(0, 3.99f);

                            if (rnd == 0)
                            {
                                Instantiate(powerupvida, new Vector3(Random.Range(-9, 9), 6.5f, 0), Quaternion.identity);
                            }

                            if (rnd == 1)
                            {
                                Instantiate(powerupescoba, new Vector3(Random.Range(-9, 9), 6.5f, 0), Quaternion.identity);
                            }

                            if (rnd == 2)
                            {
                                Instantiate(poweruppatronus, new Vector3(Random.Range(-9, 9), 6.5f, 0), Quaternion.identity);
                            }

                            if (rnd == 3)
                            {
                                Instantiate(powerupbuckbeak, new Vector3(Random.Range(-9, 9), 6.5f, 0), Quaternion.identity);
                            }
                        }

                    }

                    if (powerupescobabool == true)
                    {
                        if (superpatronus == true)
                        {
                            Instantiate(powerupvida, new Vector3(Random.Range(-9, 9), 6.5f, 0), Quaternion.identity);
                        }

                        else
                        {
                            byte rnd = (byte)Random.Range(0, 1.99f);

                            if (rnd == 0)
                            {
                                Instantiate(powerupvida, new Vector3(Random.Range(-9, 9), 6.5f, 0), Quaternion.identity);
                            }

                            if (rnd == 1)
                            {
                                Instantiate(poweruppatronus, new Vector3(Random.Range(-9, 9), 6.5f, 0), Quaternion.identity);
                            }
                        }
                    }
                }

                if (powerupbuckbeakbool == true)
                {
                    if (superpatronus == true)
                    {
                        Instantiate(powerupvida, new Vector3(Random.Range(-9, 9), 6.5f, 0), Quaternion.identity);
                    }

                    else
                    {
                        byte rnd = (byte)Random.Range(0, 1.99f);

                        if (rnd == 0)
                        {
                            Instantiate(powerupvida, new Vector3(Random.Range(-9, 9), 6.5f, 0), Quaternion.identity);
                        }

                        if (rnd == 1)
                        {
                            Instantiate(poweruppatronus, new Vector3(Random.Range(-9, 9), 6.5f, 0), Quaternion.identity);
                        }

                    }
                }
            }

            if (powerupescobabool == true)
            {
                contadorescoba.text = $"{contador}";
                bJump.SetActive(false);
            }
            else
            {
                bJump.SetActive(true);
            }

            if (contador <= 0)
            {
                powerupescobabool = false;
                contadorescoba.enabled = false;
                counterescoba.SetActive(false);
                contador = 20;
            }

            if (powerupbuckbeakbool == true)
            {
                contadorescoba.text = $"{contadorbuckbeak}";
                bJump.SetActive(false);
                bSpell.SetActive(false);
            }
            if (powerupbuckbeakbool == false && powerupescobabool == false)
            {
                bJump.SetActive(true);
                bSpell.SetActive(true);
            }

            if (contadorbuckbeak <= 0)
            {
                powerupbuckbeakbool = false;
                contadorescoba.enabled = false;
                counterescoba.SetActive(false);
                contadorbuckbeak = 15;
            }

            if (score % 10 != 0)
            {
                ActPowerUp = 0;
            }

            if (powerupescobabool == false)
            {
                //Disparo a la izquierda
                if ((Input.GetKeyDown(KeyCode.Z)) && goLeft == true && Up == false)
                {
                    Instantiate(patronumL, harry.transform.position + new Vector3(-0.6f, -0.1f, 0), Quaternion.identity);
                }

                //Disparo a la derecha
                if ((Input.GetKeyDown(KeyCode.Z)) && goRight == true && Up == false)
                {
                    Instantiate(patronumR, harry.transform.position + new Vector3(0.6f, -0.1f, 0), Quaternion.identity);
                }

                //Disparo arriba por la derecha
                if ((Input.GetKeyDown(KeyCode.Z)) && goRight == true && Up == true && salto == false)
                {
                    Instantiate(patronumU, harry.transform.position + new Vector3(0.2f, 0.7f, 0), Quaternion.identity);
                }

                //Disparo arriba a la izquierda
                if ((Input.GetKeyDown(KeyCode.Z)) && goLeft == true && Up == true && salto == false)
                {
                    Instantiate(patronumU, harry.transform.position + new Vector3(-0.2f, 0.7f, 0), Quaternion.identity);
                }
            }

            if (powerupescobabool == true)
            {
                //Disparo a la izquierda
                if ((Input.GetKeyDown(KeyCode.Z)) && goLeft == true)
                {
                    Instantiate(patronumL, harry.transform.position + new Vector3(-1.6f, 0.1f, 0), Quaternion.identity);
                }

                //Disparo a la derecha
                if ((Input.GetKeyDown(KeyCode.Z)) && goRight == true)
                {
                    Instantiate(patronumR, harry.transform.position + new Vector3(1.6f, 0.1f, 0), Quaternion.identity);
                }
            }
            //Crear dementores
            if (ActDemen == 0 && enpausa == false)
            {
                byte rnd = (byte)Random.Range(0, 1.99f);

                if (rnd == 0)
                {
                    float randomDM = Random.Range(-10, harry.transform.position.x - 3);
                    float randomN = Random.Range(0.05f, 5);
                    Instantiate(dementor, new Vector3(randomDM, randomN, 0), Quaternion.identity);
                    ActDemen = (byte)(ActDemen + 1);
                    await Task.Delay(delay);
                    ActDemen = (byte)(ActDemen - 1);
                }

                if (rnd == 1)
                {
                    float randomDM = Random.Range(harry.transform.position.x + 3, 10);
                    float randomN = Random.Range(0.05f, 5);
                    Instantiate(dementor, new Vector3(randomDM, randomN, 0), Quaternion.identity);
                    ActDemen = (byte)(ActDemen + 1);
                    await Task.Delay(delay);
                    ActDemen = (byte)(ActDemen - 1);
                }
            }

            if (vidas != 0)
            {
                if (vidas >= 1)
                {
                    vida1.SetActive(true);
                }

                else
                {
                    vida1.SetActive(false);
                }

                if (vidas >= 2)
                {
                    vida2.SetActive(true);
                }

                else
                {
                    vida2.SetActive(false);
                }

                if (vidas >= 3)
                {
                    vida3.SetActive(true);
                }

                else
                {
                    vida3.SetActive(false);
                }

                if (vidas > 3)
                {
                    vidas = 3;
                }
            }

            else
            {
                vida1.SetActive(false);
            }

            if (vidas <= 0 && repeatvideo == false)
            {
                enpausa = true;
                player.SetActive(false);
                MenuVideo.SetActive(true);
                menuUI.SetActive(false);
                if (cancelvideo)
                {
                    gameOver = true;
                }
            }

            if (vidas <= 0 && repeatvideo == true)
            {
                gameOver = true;
            }
        }
    }

    IEnumerator EscobaTimer()
    {

        while(contador > 5)
        {
            yield return new WaitForSeconds(1);
            contador--;
        }

        while(contador <= 5 && contador >= 0)
        {
            RendHarry.color = new Color(1f, 1f, 1f, 0.5f);
            yield return new WaitForSeconds(0.5f);
            RendHarry.color = new Color(1f, 1f, 1f, 1f);
            yield return new WaitForSeconds(0.5f);
            contador--;
        }

    }

    IEnumerator BuckbeakTimer()
    {
        while (contadorbuckbeak > 5)
        {
            yield return new WaitForSeconds(1);
            contadorbuckbeak--;
        }

        while (contadorbuckbeak <= 5 && contadorbuckbeak >= 0)
        {
            RendHarry.color = new Color(1f, 1f, 1f, 0.5f);
            yield return new WaitForSeconds(0.5f);
            RendHarry.color = new Color(1f, 1f, 1f, 1f);
            yield return new WaitForSeconds(0.5f);
            contadorbuckbeak--;
        }
    }
    IEnumerator PatronusTimer()
    {
        while (contadorpatronus >= 0)
        {
            yield return new WaitForSeconds(1);
            Instantiate(patronusgrande, new Vector3(-6, 2.6f, 0), Quaternion.identity);
            contadorpatronus--;
        }
        PatronusExit();
    }
}