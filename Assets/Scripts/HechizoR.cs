using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HechizoR : MonoBehaviour
{
    public float Right;
    public GameObject patronumR;
    public Transform harry;

    public bool hitdemen = false;
    private Rigidbody2D rb;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    public void OnEnable()
    {
        GameObject[] noCollision = GameObject.FindGameObjectsWithTag("Pared");
        GameObject[] noCollision2 = GameObject.FindGameObjectsWithTag("Harry");
        GameObject[] noCollision3 = GameObject.FindGameObjectsWithTag("Vidas");
        GameObject[] collisionDementor = GameObject.FindGameObjectsWithTag("Dementor");

        foreach (GameObject HechizoR in noCollision)
        {
            Physics2D.IgnoreCollision(HechizoR.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }

        foreach (GameObject HechizoR in noCollision2)
        {
            Physics2D.IgnoreCollision(HechizoR.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }

        foreach (GameObject HechizoR in noCollision3)
        {
            Physics2D.IgnoreCollision(HechizoR.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }

        foreach (GameObject HechizoR in collisionDementor)
        {
            hitdemen = Physics2D.IsTouching(HechizoR.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }
    }

    // Update is called once per frame
    public void Update()
    {
        if (transform.position.x > -12 && transform.position.x < 12)
        {
            rb.AddForce(new Vector2(Right, 0));
        }

        if (transform.position.x < -12 || transform.position.x > 12 || hitdemen == true)
        {
            Destroy(patronumR);
        }

        GameObject[] noCollision = GameObject.FindGameObjectsWithTag("Pared");
        GameObject[] noCollision2 = GameObject.FindGameObjectsWithTag("Harry");
        GameObject[] noCollision3 = GameObject.FindGameObjectsWithTag("Vidas");
        GameObject[] collisionDementor = GameObject.FindGameObjectsWithTag("Dementor");

        foreach (GameObject HechizoR in noCollision)
        {
            Physics2D.IgnoreCollision(HechizoR.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }

        foreach (GameObject HechizoR in noCollision2)
        {
            Physics2D.IgnoreCollision(HechizoR.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }

        foreach (GameObject HechizoR in noCollision3)
        {
            Physics2D.IgnoreCollision(HechizoR.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }

        foreach (GameObject HechizoR in collisionDementor)
        {
            hitdemen = Physics2D.IsTouching(HechizoR.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Dementor"))
        {
            hitdemen = true;
        }
    }
}
