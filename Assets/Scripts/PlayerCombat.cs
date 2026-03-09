using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Componente responsable del combate del jugador:
/// - Contiene referencia a un único prefab de proyectil (serializada).
/// - Implementa un pool simple (lista con búsqueda de objetos inactivos).
/// - Expone API para botones: Hechizo() / hechizoNot().
/// - No usa entrada de teclado (Android-only): spawn sólo vía UI/Joystick.
/// </summary>
[DisallowMultipleComponent]
public class PlayerCombat : MonoBehaviour
{
    public static PlayerCombat Instance { get; private set; }

    [Header("Projectile Prefab")]
    [SerializeField] private GameObject patronum;

    [Header("Pooling")]
    [SerializeField] private int initialPoolSize = 8;

    private List<GameObject> pool = new List<GameObject>();

    // Estado de botón (UI) expuesto para PlayerController
    public bool ButtonSpell { get; private set; } = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Pre-warm pool
        WarmPool(patronum, pool);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // No hay lectura de teclado aquí: disparo solo por UI (Hechizo) o llamada desde otros sistemas
    // API para botones UI (movidas desde GameManager)
    public void Hechizo()
    {
        if (GameManager.Instance == null) return;
        ButtonSpell = true;
        if (GameManager.Instance.CurrentState == GameState.Playing)
        {
            TrySpawnSpell();
        }
    }

    public void hechizoNot()
    {
        ButtonSpell = false;
    }

    // Intenta seleccionar posición de proyectil y spawnearlo según estado/powerups/facing
    private void TrySpawnSpell()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.CurrentState != GameState.Playing) return;

        var gm = GameManager.Instance;
        var powerUpHandler = PowerUpHandler.Instance;
        Vector3 basePos = gm.harry != null ? gm.harry.position : transform.position;

        bool facingLeft = gm.IsFacingLeft();
        bool up = gm.Up;
        bool salto = gm.salto;
        bool powerupEscoba = powerUpHandler.PowerupEscobaBool;
        bool powerupBuck = powerUpHandler.PowerupBuckbeakBool;

        // Cuando no hay powerups de movimiento
        if (!powerupEscoba && !powerupBuck)
        {
            if (facingLeft && !up)
            {
                SpawnProjectileFromPool(basePos + new Vector3(-0.6f, -0.1f, 0), Vector2.left);
            }
            else if (!facingLeft && !up)
            {
                SpawnProjectileFromPool(basePos + new Vector3(0.6f, -0.1f, 0), Vector2.right);
            }
            else if (up && !salto)
            {
                Vector3 offset = facingLeft ? new Vector3(-0.2f, 0.7f, 0) : new Vector3(0.2f, 0.7f, 0);
                SpawnProjectileFromPool(basePos + offset, Vector2.up);
            }
        }
        else
        {
            // Powerup de movimiento cambia offsets y solo dispara horizontal
            if (facingLeft)
            {
                SpawnProjectileFromPool(basePos + new Vector3(-1.6f, 0.1f, 0), Vector2.left);
            }
            else
            {
                SpawnProjectileFromPool(basePos + new Vector3(1.6f, 0.1f, 0), Vector2.right);
            }
        }
    }

    // Pool helpers
    private void WarmPool(GameObject prefab, List<GameObject> pool)
    {
        if (prefab == null) return;
        for (int i = 0; i < Mathf.Max(1, initialPoolSize); i++)
        {
            GameObject inst = Instantiate(prefab);
            inst.SetActive(false);
            pool.Add(inst);
        }
    }

    private void SpawnProjectileFromPool(Vector3 pos, Vector2 dir)
    {
        if (patronum == null) return;

        GameObject instance = GetInactiveFromPool(pool);
        if (instance == null)
        {
            // ampliar pool si no hay inactivos
            instance = Instantiate(patronum);
            pool.Add(instance);
        }

        instance.transform.position = pos;
        instance.transform.rotation = Quaternion.identity;
        instance.SetActive(true);

        var proj = instance.GetComponent<Projectile>();
        if (proj != null) proj.Init(dir);
    }

    private GameObject GetInactiveFromPool(List<GameObject> pool)
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (pool[i] == null) continue;
            if (!pool[i].activeInHierarchy) return pool[i];
        }
        return null;
    }
}