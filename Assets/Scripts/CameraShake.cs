using UnityEngine;

/// <summary>
/// Screen Shake desacoplado. Se suscribe a eventos de dominio sin referencias directas.
/// Requiere estar en el mismo GameObject que la Main Camera.
/// Algoritmo: trauma exponencial (GDC "Math for Game Programmers").
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraShake : MonoBehaviour
{
    [Header("Parámetros")]
    [Tooltip("Posición máxima de desplazamiento en unidades")]
    [SerializeField] private float maxOffset     = 0.3f;

    [Tooltip("Velocidad de decay del trauma por segundo")]
    [SerializeField] private float traumaDecay   = 1.5f;

    [Tooltip("Frecuencia de muestreo del ruido Perlin")]
    [SerializeField] private float shakeFrequency = 25f;

    [Header("Intensidad")]
    [Tooltip("Trauma añadido al recibir daño")]
    [SerializeField] private float traumaOnHit = 0.6f;

    private float   _trauma;
    private float   _seed;
    private Vector3 _originPos;
    private int     _previousLives = -1;

    void Awake()
    {
        _seed      = Random.value * 100f;
        _originPos = transform.localPosition;
    }

    void OnEnable()
    {
        GameManager.OnLivesChanged += HandleLivesChanged;
        GameManager.OnStateChanged += HandleStateChanged;
    }

    void OnDisable()
    {
        GameManager.OnLivesChanged -= HandleLivesChanged;
        GameManager.OnStateChanged -= HandleStateChanged;
    }

    void Update()
    {
        if (_trauma <= 0f) return;

        _trauma = Mathf.Max(0f, _trauma - traumaDecay * Time.deltaTime);

        float shake = _trauma * _trauma; // exponencial: más suave al final

        float offsetX = maxOffset * (Mathf.PerlinNoise(_seed,        Time.time * shakeFrequency) * 2f - 1f) * shake;
        float offsetY = maxOffset * (Mathf.PerlinNoise(_seed + 100f, Time.time * shakeFrequency) * 2f - 1f) * shake;

        transform.localPosition = _originPos + new Vector3(offsetX, offsetY, 0f);
    }

    // ── API pública ───────────────────────────────────────────────────────────

    public void AddTrauma(float amount)
    {
        _trauma = Mathf.Clamp01(_trauma + amount);
    }

    // ── Handlers de eventos ───────────────────────────────────────────────────

    private void HandleLivesChanged(int currentLives)
    {
        // Solo shake si:
        // 1. No es la primera llamada (_previousLives != -1)
        // 2. Las vidas disminuyeron (currentLives < _previousLives)
        // 3. Quedan vidas (currentLives > 0)
        if (_previousLives != -1 && currentLives < _previousLives && currentLives > 0)
        {
            AddTrauma(traumaOnHit);
        }

        _previousLives = currentLives;
    }

    private void HandleStateChanged(GameState state)
    {
        if (state == GameState.Menu || state == GameState.GameOver)
        {
            _trauma = 0f;
            transform.localPosition = _originPos;
        }

        // Resetear el tracker al iniciar el juego para que la primera vida no cause shake
        if (state == GameState.Playing && GameManager.Instance != null)
        {
            _previousLives = GameManager.Instance.vidas;
        }
    }
}