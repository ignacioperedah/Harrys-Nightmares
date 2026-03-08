public class ItemPatronus : CollectibleBase
{
    public override void OnCollect()
    {
        base.OnCollect();
        PowerUpHandler.Instance?.ActivatePatronus();
        PowerUpSpawner.Instance?.Unregister(gameObject);
        Destroy(gameObject);
    }
}