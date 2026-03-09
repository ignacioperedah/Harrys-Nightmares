using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager centralizado. Responsabilidad única: FSM del juego, score, vidas y dificultad.
/// - Audio    → AudioManager
/// - UI       → UIManager  (via eventos OnScoreChanged / OnLivesChanged / OnStateChanged)
/// - Powerups → PowerUpHandler (timers, bools y contadores)
/// - Animaciones / física del jugador → PlayerController
/// </summary>
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

    // ── Eventos estáticos de dominio ──────────────────────────────────────────
    public static event Action<int>       OnScoreChanged;
    public static event Action<int>       OnLivesChanged;
    public static event Action<GameState> OnStateChanged;

    // ── Referencias de escena ─────────────────────────────────────────────────
    /// <summary>
    /// Transform del jugador. Permanece aquí como punto de acceso global
    /// para sistemas externos (Dementor, PlayerCombat, spawners).
    /// </summary>
    public Transform harry;
    public GameObject dementor;

    [SerializeField] private PowerUpSpawner powerUpSpawner;
    [SerializeField] private EnemySpawner   spawner;

    [Header("Difficulty")]
    [SerializeField] private DifficultyConfig difficultyConfig;

    // ── Estado de flags ───────────────────────────────────────────────────────
    public bool exit        = false;
    public bool reset       = false;
    public bool cancelvideo = false;
    public bool repeatvideo = false;

    // ── Orientación del jugador ───────────────────────────────────────────────
    private bool _goLeft = true;

    /// <summary>
    /// Establece la orientación del jugador (true = izquierda).
    /// PlayerController llama a esto para mantener la única fuente de verdad.
    /// El Animator se actualiza directamente en PlayerController.
    /// </summary>
    public void SetFacing(bool left)
    {
        _goLeft = left;
        // La sincronización del Animator queda en PlayerController.SyncFacing()
    }

    public bool IsFacingLeft() => _goLeft;

    public bool Up    = false;
    public bool salto = false;

    public byte ActDemen = 0;

    [SerializeField] private int delay = 2000;
    public int highscore;
    public int highS;
    public Vector3 vector;

    // ── Score ─────────────────────────────────────────────────────────────────
    private int _score = 0;
    private int _lastPowerupSpawnScore = -1;

    public int score
    {
        get => _score;
        set
        {
            _score = value;
            UpdateDifficulty();
            if (spawner != null) spawner.UpdateSpawnRate(delay);
            OnScoreChanged?.Invoke(_score);

            if (_score != 0 && _score % 10 == 0)
            {
                if (_lastPowerupSpawnScore != _score)
                {
                    _lastPowerupSpawnScore = _score;
                    powerUpSpawner?.SpawnPowerup();
                }
            }
            else
            {
                _lastPowerupSpawnScore = -1;
            }
        }
    }

    // ── Vidas ─────────────────────────────────────────────────────────────────
    private int _vidas = 1;
    public int vidas
    {
        get => _vidas;
        set
        {
            if (_vidas == value) return;
            _vidas = value;
            OnLivesChanged?.Invoke(_vidas);
        }
    }

    // ── FSM ───────────────────────────────────────────────────────────────────
    private GameState _currentState = GameState.Menu;
    public GameState CurrentState
    {
        get => _currentState;
        set
        {
            if (_currentState == value) return;
            _currentState = value;
            OnStateChanged?.Invoke(_currentState);
            OnEnterState(_currentState);
        }
    }

    private bool _pausedForVideo = false;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Start()
    {
        // harry se asigna desde el Inspector; como fallback buscamos por tag
        if (harry == null)
        {
            var harryObj = GameObject.FindGameObjectWithTag("Harry");
            if (harryObj != null) harry = harryObj.transform;
        }

        highscore = PlayerPrefs.GetInt("highscore", 0);
        highS     = highscore;
        Time.timeScale = 1f;

        BroadcastCurrentValues();
        CurrentState = GameState.Menu;
    }

    /// <summary>
    /// Emite los eventos de score y vidas con los valores actuales sin modificarlos.
    /// Garantiza que la UI refleje el estado inicial aunque el valor no haya cambiado.
    /// </summary>
    private void BroadcastCurrentValues()
    {
        OnScoreChanged?.Invoke(_score);
        OnLivesChanged?.Invoke(_vidas);
    }

    // ── FSM: lógica de entrada por estado ─────────────────────────────────────
    void OnEnterState(GameState state)
    {
        switch (state)
        {
            case GameState.Menu:
                Time.timeScale = 1f;
                if (spawner != null) spawner.StopSpawning();
                _pausedForVideo = false;
                break;

            case GameState.Playing:
                Time.timeScale = 1f;
                if (spawner != null) spawner.StartSpawning(delay);
                _pausedForVideo = false;
                break;

            case GameState.Paused:
                Time.timeScale = 0f;
                if (spawner != null) spawner.StopSpawning();
                break;

            case GameState.GameOver:
                Time.timeScale = 1f;
                if (PowerUpHandler.Instance != null) PowerUpHandler.Instance.CancelAll();
                Up = false;
                // Delegar reset de animaciones al PlayerController
                if (PlayerController.Instance != null) PlayerController.Instance.ResetAnimator();
                if (AudioManager.Instance != null) AudioManager.Instance.StopMusic("Patronus");
                repeatvideo = false;
                if (score > highS)
                {
                    highS = score;
                    PlayerPrefs.SetInt("highscore", score);
                }
                if (spawner != null) spawner.StopSpawning();
                DestroyActivePowerups();
                _pausedForVideo = false;
                break;
        }
    }

    // ── API pública de transiciones ───────────────────────────────────────────
    public void StartGame()  => CurrentState = GameState.Playing;
    public void Exit()       => exit = true;
    public void Pause()      { _pausedForVideo = false; CurrentState = GameState.Paused; }
    public void Resume()     => CurrentState = GameState.Playing;

    public void Restart()
    {
        _score  = 0;
        _vidas  = 1;
        cancelvideo = false;
        repeatvideo = false;
        _lastPowerupSpawnScore = -1;

        BroadcastCurrentValues();

        if (harry != null) harry.SetPositionAndRotation(vector, Quaternion.identity);
        delay = 2000;
        if (spawner != null) spawner.StartSpawning(delay);
        CurrentState = GameState.Playing;
    }

    public void Options()     => UIManager.Instance?.ShowOptions(true);
    public void OptionsBack() => UIManager.Instance?.ShowOptions(false);

    public void ResetHS()
    {
        PlayerPrefs.SetInt("highscore", 0);
        highS = 0;
    }

    public void Home()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SumarVida()
    {
        if (vidas < 3) vidas++;
    }

    public void CancelVideo()
    {
        cancelvideo     = true;
        _pausedForVideo = false;
        CurrentState = GameState.GameOver;
    }

    public void VideoReward()
    {
        repeatvideo = true;
        _vidas = 0; // forzar diferencia para que el setter dispare el evento
        vidas  = 1;
        // Delegar orientación inicial al PlayerController
        if (PlayerController.Instance != null) PlayerController.Instance.SyncFacing(true);
        if (harry != null) harry.SetPositionAndRotation(vector, Quaternion.identity);
        _pausedForVideo = false;
        CurrentState = GameState.Playing;
    }

    // ── Update ────────────────────────────────────────────────────────────────
    public void Update()
    {
        if (CurrentState != GameState.Playing) return;

        if (vidas > 3) vidas = 3;

        if (vidas <= 0 && !repeatvideo)
        {
            if (cancelvideo)
                CurrentState = GameState.GameOver;
            else
            {
                _pausedForVideo = true;
                CurrentState = GameState.Paused;
            }
        }
        else if (vidas <= 0 && repeatvideo)
        {
            CurrentState = GameState.GameOver;
        }
    }

    // ── Dificultad ────────────────────────────────────────────────────────────
    private void UpdateDifficulty()
    {
        if (difficultyConfig != null && difficultyConfig.steps != null && difficultyConfig.steps.Length > 0)
        {
            foreach (var step in difficultyConfig.steps)
            {
                if (_score > step.threshold) { delay = step.delay; break; }
            }
        }
        else
        {
            var fallback = new (int threshold, int delay)[]
            {
                (100, 113), (90, 150), (80, 200), (70, 267), (60, 356),
                (50, 475),  (40, 633), (30, 844), (20, 1125),(10, 1500)
            };
            foreach (var step in fallback)
            {
                if (_score > step.threshold) { delay = step.delay; break; }
            }
        }
    }

    // ── Limpieza de powerups en GameOver ─────────────────────────────────────
    private void DestroyActivePowerups()
    {
        if (powerUpSpawner != null)
            powerUpSpawner.DestroyAllActivePowerups();
        else
            Debug.LogWarning("PowerUpSpawner no está asignado en GameManager.");

        if (AudioManager.Instance != null) AudioManager.Instance.StopMusic("Escoba");
    }
}