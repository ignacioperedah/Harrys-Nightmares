using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Configuración (PlayerStats)")]
    [SerializeField] private PlayerStats stats;

    [Header("Escalas")]
    [SerializeField] private Vector2 escalaNormal   = new Vector2(5f, 5f);
    [SerializeField] private Vector2 escalaEscoba   = new Vector2(4f, 4f);
    [SerializeField] private Vector2 escalaBuckbeak = new Vector2(2.25f, 2.25f);

    [SerializeField] private Joystick JSmove;

    private Animator    anim;
    private Rigidbody2D rb;
    private Transform   tr;

    private bool  jumpRequested;
    private float horizontalInput;
    private float verticalInput;
    private Vector3    previousPosition;
    private GameState? _previousGameState;

    private bool facingLeft = true;

    private bool _wasPowerupEscoba;
    private bool _wasPowerupBuckbeak;

    private bool restoreFromEscobaRequested;
    private bool restoreFromBuckbeakRequested;
    private bool resetDefaultsRequested;

    private bool _previousSpellButton = false;
    private bool _uiJumpPressed       = false;
    private bool _previousUIJump      = false;

    // ── Animator hashes ───────────────────────────────────────────────────────
    private static readonly int IdUp       = Animator.StringToHash(GameConstants.AnimatorParams.Up);
    private static readonly int IdSalto    = Animator.StringToHash(GameConstants.AnimatorParams.Salto);
    private static readonly int IdEscoba   = Animator.StringToHash(GameConstants.AnimatorParams.Escoba);
    private static readonly int IdBuckbeak = Animator.StringToHash(GameConstants.AnimatorParams.Buckbeak);
    private static readonly int IdRunLeft  = Animator.StringToHash(GameConstants.AnimatorParams.RunLeft);
    private static readonly int IdRunRight = Animator.StringToHash(GameConstants.AnimatorParams.RunRight);
    private static readonly int IdGoLeft   = Animator.StringToHash(GameConstants.AnimatorParams.GoLeft);
    private static readonly int IdDisparoL = Animator.StringToHash(GameConstants.AnimatorParams.DisparoL);
    private static readonly int IdDisparoR = Animator.StringToHash(GameConstants.AnimatorParams.DisparoR);

    // ── Propiedades de estado ─────────────────────────────────────────────────

    private bool PowerupEscoba
        => PowerUpHandler.Instance != null && PowerUpHandler.Instance.PowerupEscobaBool;

    private bool PowerupBuckbeak
        => PowerUpHandler.Instance != null && PowerUpHandler.Instance.PowerupBuckbeakBool;

    private int Vidas
    {
        get => GameManager.Instance != null ? GameManager.Instance.vidas : 0;
        set { if (GameManager.Instance != null) GameManager.Instance.vidas = value; }
    }

    private float FuerzaSalto            => stats != null ? stats.fuerzaSalto            : 400f;
    private float VelocidadSuelo         => stats != null ? stats.velocidadSuelo         : 10f;
    private float MultiplicadorAire      => stats != null ? stats.multiplicadorAire      : 0.5f;
    private float EscobaMoveMultiplier   => stats != null ? stats.escobaMoveMultiplier   : 0.1f;
    private float BuckbeakMoveMultiplier => stats != null ? stats.buckbeakMoveMultiplier : 0.15f;

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

    void Start()
    {
        rb   = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        tr   = transform;
        previousPosition = tr.position;

        if (GameManager.Instance != null) _previousGameState = GameManager.Instance.CurrentState;

        _wasPowerupEscoba   = PowerupEscoba;
        _wasPowerupBuckbeak = PowerupBuckbeak;
    }

    // ── API pública para GameManager ──────────────────────────────────────────

    public void ResetAnimator()
    {
        if (anim == null) return;
        anim.SetBool(IdUp,       false);
        anim.SetBool(IdEscoba,   false);
        anim.SetBool(IdBuckbeak, false);
        anim.SetBool(IdRunLeft,  false);
        anim.SetBool(IdRunRight, false);
        anim.SetBool(IdDisparoL, false);
        anim.SetBool(IdDisparoR, false);
        anim.SetBool(IdSalto,    false);
        anim.SetBool(IdGoLeft,   true);
    }

    public void SyncFacing(bool left)
    {
        facingLeft = left;
        if (GameManager.Instance != null) GameManager.Instance.SetFacing(left);
        if (anim != null) anim.SetBool(IdGoLeft, left);
    }

    public void ResetForVideoReward()
    {
        ResetPlayerToDefault();
        SyncFacing(true);
    }

    // ── Update ────────────────────────────────────────────────────────────────

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
        {
            DetectAndHandleStateChange();
            return;
        }

        if (Vidas < 1)
        {
            if (JSmove != null) JSmove.input = Vector2.zero;
            DetectAndHandleStateChange();
            return;
        }

        horizontalInput = JSmove != null ? JSmove.Horizontal : 0f;
        verticalInput   = JSmove != null ? JSmove.Vertical   : 0f;

        if (Mathf.Abs(horizontalInput) > 0.01f)
        {
            bool newFacingLeft = horizontalInput < 0f;
            if (newFacingLeft != facingLeft)
                SyncFacing(newFacingLeft);
        }

        bool up = JSmove != null && verticalInput > 0.95f && tr.position.y < 0f;
        if (GameManager.Instance != null) GameManager.Instance.Up = up;

        bool uiJumpEdge = _uiJumpPressed && !_previousUIJump;
        if (uiJumpEdge && tr.position.y < 0 && !PowerupEscoba && !PowerupBuckbeak)
            jumpRequested = true;
        _previousUIJump = _uiJumpPressed;

        bool uiSpell      = PlayerCombat.Instance != null && PlayerCombat.Instance.ButtonSpell;
        bool spellPressed = uiSpell && !_previousSpellButton;

        // ✅ Detectar cambio de estado de powerups ANTES de actualizar animaciones
        bool escobaActive = PowerupEscoba;
        bool buckActive   = PowerupBuckbeak;

        // Si hubo transición, cambiar escala INMEDIATAMENTE (antes del Animator)
        if (escobaActive && !_wasPowerupEscoba)
        {
            tr.localScale = escalaEscoba;
        }
        else if (buckActive && !_wasPowerupBuckbeak)
        {
            tr.localScale = escalaBuckbeak;
        }
        else if (!escobaActive && _wasPowerupEscoba)
        {
            tr.localScale = escalaNormal;
            restoreFromEscobaRequested = true;
        }
        else if (!buckActive && _wasPowerupBuckbeak)
        {
            tr.localScale = escalaNormal;
            restoreFromBuckbeakRequested = true;
        }

        UpdateAnimations(spellPressed, up);

        _previousSpellButton = uiSpell;
        _wasPowerupEscoba   = escobaActive;
        _wasPowerupBuckbeak = buckActive;

        DetectAndHandleStateChange();
    }

    // ── FixedUpdate ───────────────────────────────────────────────────────────

    void FixedUpdate()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;

        if (restoreFromEscobaRequested)
        {
            restoreFromEscobaRequested = false;
            rb.gravityScale = 1f;
        }

        if (restoreFromBuckbeakRequested)
        {
            restoreFromBuckbeakRequested = false;
            rb.gravityScale = 1f;
        }

        if (resetDefaultsRequested)
        {
            resetDefaultsRequested = false;
            rb.gravityScale = 1f;
            rb.velocity     = Vector2.zero;
        }

        if (jumpRequested)
        {
            jumpRequested = false;
            if (tr.position.y < 0 && !PowerupEscoba && !PowerupBuckbeak)
            {
                rb.AddForce(Vector2.up * FuerzaSalto);
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX(GameConstants.Audio.Jump);
            }
        }

        // ✅ FixedUpdate se enfoca SOLO en física (velocidad, gravedad, posición)
        if (!PowerupEscoba && !PowerupBuckbeak)
        {
            float speed = VelocidadSuelo * (tr.position.y > 0.01f ? MultiplicadorAire : 1f);
            rb.velocity = new Vector2(horizontalInput * speed, rb.velocity.y);

            if (rb.gravityScale != 1f) rb.gravityScale = 1f;
        }
        else
        {
            rb.velocity = Vector2.zero;

            if (PowerupEscoba)
            {
                rb.gravityScale = 0f;
                rb.MovePosition(tr.position + new Vector3(
                    horizontalInput * EscobaMoveMultiplier * Time.fixedDeltaTime,
                    verticalInput   * EscobaMoveMultiplier * Time.fixedDeltaTime, 0f));
            }
            else if (PowerupBuckbeak)
            {
                rb.gravityScale = 0f;
                rb.MovePosition(tr.position + new Vector3(
                    horizontalInput * BuckbeakMoveMultiplier * Time.fixedDeltaTime,
                    verticalInput   * BuckbeakMoveMultiplier * Time.fixedDeltaTime, 0f));
            }
        }

        previousPosition = tr.position;
    }

    // ── Animaciones ───────────────────────────────────────────────────────────

    private void UpdateAnimations(bool spellPressed, bool up)
    {
        if (anim == null) return;

        anim.SetBool(IdUp,       up);
        anim.SetBool(IdSalto,    tr.position.y > 0f);
        anim.SetBool(IdEscoba,   PowerupEscoba);
        anim.SetBool(IdBuckbeak, PowerupBuckbeak);

        anim.SetBool(IdRunRight, horizontalInput > 0f && !PowerupEscoba && !PowerupBuckbeak);
        anim.SetBool(IdRunLeft,  horizontalInput < 0f && !PowerupEscoba && !PowerupBuckbeak);
        anim.SetBool(IdGoLeft,   facingLeft);

        if (spellPressed)
        {
            anim.SetBool(IdDisparoL, !PowerupBuckbeak &&  facingLeft);
            anim.SetBool(IdDisparoR, !PowerupBuckbeak && !facingLeft);
        }
        else
        {
            anim.SetBool(IdDisparoL, false);
            anim.SetBool(IdDisparoR, false);
        }
    }

    // ── Estado FSM ────────────────────────────────────────────────────────────

    private void DetectAndHandleStateChange()
    {
        if (GameManager.Instance == null) return;
        GameState current = GameManager.Instance.CurrentState;
        if (_previousGameState.HasValue && _previousGameState.Value != current)
        {
            if (current == GameState.Menu || current == GameState.GameOver)
                ResetPlayerToDefault();
        }
        _previousGameState = current;
    }

    private void ResetPlayerToDefault()
    {
        tr.localScale          = escalaNormal;
        resetDefaultsRequested = true;
        jumpRequested          = false;
        horizontalInput        = 0f;
        verticalInput          = 0f;
        if (JSmove != null) JSmove.ResetJoystick();
        ResetAnimator();
        _wasPowerupEscoba   = false;
        _wasPowerupBuckbeak = false;
        SyncFacing(true);
    }

    // ── Input UI ──────────────────────────────────────────────────────────────

    public void Salto()    => _uiJumpPressed = true;
    public void saltoNot() => _uiJumpPressed = false;

    // ── Colisiones ────────────────────────────────────────────────────────────

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (GameManager.Instance == null) return;

        var collectible = collision.gameObject.GetComponent<CollectibleBase>();
        if (collectible != null)
        {
            collectible.OnCollect();
            return;
        }

        if (collision.gameObject.CompareTag(GameConstants.Tags.Dementor) && !PowerupBuckbeak)
        {
            Vidas = Mathf.Max(0, Vidas - 1);
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(GameConstants.Audio.VidaMenos);
        }
    }
}
