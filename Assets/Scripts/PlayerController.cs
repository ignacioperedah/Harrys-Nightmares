using UnityEngine;

/// <summary>
/// Controlador del jugador.
/// - Fuente de verdad para animaciones, física y orientación del sprite.
/// - Expone ResetAnimator() y SyncFacing() para que GameManager delegue
///   operaciones que antes hacía directamente sobre animHarry.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Configuración (PlayerStats)")]
    [SerializeField] private PlayerStats stats;

    [Header("Escalas")]
    [SerializeField] private Vector2 escalaNormal  = new Vector2(5f, 5f);
    [SerializeField] private Vector2 escalaEscoba  = new Vector2(4f, 4f);
    [SerializeField] private Vector2 escalaBuckbeak = new Vector2(2.25f, 2.25f);

    [SerializeField] private Joystick JSmove;

    private Animator      anim;
    private Rigidbody2D   rb;
    private Transform     tr;

    private bool  jumpRequested;
    private float horizontalInput;
    private float verticalInput;
    private Vector3 previousPosition;
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

    // ?? Propiedades de estado ?????????????????????????????????????????????????

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

    // ?? Lifecycle ?????????????????????????????????????????????????????????????

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

    // ?? API pública para GameManager ??????????????????????????????????????????

    /// <summary>
    /// Resetea todos los parámetros del Animator a su estado neutro.
    /// Llamado por GameManager al entrar en GameOver.
    /// </summary>
    public void ResetAnimator()
    {
        if (anim == null) return;
        anim.SetBool("up",       false);
        anim.SetBool("escoba",   false);
        anim.SetBool("buckbeak", false);
        anim.SetBool("runLeft",  false);
        anim.SetBool("runRight", false);
        anim.SetBool("disparoL", false);
        anim.SetBool("disparoR", false);
        anim.SetBool("salto",    false);
        anim.SetBool("goLeft",   false);
    }

    /// <summary>
    /// Sincroniza la orientación del sprite con el valor indicado.
    /// Llamado por GameManager.SetFacing() y GameManager.VideoReward().
    /// </summary>
    public void SyncFacing(bool left)
    {
        facingLeft = left;
        if (GameManager.Instance != null) GameManager.Instance.SetFacing(left);
        if (anim != null) anim.SetBool("goLeft", left);
    }

    // ?? Update ????????????????????????????????????????????????????????????????

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

        float axis = JSmove != null ? JSmove.Horizontal : 0f;
        horizontalInput = axis;

        verticalInput = JSmove != null ? JSmove.Vertical : 0f;

        if (Mathf.Abs(horizontalInput) > 0.01f)
        {
            bool newFacingLeft = horizontalInput < 0f;
            if (newFacingLeft != facingLeft)
            {
                // SyncFacing actualiza el campo local, GameManager y el Animator
                SyncFacing(newFacingLeft);
            }
        }

        bool up = JSmove != null && verticalInput > 0.95f && tr.position.y < 0f;
        if (GameManager.Instance != null) GameManager.Instance.Up = up;

        bool uiJumpEdge = _uiJumpPressed && !_previousUIJump;
        if (uiJumpEdge && tr.position.y < 0 && !PowerupEscoba && !PowerupBuckbeak)
            jumpRequested = true;
        _previousUIJump = _uiJumpPressed;

        bool uiSpell     = PlayerCombat.Instance != null && PlayerCombat.Instance.ButtonSpell;
        bool spellPressed = uiSpell && !_previousSpellButton;

        UpdateAnimations(spellPressed, up);

        _previousSpellButton = uiSpell;

        bool escobaActive = PowerupEscoba;
        bool buckActive   = PowerupBuckbeak;

        if (!escobaActive && _wasPowerupEscoba)   restoreFromEscobaRequested   = true;
        if (!buckActive   && _wasPowerupBuckbeak) restoreFromBuckbeakRequested = true;

        _wasPowerupEscoba   = escobaActive;
        _wasPowerupBuckbeak = buckActive;

        DetectAndHandleStateChange();
    }

    // ?? FixedUpdate ???????????????????????????????????????????????????????????

    void FixedUpdate()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;

        if (restoreFromEscobaRequested)
        {
            restoreFromEscobaRequested = false;
            tr.localScale    = escalaNormal;
            rb.gravityScale  = 1f;
        }

        if (restoreFromBuckbeakRequested)
        {
            restoreFromBuckbeakRequested = false;
            tr.localScale    = escalaNormal;
            rb.gravityScale  = 1f;
        }

        if (resetDefaultsRequested)
        {
            resetDefaultsRequested = false;
            tr.localScale   = escalaNormal;
            rb.gravityScale = 1f;
            rb.velocity     = Vector2.zero;
        }

        if (jumpRequested)
        {
            jumpRequested = false;
            if (tr.position.y < 0 && !PowerupEscoba && !PowerupBuckbeak)
            {
                rb.AddForce(Vector2.up * FuerzaSalto);
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("Jump");
            }
        }

        if (!PowerupEscoba && !PowerupBuckbeak)
        {
            float speed = VelocidadSuelo * (tr.position.y > 0.01f ? MultiplicadorAire : 1f);
            rb.velocity = new Vector2(horizontalInput * speed, rb.velocity.y);
        }
        else
        {
            rb.velocity = Vector2.zero;

            if (PowerupEscoba)
            {
                tr.localScale   = escalaEscoba;
                rb.gravityScale = 0f;
                rb.MovePosition(tr.position + new Vector3(
                    horizontalInput * EscobaMoveMultiplier   * Time.fixedDeltaTime,
                    verticalInput   * EscobaMoveMultiplier   * Time.fixedDeltaTime, 0f));
            }
            else if (PowerupBuckbeak)
            {
                tr.localScale   = escalaBuckbeak;
                rb.gravityScale = 0f;
                rb.MovePosition(tr.position + new Vector3(
                    horizontalInput * BuckbeakMoveMultiplier * Time.fixedDeltaTime,
                    verticalInput   * BuckbeakMoveMultiplier * Time.fixedDeltaTime, 0f));
            }
        }

        if (!PowerupEscoba && !PowerupBuckbeak)
        {
            if (tr.localScale   != (Vector3)escalaNormal) tr.localScale   = escalaNormal;
            if (rb.gravityScale != 1f)                    rb.gravityScale = 1f;
        }

        previousPosition = tr.position;
    }

    // ?? Animaciones ???????????????????????????????????????????????????????????

    private void UpdateAnimations(bool spellPressed, bool up)
    {
        if (anim == null) return;

        anim.SetBool("up",      up);
        anim.SetBool("salto",   tr.position.y > 0f);
        anim.SetBool("escoba",  PowerupEscoba);
        anim.SetBool("buckbeak",PowerupBuckbeak);

        bool movingRight = horizontalInput > 0f;
        bool movingLeft  = horizontalInput < 0f;
        anim.SetBool("runRight", movingRight && !PowerupEscoba && !PowerupBuckbeak);
        anim.SetBool("runLeft",  movingLeft  && !PowerupEscoba && !PowerupBuckbeak);
        anim.SetBool("goLeft",   facingLeft);

        if (spellPressed)
        {
            anim.SetBool("disparoL", !PowerupBuckbeak &&  facingLeft);
            anim.SetBool("disparoR", !PowerupBuckbeak && !facingLeft);
        }
        else
        {
            anim.SetBool("disparoL", false);
            anim.SetBool("disparoR", false);
        }
    }

    // ?? Estado FSM ????????????????????????????????????????????????????????????

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

        // Resetear input y posición visual del handle del joystick
        if (JSmove != null) JSmove.ResetJoystick();

        ResetAnimator();
        _wasPowerupEscoba   = false;
        _wasPowerupBuckbeak = false;
    }

    // ?? Input UI ??????????????????????????????????????????????????????????????

    public void Salto()    => _uiJumpPressed = true;
    public void saltoNot() => _uiJumpPressed = false;

    // ?? Colisiones ????????????????????????????????????????????????????????????

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (GameManager.Instance == null) return;

        var collectible = collision.gameObject.GetComponent<CollectibleBase>();
        if (collectible != null)
        {
            collectible.OnCollect();
            return;
        }

        if (collision.gameObject.CompareTag("Dementor") && !PowerupBuckbeak)
        {
            Vidas = Mathf.Max(0, Vidas - 1);
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("VidaMenos");
        }
    }
}
