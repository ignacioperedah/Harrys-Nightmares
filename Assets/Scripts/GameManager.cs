using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
public enum GameState
{
    Menu,
    Playing,
    Paused,
    GameOver
}
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
    // Removed: start, gameOver, enpausa booleans
    public bool exit = false;
    public bool reset = false;
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
            // Actualizaciones relacionadas con score
            UpdateDifficulty();
            if (spawner != null) spawner.UpdateSpawnRate(delay);
            if (UIManager.Instance != null) UIManager.Instance.UpdateScore(_score);
            // Spawn de powerup cuando score es múltiplo de 10 (y no 0).
            if (_score != 0 && _score % 10 == 0)
            {
                if (ActPowerUp == 0)
                {
                    ActPowerUp = 1;
                    SpawnPowerup();
                }
            }
            else
            {
                // Permitimos spawn en el siguiente múltiplo de 10
                ActPowerUp = 0;
            }
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
    // FSM state
    private GameState _currentState = GameState.Menu;
    public GameState CurrentState
    {
        get => _currentState;
        set
        {
            if (_currentState == value) return;
            _currentState = value;
            OnEnterState(_currentState);
        }
    }
    // Indica que la pausa actual es por mostrar el MenuVideo (no por menú pausa normal)
    private bool _pausedForVideo = false;
    void OnEnterState(GameState state)
    {
        switch (state)
        {
            case GameState.Menu:
                Time.timeScale = 1f;
                if (UIManager.Instance != null) UIManager.Instance.ShowStartMenu();
                if (spawner != null) spawner.StopSpawning();
                _pausedForVideo = false;
                break;
            case GameState.Playing:
                Time.timeScale = 1f;
                // Aseguramos que cualquier estado de pausa se cierre antes de mostrar la UI de juego
                if (UIManager.Instance != null) UIManager.Instance.SetPauseUI(false);
                if (UIManager.Instance != null) UIManager.Instance.ShowGameplayUI();
                if (spawner != null) spawner.StartSpawning(delay);
                _pausedForVideo = false;
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                if (_pausedForVideo)
                {
                    // Pausa provocada por menú de vídeo: mostrar sólo el MenuVideo
                    if (UIManager.Instance != null) UIManager.Instance.SetPlayerActive(false);
                    if (UIManager.Instance != null) UIManager.Instance.SetMenuVideoActive(true);
                    if (UIManager.Instance != null) UIManager.Instance.SetMenuUIActive(false);
                    if (spawner != null) spawner.StopSpawning();
                }
                else
                {
                    // Pausa normal: menú de pausa tradicional
                    if (UIManager.Instance != null) UIManager.Instance.SetPauseUI(true);
                    if (spawner != null) spawner.StopSpawning();
                }
                break;
            case GameState.GameOver:
                // limpiar estado de juego, mostrar GameOver y detener spawner
                Time.timeScale = 1f;
                PatronusExit();
                buttonJump = false;
                buttonSpell = false;
                Up = false;
                if (animHarry != null) animHarry.SetBool("up", false);
                if (audioPatronus != null) audioPatronus.volume = 0;
                repeatvideo = false;
                if (score > highS)
                {
                    highS = score;
                    PlayerPrefs.SetInt("highscore", score);
                }
                if (UIManager.Instance != null) UIManager.Instance.ShowGameOver(score, highS);
                if (spawner != null) spawner.StopSpawning();
                _pausedForVideo = false;
                break;
        }
    }
    public void Start()
    {
        animHarry = GameObject.FindGameObjectWithTag("Harry").GetComponent<Animator>();
        rbHarry = GameObject.FindGameObjectWithTag("Harry").GetComponent<Rigidbody2D>();
        RendHarry = GameObject.FindGameObjectWithTag("Harry").GetComponent<SpriteRenderer>();
        highscore = PlayerPrefs.GetInt("highscore", 0);
        highS = highscore;
        Time.timeScale = 1f;
        // Estado inicial
        CurrentState = GameState.Menu;
    }
    // API: cambiar estados mediante métodos
    public void StartGame()
    {
        CurrentState = GameState.Playing;
    }
    public void Exit()
    {
        exit = true;
    }
    public void Restart()
    {
        // Reinicio inmediato: ocultar menú de vídeo y pasar a Playing
        if (UIManager.Instance != null) UIManager.Instance.SetMenuVideoActive(false);
        // Restaurar variables de reinicio
        score = 0;
        vidas = 1;
        if (UIManager.Instance != null) UIManager.Instance.SetCounterActive(false);
        if (UIManager.Instance != null) UIManager.Instance.SetButtonsActive(true);
        if (UIManager.Instance != null) UIManager.Instance.SetPlayerActive(true);
        cancelvideo = false;
        repeatvideo = false;
        // Posicionar jugador
        if (harry != null) harry.SetPositionAndRotation(vector, Quaternion.identity);
        delay = 2000;
        contador = 20;
        powerupescobabool = false;
        if (spawner != null) spawner.StartSpawning(delay);
        CurrentState = GameState.Playing;
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
        highS = 0;
    }
    public void Pause()
    {
        // pausa normal (menú pausa)
        _pausedForVideo = false;
        CurrentState = GameState.Paused;
    }
    public void Resume()
    {
        CurrentState = GameState.Playing;
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
        if (harrypatronus != null) harrypatronus.SetActive(true);
        StartCoroutine(PatronusTimer());
        if (audioEscoba != null) audioEscoba.SetActive(true);
        if (audioPatronus != null) audioPatronus.volume = 0.75f;
        superpatronus = true;
    }
    public void PatronusExit()
    {
        if (harrypatronus != null) harrypatronus.SetActive(false);
        if (audioEscoba != null) audioEscoba.SetActive(false);
        superpatronus = false;
        contadorpatronus = 10.0f;
    }
    public void Salto()
    {
        // evitar saltar si estamos en el aire: comprobación directa de Y
        buttonJump = true;
        if (harry != null && harry.position.y < 0 && powerupescobabool == false)
        {
            if (rbHarry != null) rbHarry.AddForce(new Vector2(0, 400));
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
            if (buttonSpell && goLeft && !Up) Instantiate(patronumL, harry.position + new Vector3(-0.6f, -0.1f, 0), Quaternion.identity);
            if (buttonSpell && goRight && !Up) Instantiate(patronumR, harry.position + new Vector3(0.6f, -0.1f, 0), Quaternion.identity);
            if (buttonSpell && goRight && Up && !salto) Instantiate(patronumU, harry.position + new Vector3(0.2f, 0.7f, 0), Quaternion.identity);
            if (buttonSpell && goLeft && Up && !salto) Instantiate(patronumU, harry.position + new Vector3(-0.2f, 0.7f, 0), Quaternion.identity);
        }
        if (powerupescobabool)
        {
            if (buttonSpell && goLeft) Instantiate(patronumL, harry.position + new Vector3(-1.6f, 0.1f, 0), Quaternion.identity);
            if (buttonSpell && goRight) Instantiate(patronumR, harry.position + new Vector3(1.6f, 0.1f, 0), Quaternion.identity);
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
        // ir directamente a GameOver al cancelar el video de continuidad
        CurrentState = GameState.GameOver;
    }
    public void VideoReward()
    {
        // Volver desde menuVideo: asegurar orientación a la izquierda antes de jugar
        if (UIManager.Instance != null) UIManager.Instance.SetMenuVideoActive(false);
        if (UIManager.Instance != null) UIManager.Instance.SetPlayerActive(true);
        if (UIManager.Instance != null) UIManager.Instance.SetMenuUIActive(true);
        repeatvideo = true;
        vidas = 1;
        // forzar orientación inicial: disparo hacia la izquierda
        goLeft = true;
        goRight = false;
        if (animHarry != null) animHarry.SetBool("goLeft", true);
        if (harry != null) harry.SetPositionAndRotation(vector, Quaternion.identity);
        // salir del modo "pausa por video"
        _pausedForVideo = false;
        CurrentState = GameState.Playing;
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
    // Spawn aleatorio de powerup (vida, escoba, patronus, buckbeak)
    private void SpawnPowerup()
    {
        // 0=vida, 1=escoba, 2=patronus, 3=buckbeak
        var candidates = new System.Collections.Generic.List<int>();
        // Vida siempre permitida (aunque jugador tenga ya 3 vidas).
        candidates.Add(0);
        // Si cualquiera de los powerups de "movimiento" está activo,
        // no permitimos generar ni Escoba ni Buckbeak.
        bool movementPowerupActive = powerupescobabool || powerupbuckbeakbool;
        if (!movementPowerupActive)
        {
            candidates.Add(1); // escoba
            candidates.Add(3); // buckbeak
        }
        // Si no hay Patronus activo, permitir Patronus
        if (!superpatronus)
        {
            candidates.Add(2); // patronus
        }
        // Seguridad: aseguramos al menos una opción
        if (candidates.Count == 0)
        {
            candidates.Add(0);
        }
        int choice = candidates[Random.Range(0, candidates.Count)];
        float x = Random.Range(-9f, 9f);
        Vector3 pos = new Vector3(x, 6.5f, 0);
        switch (choice)
        {
            case 0: // vida
                if (powerupvida != null) Instantiate(powerupvida, pos, Quaternion.identity);
                break;
            case 1: // escoba
                if (powerupescoba != null) Instantiate(powerupescoba, pos, Quaternion.identity);
                break;
            case 2: // patronus
                if (poweruppatronus != null) Instantiate(poweruppatronus, pos, Quaternion.identity);
                break;
            case 3: // buckbeak
                if (powerupbuckbeak != null) Instantiate(powerupbuckbeak, pos, Quaternion.identity);
                break;
        }
    }
    // Update is called once per frame
    public void Update()
    {
        // entradas globales
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // salir rápido desde ciertos estados
            if (CurrentState == GameState.Menu) Home();
        }
        switch (CurrentState)
        {
            case GameState.Menu:
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    StartGame();
                }
                break;
            case GameState.Playing:
                // movimiento / dirección / entrada
                if (Input.GetKey(KeyCode.LeftArrow) || (JS != null && JS.JSmove.Horizontal < 0))
                {
                    goLeft = true;
                    goRight = false;
                }
                if (Input.GetKey(KeyCode.RightArrow) || (JS != null && JS.JSmove.Horizontal > 0))
                {
                    goRight = true;
                    goLeft = false;
                }
                if ((Input.GetKey(KeyCode.UpArrow) || (JS != null && JS.JSmove.Vertical > 0.95f)) && harry.position.y < 0)
                {
                    Up = true;
                    if (animHarry != null) animHarry.SetBool("up", true);
                }
                if (Input.GetKeyUp(KeyCode.UpArrow) || (JS != null && JS.JSmove.Vertical < 0.95f) || harry.position.y > 0)
                {
                    Up = false;
                    if (animHarry != null) animHarry.SetBool("up", false);
                }
                // Pausar
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Pause();
                }
                // Powerups / counters UI
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
                // Hechizo con teclado
                if (!powerupescobabool)
                {
                    if (Input.GetKeyDown(KeyCode.Z) && goLeft && !Up) Instantiate(patronumL, harry.position + new Vector3(-0.6f, -0.1f, 0), Quaternion.identity);
                    if (Input.GetKeyDown(KeyCode.Z) && goRight && !Up) Instantiate(patronumR, harry.position + new Vector3(0.6f, -0.1f, 0), Quaternion.identity);
                    if (Input.GetKeyDown(KeyCode.Z) && goRight && Up && !salto) Instantiate(patronumU, harry.position + new Vector3(0.2f, 0.7f, 0), Quaternion.identity);
                    if (Input.GetKeyDown(KeyCode.Z) && goLeft && Up && !salto) Instantiate(patronumU, harry.position + new Vector3(-0.2f, 0.7f, 0), Quaternion.identity);
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.Z) && goLeft) Instantiate(patronumL, harry.position + new Vector3(-1.6f, 0.1f, 0), Quaternion.identity);
                    if (Input.GetKeyDown(KeyCode.Z) && goRight) Instantiate(patronumR, harry.position + new Vector3(1.6f, 0.1f, 0), Quaternion.identity);
                }
                // Actualizar vidas en UI
                if (UIManager.Instance != null) UIManager.Instance.UpdateLives(vidas);
                if (vidas > 3) vidas = 3;
                // Si se quedan sin vidas: mostrar menú de vídeo (pausa) o ir a GameOver según bandera
                if (vidas <= 0 && !repeatvideo)
                {
                    if (cancelvideo)
                    {
                        CurrentState = GameState.GameOver;
                    }
                    else
                    {
                        // marcar que la pausa será por menuVideo y cambiar estado,
                        // OnEnterState manejará la visibilidad correcta de menús
                        _pausedForVideo = true;
                        CurrentState = GameState.Paused;
                    }
                }
                if (vidas <= 0 && repeatvideo)
                {
                    CurrentState = GameState.GameOver;
                }
                break;
            case GameState.Paused:
                // entradas cuando estamos en pausa (incluye menú pausa y menú de vídeo)
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    // reanudar si se acepta la recompensa o se sale del menú pausa
                    Resume();
                }
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Home();
                }
                break;
            case GameState.GameOver:
                // en GameOver escuchamos sólo reinicio o vuelta a Home
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    Restart();
                }
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Home();
                }
                break;
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