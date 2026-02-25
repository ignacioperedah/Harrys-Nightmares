using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuckBeak : MonoBehaviour
{
    public GameObject buckbeak;
    public PolygonCollider2D polygonCollider;

    private Rigidbody2D rb;

    bool powerup = false;

    // Start is called before the first frame update
    public void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        polygonCollider = GetComponent<PolygonCollider2D>();
    }

    // Update is called once per frame
    public void Update()
    {
        if (transform.position.y < -10 || transform.position.y > 10)
        {
            Destroy(buckbeak);
        }

        if (powerup == true)
        {
            Destroy(buckbeak);
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Harry"))
        {
            powerup = true;
        }
    }
}