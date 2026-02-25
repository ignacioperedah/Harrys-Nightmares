using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Score UI")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text highScoreText;

    [Header("Health UI")]
    [SerializeField] private GameObject[] hearts;

    // Esta función centraliza la actualización de la puntuación
    public void UpdateScoreUI(int currentScore)
    {
        scoreText.text = $"Score: {currentScore}";
    }

    // Esta función maneja TODA la lógica de las vidas con un simple bucle
    public void UpdateHeartsUI(int currentLives)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            // Si el índice es menor a las vidas actuales, el corazón se activa
            hearts[i].SetActive(i < currentLives);
        }
    }

    public void ShowGameOver(int finalScore, int bestScore)
    {
        // Acá podrías activar el menú de GameOver y setear los textos finales
    }
}
