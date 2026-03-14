using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // Patrón Singleton para acceso fácil
    public static UIManager Instance;

    [Header("Menus")]
    public GameObject menuInicio;
    public GameObject menuGameOver;
    public GameObject menuPause;
    public GameObject menuUI;
    public GameObject menuOptions;

    [Header("Gameplay UI")]
    public GameObject SCORE;
    public GameObject enjuego;
    public GameObject enmenus;
    public GameObject buttonsUI;
    public GameObject bJump;
    public GameObject bSpell;
    public GameObject counterescoba;
    public Text       contadorescoba;
    public GameObject MenuVideo;
    public GameObject player;

    [Header("Score Texts")]
    public Text scoreInGameText;
    public Text scoreDeathText;
    public Text highScoreText;

    [Header("Health UI")]
    public GameObject[] hearts;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ── Suscripción a eventos ─────────────────────────────────────────────────

    private void OnEnable()
    {
        GameManager.OnScoreChanged    += UpdateScore;
        GameManager.OnLivesChanged    += UpdateLives;
        GameManager.OnStateChanged    += HandleStateChanged;
        PowerUpHandler.OnPowerupStateChanged += HandlePowerupState;
        PowerUpHandler.OnCounterChanged      += UpdateCounterText;
    }

    private void OnDisable()
    {
        GameManager.OnScoreChanged    -= UpdateScore;
        GameManager.OnLivesChanged    -= UpdateLives;
        GameManager.OnStateChanged    -= HandleStateChanged;
        PowerUpHandler.OnPowerupStateChanged -= HandlePowerupState;
        PowerUpHandler.OnCounterChanged      -= UpdateCounterText;
    }

    // ── Reacción al estado de la FSM ──────────────────────────────────────────

    private void HandleStateChanged(GameState newState)
    {
        switch (newState)
        {
            case GameState.Menu:
                ShowStartMenu();
                break;

            case GameState.Playing:
                // Cierra cualquier overlay antes de mostrar el juego
                SetPauseUI(false);
                ShowGameplayUI();
                // Elimina referencias a Restart que antes manipulaban la UI directamente
                SetCounterActive(false);
                SetButtonsActive(true);
                SetPlayerActive(true);
                SetMenuVideoActive(false);
                break;

            case GameState.Paused:
                // Detecta pausa por video leyendo el estado público del GameManager
                var gm = GameManager.Instance;
                bool pausedForVideo = gm != null && !gm.cancelvideo && gm.vidas <= 0 && !gm.repeatvideo;
                if (pausedForVideo)
                {
                    SetPlayerActive(false);
                    SetMenuVideoActive(true);
                    SetMenuUIActive(false);
                }
                else
                {
                    SetPauseUI(true);
                }
                break;

            case GameState.GameOver:
                var gmGO = GameManager.Instance;
                ShowGameOver(gmGO != null ? gmGO.score : 0, gmGO != null ? gmGO.highS : 0);
                SetCounterActive(false);
                SetPlayerActive(true);
                SetMenuUIActive(true);
                break;
        }
    }

    // ── Reacción al estado de un powerup ─────────────────────────────────────

    private void HandlePowerupState(string powerupName, bool isActive)
    {
        SetCounterActive(isActive);

        var ph = PowerUpHandler.Instance;

        if (isActive)
        {
            if (powerupName == "Buckbeak")
            {
                SetJumpButtonActive(false);
                SetSpellButtonActive(false);
            }
            else if (powerupName == "Escoba")
            {
                SetJumpButtonActive(false);
            }
            return;
        }

        // Al desactivarse, restaurar botones según el estado actual
        bool anyActive = ph != null && (ph.PowerupEscobaBool || ph.PowerupBuckbeakBool);
        
        if (!anyActive)
        {
            // Ningún powerup activo: restaurar todos los botones
            SetJumpButtonActive(true);
            SetSpellButtonActive(true);
        }
        else
        {
            // Si se desactiva uno pero el otro sigue activo, mantenemos la lógica correspondiente
            if (ph.PowerupBuckbeakBool)
            {
                SetJumpButtonActive(false);
                SetSpellButtonActive(false);
            }
            else if (ph.PowerupEscobaBool)
            {
                SetJumpButtonActive(false);
                SetSpellButtonActive(true); // Escoba sí permite disparar
            }
        }
    }

    // ── Score ─────────────────────────────────────────────────────────────────

    public void UpdateScore(int score)
    {
        if (scoreInGameText != null)
            scoreInGameText.text = $"Score: {score}";
    }

    // ── Vidas / corazones ─────────────────────────────────────────────────────

    public void UpdateLives(int currentLives)
    {
        for (int i = 0; i < hearts.Length; i++)
            hearts[i].SetActive(i < currentLives);
    }

    // ── Game Over ─────────────────────────────────────────────────────────────

    public void ShowGameOver(int finalScore, int bestScore)
    {
        SetMenuVideoActive(false);
        HideGameplayUI();
        SetMenuUIActive(false);
        if (menuGameOver   != null) menuGameOver.SetActive(true);
        if (scoreDeathText != null) scoreDeathText.text = $"{finalScore}";
        if (highScoreText  != null) highScoreText.text  = $"{bestScore}";
    }

    public void HideGameOver()
    {
        if (menuGameOver != null) menuGameOver.SetActive(false);
    }

    // ── Pausa ─────────────────────────────────────────────────────────────────

    public void SetPauseUI(bool isPaused)
    {
        if (menuPause != null) menuPause.SetActive(isPaused);
        if (menuUI    != null) menuUI.SetActive(!isPaused);
    }

    // ── Menú principal ────────────────────────────────────────────────────────

    public void ShowStartMenu()
    {
        if (menuInicio != null) menuInicio.SetActive(true);
        if (enmenus    != null) enmenus.SetActive(true);
        if (SCORE      != null) SCORE.SetActive(false);
        if (enjuego    != null) enjuego.SetActive(false);
        if (menuUI     != null) menuUI.SetActive(false);
        HideGameOver();
    }

    public void HideStartMenu()
    {
        if (menuInicio != null) menuInicio.SetActive(false);
        if (enmenus    != null) enmenus.SetActive(false);
    }

    // ── Gameplay UI ───────────────────────────────────────────────────────────

    public void ShowGameplayUI()
    {
        HideStartMenu();
        HideGameOver();
        SetMenuVideoActive(false);
        if (SCORE     != null) SCORE.SetActive(true);
        if (enjuego   != null) enjuego.SetActive(true);
        if (menuUI    != null) menuUI.SetActive(true);
        if (buttonsUI != null) buttonsUI.SetActive(true);
    }

    public void HideGameplayUI()
    {
        if (SCORE     != null) SCORE.SetActive(false);
        if (enjuego   != null) enjuego.SetActive(false);
        if (menuUI    != null) menuUI.SetActive(false);
        if (buttonsUI != null) buttonsUI.SetActive(false);
    }

    // ── Options ───────────────────────────────────────────────────────────────

    public void ShowOptions(bool show)
    {
        if (menuOptions != null) menuOptions.SetActive(show);
        if (menuInicio  != null) menuInicio.SetActive(!show);
    }

    // ── Botones ───────────────────────────────────────────────────────────────

    public void SetButtonsActive(bool active)
    {
        if (buttonsUI != null) buttonsUI.SetActive(active);
    }

    public void SetJumpButtonActive(bool active)
    {
        if (bJump != null) bJump.SetActive(active);
    }

    public void SetSpellButtonActive(bool active)
    {
        if (bSpell != null) bSpell.SetActive(active);
    }

    // ── Counter (escoba / buckbeak) ───────────────────────────────────────────

    public void SetCounterActive(bool active)
    {
        if (counterescoba  != null) counterescoba.SetActive(active);
        if (contadorescoba != null) contadorescoba.enabled = active;
    }

    /// <summary>Recibe el valor float desde PowerUpHandler.OnCounterChanged.</summary>
    private void UpdateCounterText(float value)
    {
        if (contadorescoba != null)
            contadorescoba.text = Mathf.CeilToInt(value).ToString();
    }

    // ── MenuVideo / Player ────────────────────────────────────────────────────

    public void SetMenuVideoActive(bool active)
    {
        if (MenuVideo != null) MenuVideo.SetActive(active);
    }

    public void SetPlayerActive(bool active)
    {
        if (player != null) player.SetActive(active);
    }

    public void SetMenuUIActive(bool active)
    {
        if (menuUI != null) menuUI.SetActive(active);
    }
}
