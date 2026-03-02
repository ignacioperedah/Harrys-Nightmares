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
    // Configuración a través de ScriptableObject (fallbacks si no está asignado)
    [Header("Configuración (PlayerStats)")]
    [SerializeField] private PlayerStats stats;

    [Header("Escalas")]
    [SerializeField] private Vector2 escalaNormal = new Vector2(5f, 5f);
    [SerializeField] private Vector2 escalaEscoba = new Vector2(4f, 4f);
    [SerializeField] private Vector2 escalaBuckbeak = new Vector2(2.25f, 2.25f);

    // Referencias
    [SerializeField] private Joystick JSmove; // ahora el Joystick está directamente aquí
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

    // Trackers de solicitud para aplicar cambios de Rigidbody en FixedUpdate
    private bool restoreFromEscobaRequested;
    private bool restoreFromBuckbeakRequested;
    private bool resetDefaultsRequested;

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

    // Accesos seguros a stats con fallback a valores previos para no romper escenas existentes
    private float FuerzaSalto => stats != null ? stats.fuerzaSalto : 400f;
    private float VelocidadSuelo => stats != null ? stats.velocidadSuelo : 10f;
    private float MultiplicadorAire => stats != null ? stats.multiplicadorAire : 0.5f;
    private float EscobaMoveMultiplier => stats != null ? stats.escobaMoveMultiplier : 0.1f;
    private float BuckbeakMoveMultiplier => stats != null ? stats.buckbeakMoveMultiplier : 0.15f;

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

        // Si el jugador está sin vidas: anular input del joystick (antes lo hacía Controller)
        if (Vidas < 1)
        {
            if (JSmove != null) JSmove.input = Vector2.zero;
            // No tocar el background aquí: UIManager / GameManager gestionan la visibilidad
            DetectAndHandleStateChange();
            return;
        }

        // Entrada horizontal: solo joystick. No teclado.
        float axis = 0f;
        if (JSmove != null) axis = JSmove.Horizontal;
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
        if (JSmove != null && JSmove.Vertical > 0.95f && tr.position.y < 0f) up = true;
        if (GameManager.Instance != null) GameManager.Instance.Up = up;

        // Salto: UI (detectamos borde para evitar saltos repetidos por hold)
        bool uiJumpEdge = _uiJumpPressed && !_previousUIJump;
        if (uiJumpEdge && tr.position.y < 0 && !PowerupEscoba && !PowerupBuckbeak)
        {
            // Solo marcamos la intención; la física se aplica en FixedUpdate
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

        // ---- Detectar transiciones de powerups y solicitar restauración (aplicar en FixedUpdate) ----
        bool escobaActive = PowerupEscoba;
        bool buckActive = PowerupBuckbeak;

        if (!escobaActive && _wasPowerupEscoba)
        {
            // La escoba terminó -> solicitar restauración de gravedad y escala en FixedUpdate
            restoreFromEscobaRequested = true;
        }

        if (!buckActive && _wasPowerupBuckbeak)
        {
            // Buckbeak terminó -> solicitar restauración de gravedad y escala en FixedUpdate
            restoreFromBuckbeakRequested = true;
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

        // Aplicar solicitudes de restauración solicitadas en Update (modifican Rigidbody/transform físicos)
        if (restoreFromEscobaRequested)
        {
            restoreFromEscobaRequested = false;
            tr.localScale = escalaNormal;
            rb.gravityScale = 1f;
            // animación se mantiene sincronizada por UpdateAnimations
        }

        if (restoreFromBuckbeakRequested)
        {
            restoreFromBuckbeakRequested = false;
            tr.localScale = escalaNormal;
            rb.gravityScale = 1f;
        }

        if (resetDefaultsRequested)
        {
            resetDefaultsRequested = false;
            tr.localScale = escalaNormal;
            rb.gravityScale = 1f;
            rb.velocity = Vector2.zero;
        }

        // Saltar (física) - siempre en FixedUpdate
        if (jumpRequested)
        {
            // Consumir la petición de salto aquí (la petición se generó en Update)
            jumpRequested = false;
            if (tr.position.y < 0 && !PowerupEscoba && !PowerupBuckbeak)
            {
                rb.AddForce(Vector2.up * FuerzaSalto);
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("Jump");
            }
        }

        // Movimiento horizontal: en suelo y en aire con multiplicador
        if (!PowerupEscoba && !PowerupBuckbeak)
        {
            float speed = VelocidadSuelo * (tr.position.y > 0.01f ? MultiplicadorAire : 1f);
            Vector2 targetVelocity = new Vector2(horizontalInput * speed, rb.velocity.y);
            rb.velocity = targetVelocity;
        }
        else
        {
            // Movimiento cuando estamos montados en escoba o Buckbeak:
            rb.velocity = Vector2.zero;
            float xInput = horizontalInput;
            float yInput = 0f;
            if (JSmove != null) yInput = JSmove.Vertical;

            if (PowerupEscoba)
            {
                tr.localScale = escalaEscoba;
                rb.gravityScale = 0f;
                Vector3 delta = new Vector3(xInput * EscobaMoveMultiplier * Time.fixedDeltaTime, yInput * EscobaMoveMultiplier * Time.fixedDeltaTime, 0f);
                rb.MovePosition(tr.position + delta);
            }
            else if (PowerupBuckbeak)
            {
                tr.localScale = escalaBuckbeak;
                rb.gravityScale = 0f;
                Vector3 delta = new Vector3(xInput * BuckbeakMoveMultiplier * Time.fixedDeltaTime, yInput * BuckbeakMoveMultiplier * Time.fixedDeltaTime, 0f);
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
        // Restaurar valores por defecto (no aplicar Rigidbody directamente aquí)
        tr.localScale = escalaNormal;
        // Solicitar al FixedUpdate que haga los cambios físicos seguros
        resetDefaultsRequested = true;

        // Reset de estado no-físico
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

        // Intentar usar la interfaz ICollectible para objetos recolectables
        var collectible = collision.gameObject.GetComponent<CollectibleBase>();
        if (collectible != null)
        {
            collectible.OnCollect();
            return;
        }

        // Lógica para colisiones que NO son items (ej. enemigos)
        if (collision.gameObject.CompareTag("Dementor"))
        {
            if (!PowerupBuckbeak)
            {
                Vidas = Mathf.Max(0, Vidas - 1);
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("VidaMenos");
            }
        }
    }
}
