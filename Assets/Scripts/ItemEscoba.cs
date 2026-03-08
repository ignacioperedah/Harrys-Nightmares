using UnityEngine;

public class ItemEscoba : CollectibleBase
{
    public override void OnCollect()
    {
        base.OnCollect();
        PowerUpHandler.Instance?.ActivateEscoba();
        PowerUpSpawner.Instance?.Unregister(gameObject);
        Destroy(gameObject);
    }
}