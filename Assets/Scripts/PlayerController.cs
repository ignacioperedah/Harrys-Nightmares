using UnityEngine;

/// <summary>
/// Controlador del jugador (refactor de Harry.cs).
/// - Fuente de verdad para vidas y powerups: GameManager.Instance.
/// - Entrada en Update, física en FixedUpdate.
/// - Animaciones basadas en la dirección real (facing) y sincronizadas con GameManager.
/// - Maneja la entrada de salto (UI/JS) directamente. Sin teclado.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    // Fuerzas / velocidades (configurables)
    [Header("Movimiento")]
    [SerializeField] private float fuerzaSalto = 400f;
    [SerializeField] private float velocidadSuelo = 10f;
    [SerializeField] private float multiplicadorAire = 0.5f;

    [Header("Powerups - movimiento")]
    [SerializeField] private float escobaMoveMultiplier = 0.1f;
    [SerializeField] private float buckbeakMoveMultiplier = 0.15f;

    [Header("Escalas")]
    [SerializeField] private Vector2 escalaNormal = new Vector2(5f, 5f);
    [SerializeField] private Vector2 escalaEscoba = new Vector2(4f, 4f);
    [SerializeField] private Vector2 escalaBuckbeak = new Vector2(2.25f, 2.25f);

    // Referencias
    [SerializeField] private Controller JS; // obligatorio para control en mobile
    private Animator anim;
    private Rigidbody2D rb;
    private Transform tr;

    // Estado interno
    private bool jumpRequested;
    private float horizontalInput;
    private Vector3 previousPosition;
    private GameState? _previousGameState;

    // Facing: última dirección horizontal no-nula (true = izquierda)
    private bool facingLeft = true;

    // Trackers para detectar transiciones de powerups
    private bool _wasPowerupEscoba;
    private bool _wasPowerupBuckbeak;

    // Tracker para detectar borde de pulsación del botón de disparo UI
    private bool _previousSpellButton = false;

    // UI Jump tracking: la UI llamará Salto()/saltoNot()
    private bool _uiJumpPressed = false;
    private bool _previousUIJump = false;

    // Propiedades que leen/escriben en GameManager (fuente de verdad)
    private bool PowerupEscoba
    {
        get => GameManager.Instance != null && GameManager.Instance.powerupescobabool;
        set { if (GameManager.Instance != null) GameManager.Instance.powerupescobabool = value; }
    }

    private bool PowerupBuckbeak
    {
        get => GameManager.Instance != null && GameManager.Instance.powerupbuckbeakbool;
        set { if (GameManager.Instance != null) GameManager.Instance.powerupbuckbeakbool = value; }
    }

    private int Vidas
    {
        get => GameManager.Instance != null ? GameManager.Instance.vidas : 0;
        set { if (GameManager.Instance != null) GameManager.Instance.vidas = value; }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        tr = transform;
        previousPosition = tr.position;

        if (GameManager.Instance != null) _previousGameState = GameManager.Instance.CurrentState;

        // Inicializar trackers según estado actual
        _wasPowerupEscoba = PowerupEscoba;
        _wasPowerupBuckbeak = PowerupBuckbeak;
    }

    void Update()
    {
        // Protección por estado del juego
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
        {
            DetectAndHandleStateChange();
            return;
        }

        // Entrada horizontal: solo joystick/Controller. No teclado.
        float axis = 0f;
        if (JS != null) axis = JS.JSmove.Horizontal;
        horizontalInput = axis;

        // Actualizar facing sólo cuando hay entrada horizontal no nula.
        if (Mathf.Abs(horizontalInput) > 0.01f)
        {
            bool newFacingLeft = horizontalInput < 0f;
            if (newFacingLeft != facingLeft)
            {
                facingLeft = newFacingLeft;
                // sincronizar con GameManager (fuente de verdad para orientación)
                if (GameManager.Instance != null) GameManager.Instance.SetFacing(facingLeft);
            }
        }

        // Up (apuntar arriba) -> lo determina joystick vertical (sin teclado)
        bool up = false;
        if (JS != null && JS.JSmove.Vertical > 0.95f && tr.position.y < 0f) up = true;
        if (GameManager.Instance != null) GameManager.Instance.Up = up;

        // Salto: UI (detectamos borde para evitar saltos repetidos por hold)
        bool uiJumpEdge = _uiJumpPressed && !_previousUIJump;
        if (uiJumpEdge && tr.position.y < 0 && !PowerupEscoba && !PowerupBuckbeak)
        {
            jumpRequested = true;
        }
        _previousUIJump = _uiJumpPressed;

        // Hechizo/animación de disparo -> edge detect sobre PlayerCombat.ButtonSpell
        bool uiSpell = PlayerCombat.Instance != null && PlayerCombat.Instance.ButtonSpell;
        bool spellPressed = (uiSpell && !_previousSpellButton);

        // Actualizamos animaciones cada frame (valores simples)
        UpdateAnimations(spellPressed);

        // actualizar tracker del botón para detectar borde en siguiente frame
        _previousSpellButton = uiSpell;

        // ---- Detectar transiciones de powerups y restaurar escala/gravidad si terminan ----
        bool escobaActive = PowerupEscoba;
        bool buckActive = PowerupBuckbeak;

        if (!escobaActive && _wasPowerupEscoba)
        {
            // La escoba terminó -> restaurar valores relevantes
            tr.localScale = escalaNormal;
            rb.gravityScale = 1f;
            if (anim != null) anim.SetBool("escoba", false);
            if (GameManager.Instance != null && GameManager.Instance.audioEscoba != null)
            {
                GameManager.Instance.audioEscoba.SetActive(false);
            }
        }

        if (!buckActive && _wasPowerupBuckbeak)
        {
            // Buckbeak terminó -> restaurar valores relevantes
            tr.localScale = escalaNormal;
            rb.gravityScale = 1f;
            if (anim != null) anim.SetBool("buckbeak", false);
            if (GameManager.Instance != null && GameManager.Instance.audioEscoba != null)
            {
                GameManager.Instance.audioEscoba.SetActive(false);
            }
        }

        // actualizar trackers
        _wasPowerupEscoba = escobaActive;
        _wasPowerupBuckbeak = buckActive;

        // detectar cambios de estado para resetear si es necesario
        DetectAndHandleStateChange();
    }

    void FixedUpdate()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;

        // Saltar (física)
        if (jumpRequested)
        {
            jumpRequested = false;
            if (tr.position.y < 0 && !PowerupEscoba && !PowerupBuckbeak)
            {
                rb.AddForce(Vector2.up * fuerzaSalto);
            }
        }

        // Movimiento horizontal: en suelo y en aire con multiplicador
        if (!PowerupEscoba && !PowerupBuckbeak)
        {
            float speed = velocidadSuelo * (tr.position.y > 0.01f ? multiplicadorAire : 1f);
            Vector2 targetVelocity = new Vector2(horizontalInput * speed, rb.velocity.y);
            rb.velocity = targetVelocity;
        }
        else
        {
            // Movimiento cuando estamos montados en escoba o Buckbeak:
            rb.velocity = Vector2.zero;
            float xInput = horizontalInput;
            float yInput = 0f;
            if (JS != null) yInput = JS.JSmove.Vertical;

            if (PowerupEscoba)
            {
                tr.localScale = escalaEscoba;
                rb.gravityScale = 0f;
                Vector3 delta = new Vector3(xInput * escobaMoveMultiplier * Time.fixedDeltaTime, yInput * escobaMoveMultiplier * Time.fixedDeltaTime, 0f);
                rb.MovePosition(tr.position + delta);
            }
            else if (PowerupBuckbeak)
            {
                tr.localScale = escalaBuckbeak;
                rb.gravityScale = 0f;
                Vector3 delta = new Vector3(xInput * buckbeakMoveMultiplier * Time.fixedDeltaTime, yInput * buckbeakMoveMultiplier * Time.fixedDeltaTime, 0f);
                rb.MovePosition(tr.position + delta);
            }
        }

        // Seguridad: si no hay powerups asegurar valores por defecto (evita quedarse escalado por errores)
        if (!PowerupEscoba && !PowerupBuckbeak)
        {
            tr.localScale = escalaNormal;
            rb.gravityScale = 1f;
        }

        previousPosition = tr.position;
    }

    private void UpdateAnimations(bool spellPressed)
    {
        if (anim == null) return;

        // salto: basamos en la altura
        bool enAire = tr.position.y > 0f;
        anim.SetBool("salto", enAire);

        // powerups animaciones
        anim.SetBool("escoba", PowerupEscoba);
        anim.SetBool("buckbeak", PowerupBuckbeak);

        // correr / orientación
        bool movingRight = horizontalInput > 0f;
        bool movingLeft = horizontalInput < 0f;

        anim.SetBool("runRight", movingRight && !PowerupEscoba && !PowerupBuckbeak);
        anim.SetBool("runLeft", movingLeft && !PowerupEscoba && !PowerupBuckbeak);
        anim.SetBool("goLeft", facingLeft);

        // disparo: activamos solo cuando se produce el evento de pulsación (edge)
        if (spellPressed)
        {
            anim.SetBool("disparoL", facingLeft);
            anim.SetBool("disparoR", !facingLeft);
        }
        else
        {
            anim.SetBool("disparoL", false);
            anim.SetBool("disparoR", false);
        }
    }

    private void DetectAndHandleStateChange()
    {
        if (GameManager.Instance == null) return;

        GameState current = GameManager.Instance.CurrentState;
        if (_previousGameState.HasValue && _previousGameState.Value != current)
        {
            if (current == GameState.Menu || current == GameState.GameOver)
            {
                ResetPlayerToDefault();
            }
        }
        _previousGameState = current;
    }

    private void ResetPlayerToDefault()
    {
        // Restaurar valores por defecto
        tr.localScale = escalaNormal;
        rb.gravityScale = 1f;
        rb.velocity = Vector2.zero;
        jumpRequested = false;
        horizontalInput = 0f;

        if (anim != null)
        {
            anim.SetBool("escoba", false);
            anim.SetBool("buckbeak", false);
            anim.SetBool("runLeft", false);
            anim.SetBool("runRight", false);
            anim.SetBool("disparoL", false);
            anim.SetBool("disparoR", false);
            anim.SetBool("salto", false);
            anim.SetBool("goLeft", false);
        }

        // Reset trackers
        _wasPowerupEscoba = false;
        _wasPowerupBuckbeak = false;
    }

    // Métodos públicos para que la UI asigne eventos PointerDown/PointerUp
    public void Salto()
    {
        _uiJumpPressed = true;
    }

    public void saltoNot()
    {
        _uiJumpPressed = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (GameManager.Instance == null) return;

        string tag = collision.gameObject.tag;
        switch (tag)
        {
            case "Dementor":
                if (!PowerupBuckbeak)
                {
                    Vidas = Mathf.Max(0, Vidas - 1);
                    if (GameManager.Instance.audiovidamenos != null) GameManager.Instance.audiovidamenos.SetActive(true);
                }
                break;

            case "Vidas":
                if (Vidas < 3)
                {
                    Vidas++;
                }
                break;

            case "Escoba":
                PowerupEscoba = true;
                GameManager.Instance.Escoba();
                if (GameManager.Instance.audioEscoba != null) GameManager.Instance.audioEscoba.SetActive(true);
                break;

            case "Patronus":
                GameManager.Instance.Patronus();
                break;

            case "Buckbeak":
                PowerupBuckbeak = true;
                GameManager.Instance.Buckbeak();
                if (GameManager.Instance.audioEscoba != null) GameManager.Instance.audioEscoba.SetActive(true);
                break;

            default:
                break;
        }
    }
}
