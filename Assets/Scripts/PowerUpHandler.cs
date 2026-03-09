using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Gestiona los timers y efectos visuales de los powerups activos en el jugador.
/// Es la única fuente de verdad para el estado activo de cada powerup y sus contadores.
/// Comunica cambios de estado y contador mediante eventos C# (Action) puros.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PowerUpHandler : MonoBehaviour
{
    public static PowerUpHandler Instance { get; private set; }

    // ── Eventos de dominio ────────────────────────────────────────────────────
    /// <summary>Notifica el nombre del powerup activo y si está activo o no.</summary>
    public static event Action<string, bool> OnPowerupStateChanged;

    /// <summary>Notifica el contador actual (en segundos) del powerup activo.</summary>
    public static event Action<float> OnCounterChanged;

    [SerializeField] private GameObject harrypatronus;
    [SerializeField] private GameObject patronusgrande;

    private SpriteRenderer _rend;
    private Coroutine _escobaCoroutine;
    private Coroutine _buckbeakCoroutine;
    private Coroutine _patronusCoroutine;

    // ── Estado propio (fuente de verdad de powerups) ──────────────────────────
    public bool  PowerupEscobaBool   { get; private set; }
    public bool  PowerupBuckbeakBool { get; private set; }
    public bool  PowerUpPatronusBool { get; private set; }  // ← añadido
    public float Contador            { get; private set; } = 20f;
    public float ContadorBuckbeak    { get; private set; } = 15f;
    public float ContadorPatronus    { get; private set; } = 10f;

    private void Awake()
    {
        Instance = this;
        _rend = GetComponent<SpriteRenderer>();
    }

    // ── Vida ──────────────────────────────────────────────────────────────────

    /// <summary>Incrementa las vidas del jugador (máx. 3).</summary>
    public void ActivateVida()
    {
        GameManager.Instance?.SumarVida();
    }

    // ── Escoba ────────────────────────────────────────────────────────────────

    public void ActivateEscoba(float duration = 20f)
    {
        if (_escobaCoroutine != null) StopCoroutine(_escobaCoroutine);
        Contador = duration;
        _escobaCoroutine = StartCoroutine(EscobaTimer());
    }

    private IEnumerator EscobaTimer()
    {
        PowerupEscobaBool = true;
        OnPowerupStateChanged?.Invoke("Escoba", true);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMusic("Escoba");

        while (Contador > 5f)
        {
            yield return new WaitForSeconds(1f);
            Contador--;
            OnCounterChanged?.Invoke(Contador);
        }
        while (Contador > 0f)
        {
            _rend.color = new Color(1f, 1f, 1f, 0.5f);
            yield return new WaitForSeconds(0.5f);
            _rend.color = new Color(1f, 1f, 1f, 1f);
            yield return new WaitForSeconds(0.5f);
            Contador--;
            OnCounterChanged?.Invoke(Contador);
        }

        PowerupEscobaBool = false;
        Contador = 20f;
        _rend.color = Color.white;
        OnPowerupStateChanged?.Invoke("Escoba", false);
        if (AudioManager.Instance != null) AudioManager.Instance.StopMusic("Escoba");
        _escobaCoroutine = null;
    }

    // ── Buckbeak ──────────────────────────────────────────────────────────────

    public void ActivateBuckbeak(float duration = 15f)
    {
        if (_buckbeakCoroutine != null) StopCoroutine(_buckbeakCoroutine);
        ContadorBuckbeak = duration;
        _buckbeakCoroutine = StartCoroutine(BuckbeakTimer());
    }

    private IEnumerator BuckbeakTimer()
    {
        PowerupBuckbeakBool = true;
        OnPowerupStateChanged?.Invoke("Buckbeak", true);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMusic("Buckbeak");

        while (ContadorBuckbeak > 5f)
        {
            yield return new WaitForSeconds(1f);
            ContadorBuckbeak--;
            OnCounterChanged?.Invoke(ContadorBuckbeak);
        }
        while (ContadorBuckbeak > 0f)
        {
            _rend.color = new Color(1f, 1f, 1f, 0.5f);
            yield return new WaitForSeconds(0.5f);
            _rend.color = new Color(1f, 1f, 1f, 1f);
            yield return new WaitForSeconds(0.5f);
            ContadorBuckbeak--;
            OnCounterChanged?.Invoke(ContadorBuckbeak);
        }

        PowerupBuckbeakBool = false;
        ContadorBuckbeak = 15f;
        _rend.color = Color.white;
        OnPowerupStateChanged?.Invoke("Buckbeak", false);
        if (AudioManager.Instance != null) AudioManager.Instance.StopMusic("Buckbeak");
        _buckbeakCoroutine = null;
    }

    // ── Patronus ──────────────────────────────────────────────────────────────

    public void ActivatePatronus(float duration = 10f)
    {
        if (_patronusCoroutine != null) StopCoroutine(_patronusCoroutine);
        ContadorPatronus = duration;
        _patronusCoroutine = StartCoroutine(PatronusTimer());
    }

    public void DeactivatePatronus()
    {
        if (_patronusCoroutine != null)
        {
            StopCoroutine(_patronusCoroutine);
            _patronusCoroutine = null;
        }
        if (harrypatronus != null) harrypatronus.SetActive(false);
        PowerUpPatronusBool = false;  // ← añadido
        ContadorPatronus = 10f;
        if (AudioManager.Instance != null) AudioManager.Instance.StopMusic("Patronus");
    }

    private IEnumerator PatronusTimer()
    {
        if (harrypatronus != null) harrypatronus.SetActive(true);
        PowerUpPatronusBool = true;  // ← añadido
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMusic("Patronus");

        while (ContadorPatronus >= 0f)
        {
            yield return new WaitForSeconds(1f);
            if (patronusgrande != null)
                Instantiate(patronusgrande, new Vector3(-6f, 2.6f, 0f), Quaternion.identity);
            ContadorPatronus--;
        }

        DeactivatePatronus();
    }

    // ── Limpieza general ──────────────────────────────────────────────────────

    public void CancelAll()
    {
        if (_escobaCoroutine != null)
        {
            StopCoroutine(_escobaCoroutine);
            _escobaCoroutine = null;
            PowerupEscobaBool = false;
            Contador = 20f;
            OnPowerupStateChanged?.Invoke("Escoba", false);
        }
        if (_buckbeakCoroutine != null)
        {
            StopCoroutine(_buckbeakCoroutine);
            _buckbeakCoroutine = null;
            PowerupBuckbeakBool = false;
            ContadorBuckbeak = 15f;
            OnPowerupStateChanged?.Invoke("Buckbeak", false);
        }
        DeactivatePatronus();  // ya resetea PowerUpPatronusBool internamente
        _rend.color = Color.white;
    }
}