using UnityEngine;

public class ItemEscoba : CollectibleBase
{
    public override void OnCollect()
    {
        base.OnCollect();
        var gm = GameManager.Instance;
        if (gm != null)
        {
            gm.powerupescobabool = true;
            gm.Escoba();
        }

        // Desregistrar del spawner si existe (uso del singleton)
        PowerUpSpawner.Instance?.Unregister(gameObject);

        Destroy(gameObject);
    }
}