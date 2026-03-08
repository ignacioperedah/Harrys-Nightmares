using System.Collections;
using UnityEngine;

/// <summary>
/// Gestiona los timers y efectos visuales de los powerups activos en el jugador.
/// Desacopla la lógica de efectos del GameManager.
/// Es el punto de entrada directo para todos los items recogibles.
/// Delega en GameManager solo lo que no puede gestionar (vidas, estado de juego).
/// Delega en PlayerController lo que afecta a física/escala del jugador.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PowerUpHandler : MonoBehaviour
{
    public static PowerUpHandler Instance { get; private set; }

    [SerializeField] private GameObject harrypatronus;
    [SerializeField] private GameObject patronusgrande;

    private SpriteRenderer _rend;
    private Coroutine _escobaCoroutine;
    private Coroutine _buckbeakCoroutine;
    private Coroutine _patronusCoroutine;

    private void Awake()
    {
        Instance = this;
        _rend = GetComponent<SpriteRenderer>();
    }

    // ── Vida ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Incrementa las vidas del jugador (máx. 3).
    /// Delega en GameManager solo la modificación del valor de vidas.
    /// </summary>
    public void ActivateVida()
    {
        GameManager.Instance.SumarVida();
    }

    // ── Escoba ────────────────────────────────────────────────────────────────

    public void ActivateEscoba(float duration = 20f)
    {
        if (_escobaCoroutine != null) StopCoroutine(_escobaCoroutine);
        var gm = GameManager.Instance;
        if (gm != null) gm.contador = duration;
        _escobaCoroutine = StartCoroutine(EscobaTimer());
    }

    private IEnumerator EscobaTimer()
    {
        var gm = GameManager.Instance;
        if (gm == null) yield break;

        gm.powerupescobabool = true;
        if (UIManager.Instance != null) UIManager.Instance.SetCounterActive(true);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMusic("Escoba");

        while (gm.contador > 5f)
        {
            yield return new WaitForSeconds(1f);
            gm.contador--;
        }
        while (gm.contador > 0f)
        {
            _rend.color = new Color(1f, 1f, 1f, 0.5f);
            yield return new WaitForSeconds(0.5f);
            _rend.color = new Color(1f, 1f, 1f, 1f);
            yield return new WaitForSeconds(0.5f);
            gm.contador--;
        }

        gm.powerupescobabool = false;
        gm.contador = 20f;
        _rend.color = Color.white;
        if (UIManager.Instance != null) UIManager.Instance.SetCounterActive(false);
        if (AudioManager.Instance != null) AudioManager.Instance.StopMusic("Escoba");
    }

    // ── Buckbeak ──────────────────────────────────────────────────────────────

    public void ActivateBuckbeak(float duration = 15f)
    {
        if (_buckbeakCoroutine != null) StopCoroutine(_buckbeakCoroutine);
        var gm = GameManager.Instance;
        if (gm != null) gm.contadorbuckbeak = duration;
        _buckbeakCoroutine = StartCoroutine(BuckbeakTimer());
    }

    private IEnumerator BuckbeakTimer()
    {
        var gm = GameManager.Instance;
        if (gm == null) yield break;

        gm.powerupbuckbeakbool = true;
        if (UIManager.Instance != null) UIManager.Instance.SetCounterActive(true);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMusic("Buckbeak");

        while (gm.contadorbuckbeak > 5f)
        {
            yield return new WaitForSeconds(1f);
            gm.contadorbuckbeak--;
        }
        while (gm.contadorbuckbeak > 0f)
        {
            _rend.color = new Color(1f, 1f, 1f, 0.5f);
            yield return new WaitForSeconds(0.5f);
            _rend.color = new Color(1f, 1f, 1f, 1f);
            yield return new WaitForSeconds(0.5f);
            gm.contadorbuckbeak--;
        }

        gm.powerupbuckbeakbool = false;
        gm.contadorbuckbeak = 15f;
        _rend.color = Color.white;
        if (UIManager.Instance != null) UIManager.Instance.SetCounterActive(false);
        if (AudioManager.Instance != null) AudioManager.Instance.StopMusic("Buckbeak");
    }

    // ── Patronus ──────────────────────────────────────────────────────────────

    public void ActivatePatronus(float duration = 10f)
    {
        if (_patronusCoroutine != null) StopCoroutine(_patronusCoroutine);
        var gm = GameManager.Instance;
        if (gm != null) gm.contadorpatronus = duration;
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
        var gm = GameManager.Instance;
        if (gm != null)
        {
            gm.superpatronus = false;
            gm.contadorpatronus = 10f;
        }
        if (AudioManager.Instance != null) AudioManager.Instance.StopMusic("Patronus");
    }

    private IEnumerator PatronusTimer()
    {
        var gm = GameManager.Instance;
        if (gm == null) yield break;

        if (harrypatronus != null) harrypatronus.SetActive(true);
        gm.superpatronus = true;
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMusic("Patronus");

        while (gm.contadorpatronus >= 0f)
        {
            yield return new WaitForSeconds(1f);
            if (patronusgrande != null)
                Instantiate(patronusgrande, new Vector3(-6f, 2.6f, 0f), Quaternion.identity);
            gm.contadorpatronus--;
        }

        DeactivatePatronus();
    }

    // ── Limpieza general ──────────────────────────────────────────────────────

    public void CancelAll()
    {
        if (_escobaCoroutine != null) { StopCoroutine(_escobaCoroutine); _escobaCoroutine = null; }
        if (_buckbeakCoroutine != null) { StopCoroutine(_buckbeakCoroutine); _buckbeakCoroutine = null; }
        DeactivatePatronus();
        _rend.color = Color.white;
    }
}