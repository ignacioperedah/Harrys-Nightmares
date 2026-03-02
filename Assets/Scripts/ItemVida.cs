public class ItemVida : CollectibleBase
{
    public override void OnCollect()
    {
        base.OnCollect();
        var gm = GameManager.Instance;
        if (gm != null)
        {
            if (gm.vidas < 3) gm.vidas++;
        }

        // Desregistrar del spawner si existe (uso del singleton)
        PowerUpSpawner.Instance?.Unregister(gameObject);

        Destroy(gameObject);
    }
}