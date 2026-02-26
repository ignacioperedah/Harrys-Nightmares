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
    public Text contadorescoba;
    public GameObject MenuVideo;
    public GameObject player;

    [Header("Score Texts")]
    public Text scoreInGameText; // Score (in game)
    public Text scoreDeathText;  // Score on death screen
    public Text highScoreText;   // High score on death screen

    [Header("Health UI")]
    public GameObject[] hearts; // Arrastrá tus 3 GameObjects de corazones aquí

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Score
    public void UpdateScore(int score)
    {
        if (scoreInGameText != null)
            scoreInGameText.text = $"Score: {score}";
    }

    // Lives / corazones
    public void UpdateLives(int currentLives)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].SetActive(i < currentLives);
        }
    }

    // Muestra pantalla Game Over y actualiza textos
    public void ShowGameOver(int finalScore, int bestScore)
    {
        // Aseguramos que el menú de vídeo no quede visible al pasar a GameOver.
        SetMenuVideoActive(false);

        // Desactivamos la UI de juego (incluye 'enjuego') para detener su música/sonidos.
        HideGameplayUI();

        SetMenuUIActive(false);
        if (menuGameOver != null) menuGameOver.SetActive(true);
        if (scoreDeathText != null) scoreDeathText.text = $"{finalScore}";
        if (highScoreText != null) highScoreText.text = $"{bestScore}";
    }

    // Oculta el GameOver (útil al reiniciar)
    public void HideGameOver()
    {
        if (menuGameOver != null) menuGameOver.SetActive(false);
    }

    // Pause UI
    public void SetPauseUI(bool isPaused)
    {
        if (menuPause != null) menuPause.SetActive(isPaused);
        if (menuUI != null) menuUI.SetActive(!isPaused);
    }

    // Start / Main menu
    public void ShowStartMenu()
    {
        if (menuInicio != null) menuInicio.SetActive(true);
        if (enmenus != null) enmenus.SetActive(true);
        if (SCORE != null) SCORE.SetActive(false);
        if (enjuego != null) enjuego.SetActive(false);
        if (menuUI != null) menuUI.SetActive(false);
        // Aseguramos que el GameOver no aparezca sobre el menú de inicio
        HideGameOver();
    }

    public void HideStartMenu()
    {
        if (menuInicio != null) menuInicio.SetActive(false);
        if (enmenus != null) enmenus.SetActive(false);
    }

    public void ShowGameplayUI()
    {
        // Al entrar en juego aseguramos que cualquier pantalla superpuesta (GameOver, MenuVideo, etc.) esté oculta
        HideStartMenu();
        HideGameOver();
        SetMenuVideoActive(false);

        if (SCORE != null) SCORE.SetActive(true);
        if (enjuego != null) enjuego.SetActive(true);
        if (menuUI != null) menuUI.SetActive(true);
        if (buttonsUI != null) buttonsUI.SetActive(true);
    }

    public void HideGameplayUI()
    {
        if (SCORE != null) SCORE.SetActive(false);
        if (enjuego != null) enjuego.SetActive(false);
        if (menuUI != null) menuUI.SetActive(false);
        if (buttonsUI != null) buttonsUI.SetActive(false);
    }

    // Options menu
    public void ShowOptions(bool show)
    {
        if (menuOptions != null) menuOptions.SetActive(show);
        if (menuInicio != null) menuInicio.SetActive(!show);
    }

    // Botones de UI
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

    // Counter (escoba / buckbeak)
    public void SetCounterActive(bool active)
    {
        if (counterescoba != null) counterescoba.SetActive(active);
        if (contadorescoba != null) contadorescoba.enabled = active;
    }

    public void UpdateCounterText(string text)
    {
        if (contadorescoba != null) contadorescoba.text = text;
    }

    // Menu video / player
    public void SetMenuVideoActive(bool active)
    {
        if (MenuVideo != null) MenuVideo.SetActive(active);
    }

    public void SetPlayerActive(bool active)
    {
        if (player != null) player.SetActive(active);
    }

    // Exponer control directo de menuUI si es necesario
    public void SetMenuUIActive(bool active)
    {
        if (menuUI != null) menuUI.SetActive(active);
    }
}
