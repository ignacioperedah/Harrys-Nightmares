using UnityEngine;

public class ItemBuckbeak : CollectibleBase
{
    public override void OnCollect()
    {
        base.OnCollect();
        PowerUpHandler.Instance?.ActivateBuckbeak();
        PowerUpSpawner.Instance?.Unregister(gameObject);
        Destroy(gameObject);
    }
}