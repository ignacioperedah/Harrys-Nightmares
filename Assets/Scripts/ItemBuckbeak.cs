using UnityEngine;

public class ItemBuckbeak : CollectibleBase
{
    public override void OnCollect()
    {
        base.OnCollect();
        var gm = GameManager.Instance;
        if (gm != null)
        {
            gm.powerupbuckbeakbool = true;
            gm.Buckbeak();
        }

        // Desregistrar del spawner si existe (uso del singleton)
        PowerUpSpawner.Instance?.Unregister(gameObject);

        Destroy(gameObject);
    }
}