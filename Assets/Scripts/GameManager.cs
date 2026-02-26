using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public Transform harry;
    public GameObject patronumL;
    public GameObject patronumR;
    public GameObject patronumU;
    public GameObject dementor;
    public GameObject menuOptions;
    public GameObject powerupvida;
    public GameObject powerupescoba;
    public GameObject poweruppatronus;
    public GameObject powerupbuckbeak;
    public GameObject harrypatronus;
    public GameObject patronusgrande;
    public GameObject bJump;
    public GameObject bSpell;

    public Text contadorescoba;

    public Text scoremuerte;
    public Text Hscore;

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
    [SerializeField] int delay = 2000;
    public int highscore;
    public int highS;
    public Vector3 vector;

    [SerializeField] private EnemySpawner spawner;

    private int _score = 0;

    public int score
    {
        get => _score;
        set
        {
            _score = value;
            UpdateDifficulty();
            if (spawner != null) spawner.UpdateSpawnRate(delay);
            // actualizar UI inmediatamente cuando cambie la puntuación
            if (UIManager.Instance != null) UIManager.Instance.UpdateScore(_score);
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

        // Mostrar menú inicial (si existe UIManager)
        if (UIManager.Instance != null) UIManager.Instance.ShowStartMenu();
    }

    public void StartGame()
    {
        start = true;
        if (spawner != null) spawner.StartSpawning(delay);
        if (UIManager.Instance != null) UIManager.Instance.ShowGameplayUI();
    }

    public void Exit()
    {
        exit = true;
    }

    public void Restart()
    {
        if (UIManager.Instance != null) UIManager.Instance.SetMenuVideoActive(false);
        reset = true;
        if (spawner != null) spawner.StartSpawning(delay);
    }

    public void Options()
    {
        if (UIManager.Instance != null) UIManager.Instance.ShowOptions(true);
    }

    public void OptionsBack()
    {
        if (UIManager.Instance != null) UIManager.Instance.ShowOptions(false);
    }

    public void ResetHS()
    {
        PlayerPrefs.SetInt("highscore", 0);
    }

    public void Pause()
    {
        if (UIManager.Instance != null) UIManager.Instance.SetPauseUI(true);
        Time.timeScale = 0f;
        enpausa = true;

        if (spawner != null) spawner.StopSpawning();
    }

    public void Resume()
    {
        if (UIManager.Instance != null) UIManager.Instance.SetPauseUI(false);
        if (UIManager.Instance != null) UIManager.Instance.SetButtonsActive(true);
        Time.timeScale = 1f;
        enpausa = false;

        if (spawner != null) spawner.StartSpawning(delay);
    }

    public void Home()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Escoba()
    {
        if (UIManager.Instance != null) UIManager.Instance.SetCounterActive(true);
        if (UIManager.Instance != null) UIManager.Instance.UpdateCounterText($"{contador}");
        StartCoroutine(EscobaTimer());
    }

    public void Buckbeak()
    {
        if (UIManager.Instance != null) UIManager.Instance.SetCounterActive(true);
        if (UIManager.Instance != null) UIManager.Instance.UpdateCounterText($"{contadorbuckbeak}");
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
        if (UIManager.Instance != null) UIManager.Instance.SetMenuVideoActive(false);
        if (UIManager.Instance != null) UIManager.Instance.SetPlayerActive(true);
        if (UIManager.Instance != null) UIManager.Instance.SetMenuUIActive(true);
    }

    public void VideoReward()
    {
        if (UIManager.Instance != null) UIManager.Instance.SetMenuVideoActive(false);
        if (UIManager.Instance != null) UIManager.Instance.SetPlayerActive(true);
        if (UIManager.Instance != null) UIManager.Instance.SetMenuUIActive(true);
        Resume();
        repeatvideo = true;
        vidas = 1;
        harry.SetPositionAndRotation(vector, Quaternion.identity);
    }

    //Velocidad de aparicion dementores
    void UpdateDifficulty()
    {
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
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            StartGame();
        }

        if (gameOver == true)
        {
            // Delegamos toda la UI al UIManager
            PatronusExit();

            buttonJump = false;
            buttonSpell = false;

            Up = false;
            animHarry.SetBool("up", false);
            audioPatronus.volume = 0;

            repeatvideo = false;

            if (score > highS)
            {
                highS = score;
                PlayerPrefs.SetInt("highscore", score);
            }

            if (UIManager.Instance != null) UIManager.Instance.ShowGameOver(score, highS);

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Home();
            }

            scoremuerte = GameObject.FindGameObjectWithTag("ScoreMuerte").GetComponent<Text>();
            scoremuerte.text = $"{score}";

            if (reset == true || Input.GetKeyDown(KeyCode.Return))
            {
                score = 0;
                vidas = 1;
                if (UIManager.Instance != null) UIManager.Instance.ShowGameplayUI();
                if (UIManager.Instance != null) UIManager.Instance.SetCounterActive(false);
                if (UIManager.Instance != null) UIManager.Instance.SetMenuVideoActive(false);
                if (UIManager.Instance != null) UIManager.Instance.SetPlayerActive(true);
                if (UIManager.Instance != null) UIManager.Instance.SetButtonsActive(true);

                cancelvideo = false;
                enpausa = false;
                start = true;
                gameOver = false;
                if (spawner != null) spawner.StartSpawning(delay);
                harry.SetPositionAndRotation(vector, Quaternion.identity);
                delay = 2000;
                contador = 20;
                powerupescobabool = false;
            }
        }

        if (enpausa == true)
        {
            // input handling sigue aquí
            if (Input.GetKeyDown(KeyCode.Return))
            {
                Resume();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
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
            // Delegar UI a UIManager
            if (UIManager.Instance != null) UIManager.Instance.ShowGameplayUI();
            // actualizar score en UI
            if (UIManager.Instance != null) UIManager.Instance.UpdateScore(score);

            reset = false;

            if (Input.GetKeyDown(KeyCode.Escape))
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

            //Powerups...
            if (score % 10 == 0 && ActPowerUp == 0 && score != 0)
            {
                ActPowerUp++;

                if (!powerupbuckbeakbool)
                {
                    if (!powerupescobabool)
                    {
                        if (superpatronus)
                        {
                            byte rnd = (byte)Random.Range(0, 2.99f);

                            if (rnd == 0) Instantiate(powerupvida, new Vector3(Random.Range(-9, 9), 6.5f, 0), Quaternion.identity);
                            if (rnd == 1) Instantiate(powerupescoba, new Vector3(Random.Range(-9, 9), 6.5f, 0), Quaternion.identity);
                            if (rnd == 2) Instantiate(powerupbuckbeak, new Vector3(Random.Range(-9, 9), 6.5f, 0), Quaternion.identity);
                        }
                        else
                        {
                            byte rnd = (byte)Random.Range(0, 3.99f);
                            if (rnd == 0) Instantiate(powerupvida, new Vector3(Random.Range(-9, 9), 6.5f, 0), Quaternion.identity);
                            if (rnd == 1) Instantiate(powerupescoba, new Vector3(Random.Range(-9, 9), 6.5f, 0), Quaternion.identity);
                            if (rnd == 2) Instantiate(poweruppatronus, new Vector3(Random.Range(-9, 9), 6.5f, 0), Quaternion.identity);
                            if (rnd == 3) Instantiate(powerupbuckbeak, new Vector3(Random.Range(-9, 9), 6.5f, 0), Quaternion.identity);
                        }
                    }
                    else // powerupescobabool == true
                    {
                        if (superpatronus) Instantiate(powerupvida, new Vector3(Random.Range(-9, 9), 6.5f, 0), Quaternion.identity);
                        else
                        {
                            byte rnd = (byte)Random.Range(0, 1.99f);
                            if (rnd == 0) Instantiate(powerupvida, new Vector3(Random.Range(-9, 9), 6.5f, 0), Quaternion.identity);
                            if (rnd == 1) Instantiate(poweruppatronus, new Vector3(Random.Range(-9, 9), 6.5f, 0), Quaternion.identity);
                        }
                    }
                }

                if (powerupbuckbeakbool)
                {
                    if (superpatronus) Instantiate(powerupvida, new Vector3(Random.Range(-9, 9), 6.5f, 0), Quaternion.identity);
                    else
                    {
                        byte rnd = (byte)Random.Range(0, 1.99f);
                        if (rnd == 0) Instantiate(powerupvida, new Vector3(Random.Range(-9, 9), 6.5f, 0), Quaternion.identity);
                        if (rnd == 1) Instantiate(poweruppatronus, new Vector3(Random.Range(-9, 9), 6.5f, 0), Quaternion.identity);
                    }
                }
            }

            // Counter UI
            if (powerupescobabool)
            {
                if (UIManager.Instance != null) UIManager.Instance.UpdateCounterText($"{contador}");
                if (UIManager.Instance != null) UIManager.Instance.SetJumpButtonActive(false);
            }
            else
            {
                if (UIManager.Instance != null) UIManager.Instance.SetJumpButtonActive(true);
            }

            if (contador <= 0)
            {
                powerupescobabool = false;
                if (UIManager.Instance != null) UIManager.Instance.SetCounterActive(false);
                contador = 20;
            }

            if (powerupbuckbeakbool)
            {
                if (UIManager.Instance != null) UIManager.Instance.UpdateCounterText($"{contadorbuckbeak}");
                if (UIManager.Instance != null) UIManager.Instance.SetJumpButtonActive(false);
                if (UIManager.Instance != null) UIManager.Instance.SetSpellButtonActive(false);
            }
            if (!powerupbuckbeakbool && !powerupescobabool)
            {
                if (UIManager.Instance != null) UIManager.Instance.SetJumpButtonActive(true);
                if (UIManager.Instance != null) UIManager.Instance.SetSpellButtonActive(true);
            }

            if (contadorbuckbeak <= 0)
            {
                powerupbuckbeakbool = false;
                if (UIManager.Instance != null) UIManager.Instance.SetCounterActive(false);
                contadorbuckbeak = 15;
            }

            if (score % 10 != 0)
            {
                ActPowerUp = 0;
            }

            if (!powerupescobabool)
            {
                if ((Input.GetKeyDown(KeyCode.Z)) && goLeft == true && Up == false) Instantiate(patronumL, harry.transform.position + new Vector3(-0.6f, -0.1f, 0), Quaternion.identity);
                if ((Input.GetKeyDown(KeyCode.Z)) && goRight == true && Up == false) Instantiate(patronumR, harry.transform.position + new Vector3(0.6f, -0.1f, 0), Quaternion.identity);
                if ((Input.GetKeyDown(KeyCode.Z)) && goRight == true && Up == true && salto == false) Instantiate(patronumU, harry.transform.position + new Vector3(0.2f, 0.7f, 0), Quaternion.identity);
                if ((Input.GetKeyDown(KeyCode.Z)) && goLeft == true && Up == true && salto == false) Instantiate(patronumU, harry.transform.position + new Vector3(-0.2f, 0.7f, 0), Quaternion.identity);
            }
            else
            {
                if ((Input.GetKeyDown(KeyCode.Z)) && goLeft == true) Instantiate(patronumL, harry.transform.position + new Vector3(-1.6f, 0.1f, 0), Quaternion.identity);
                if ((Input.GetKeyDown(KeyCode.Z)) && goRight == true) Instantiate(patronumR, harry.transform.position + new Vector3(1.6f, 0.1f, 0), Quaternion.identity);
            }

            // Vidas via UIManager
            if (UIManager.Instance != null) UIManager.Instance.UpdateLives(vidas);

            if (vidas != 0)
            {
                if (vidas > 3) vidas = 3;
            }

            if (vidas <= 0 && repeatvideo == false)
            {
                // Si el jugador ya pulsó "Cancel", no reabrimos el menú de video;
                // en su lugar vamos directamente a GameOver.
                if (cancelvideo)
                {
                    gameOver = true;
                }
                else
                {
                    enpausa = true;
                    if (UIManager.Instance != null) UIManager.Instance.SetPlayerActive(false);
                    if (UIManager.Instance != null) UIManager.Instance.SetMenuVideoActive(true);
                    if (UIManager.Instance != null) UIManager.Instance.SetMenuUIActive(false);
                    if (spawner != null) spawner.StopSpawning();
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

        while (contador > 5)
        {
            yield return new WaitForSeconds(1);
            contador--;
        }

        while (contador <= 5 && contador >= 0)
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