using UnityEngine;

public class ItemVida : CollectibleBase
{
    public override void OnCollect()
    {
        base.OnCollect();
        PowerUpHandler.Instance?.ActivateVida();
        PowerUpSpawner.Instance?.Unregister(gameObject);
        Destroy(gameObject);
    }
}