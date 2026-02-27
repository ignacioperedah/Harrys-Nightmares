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
    public static GameManager Instance { get; private set; }

    public Transform harry;
    public GameObject dementor;
    public GameObject menuOptions;
    // moved power-up prefabs to PowerUpSpawner
    [SerializeField] private PowerUpSpawner powerUpSpawner;
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
    public bool exit = false;
    public bool reset = false;
    public bool powerupescobabool = false;
    public bool powerupbuckbeakbool = false;
    public bool cancelvideo = false;
    public bool repeatvideo = false;
    public bool superpatronus = false;

    bool goLeft = true;
    bool goRight = false;

    /// <summary>
    /// Establece la orientaci�n del jugador (true = izquierda).
    /// PlayerController llamar� a esto para mantener la �nica fuente de verdad.
    /// </summary>
    public void SetFacing(bool left)
    {
        goLeft = left;
        goRight = !left;
        if (animHarry != null) animHarry.SetBool("goLeft", left);
    }

    public bool IsFacingLeft() => goLeft;

    public bool Up = false;
    public bool salto = false;

    public byte ActDemen = 0;
    // ya no usamos ActPowerUp como lock; se gestiona con _lastPowerupSpawnScore
    [SerializeField] int delay = 2000;
    public int highscore;
    public int highS;
    public Vector3 vector;
    [SerializeField] private EnemySpawner spawner;
    private int _score = 0;

    // Evita spawn duplicado por el mismo valor de score m�ltiplo de 10
    private int _lastPowerupSpawnScore = -1;

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

            // Spawn de powerup cuando score es m�ltiplo de 10 (y no 0).
            if (_score != 0 && _score % 10 == 0)
            {
                // Solo spawnear una vez para ese valor de score
                if (_lastPowerupSpawnScore != _score)
                {
                    _lastPowerupSpawnScore = _score;
                    powerUpSpawner?.SpawnPowerup();
                }
            }
            else
            {
                // reset para futuros m�ltiplos
                _lastPowerupSpawnScore = -1;
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
    // Indica que la pausa actual es por mostrar el MenuVideo (no por men� pausa normal)
    private bool _pausedForVideo = false;

    void Awake()
    {
        // Singleton sencillo para acceder desde PlayerController u otros
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

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
                    // Pausa provocada por men� de v�deo: mostrar s�lo el MenuVideo
                    if (UIManager.Instance != null) UIManager.Instance.SetPlayerActive(false);
                    if (UIManager.Instance != null) UIManager.Instance.SetMenuVideoActive(true);
                    if (UIManager.Instance != null) UIManager.Instance.SetMenuUIActive(false);
                    if (spawner != null) spawner.StopSpawning();
                }
                else
                {
                    // Pausa normal: men� de pausa tradicional
                    if (UIManager.Instance != null) UIManager.Instance.SetPauseUI(true);
                    if (spawner != null) spawner.StopSpawning();
                }
                break;
            case GameState.GameOver:
                // limpiar estado de juego, mostrar GameOver y detener spawner
                Time.timeScale = 1f;
                PatronusExit();
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

                // Eliminar powerups que est�n cayendo cuando entramos a GameOver
                DestroyActivePowerups();

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
    // API: cambiar estados mediante m�todos
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
        // Reinicio inmediato: ocultar men� de v�deo y pasar a Playing
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
        // pausa normal (men� pausa)
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

    public void saltoNot()
    {
        // Ya gestionado por PlayerController
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
        // Volver desde menuVideo: asegurar orientaci�n a la izquierda antes de jugar
        if (UIManager.Instance != null) UIManager.Instance.SetMenuVideoActive(false);
        if (UIManager.Instance != null) UIManager.Instance.SetPlayerActive(true);
        if (UIManager.Instance != null) UIManager.Instance.SetMenuUIActive(true);
        repeatvideo = true;
        vidas = 1;
        // forzar orientaci�n inicial: disparo hacia la izquierda
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

    /// <summary>
    /// Elimina powerups activos que est�n cayendo (solo cuando entramos en GameOver seg�n especificaci�n).
    /// </summary>
    private void DestroyActivePowerups()
    {
        // Tags usados en el juego para powerups/coleccionables
        string[] tags = new[] { "Vidas", "Escoba", "Patronus", "Buckbeak" };

        foreach (var t in tags)
        {
            var objs = GameObject.FindGameObjectsWithTag(t);
            if (objs == null || objs.Length == 0) continue;
            foreach (var go in objs)
            {
                Destroy(go);
            }
        }

        // Tambi�n aseguramos ocultar contador y audio asociado
        if (UIManager.Instance != null) UIManager.Instance.SetCounterActive(false);
        if (audioEscoba != null) audioEscoba.SetActive(false);
    }

    // Update is called once per frame
    public void Update()
    {
        // Quitado soporte de teclado. Todo el control se hace por UI/JS.
        switch (CurrentState)
        {
            case GameState.Menu:
                // la transici�n a Playing la debe manejar la UI -> StartGame()
                break;
            case GameState.Playing:
                // Actualizar UI y contadores (no inputs de teclado)
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
                    // mantenemos _lastPowerupSpawnScore logic en setter; nada que hacer aqu�
                }

                // Actualizar vidas en UI
                if (UIManager.Instance != null) UIManager.Instance.UpdateLives(vidas);
                if (vidas > 3) vidas = 3;
                // Si se quedan sin vidas: mostrar men� de v�deo (pausa) o ir a GameOver seg�n bandera
                if (vidas <= 0 && !repeatvideo)
                {
                    if (cancelvideo)
                    {
                        CurrentState = GameState.GameOver;
                    }
                    else
                    {
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
                // control por UI
                break;
            case GameState.GameOver:
                // control por UI
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