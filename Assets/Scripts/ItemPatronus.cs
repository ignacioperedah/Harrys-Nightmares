public class ItemPatronus : CollectibleBase
{
    public override void OnCollect()
    {
        base.OnCollect();
        var gm = GameManager.Instance;
        if (gm != null)
        {
            gm.Patronus();
        }

        // Desregistrar del spawner si existe (uso del singleton)
        PowerUpSpawner.Instance?.Unregister(gameObject);

        Destroy(gameObject);
    }
}