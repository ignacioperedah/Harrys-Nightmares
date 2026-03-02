using UnityEngine;

public class CollectibleBase : MonoBehaviour
{
    void Update()
    {
        if (transform.position.y < -10 || transform.position.y > 10) Destroy(gameObject);
    }
    public virtual void OnCollect()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("Pickup");
    } 
}