using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HechizoL : MonoBehaviour
{
    public float Left;
    public GameObject patronumL;
    public Transform harry;

    private Rigidbody2D rb;

    public bool hitdemen = false;
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

        foreach (GameObject HechizoL in noCollision)
        {
            Physics2D.IgnoreCollision(HechizoL.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }

        foreach (GameObject HechizoL in noCollision2)
        {
            Physics2D.IgnoreCollision(HechizoL.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }

        foreach (GameObject HechizoL in noCollision3)
        {
            Physics2D.IgnoreCollision(HechizoL.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }

        foreach (GameObject HechizoL in collisionDementor)
        {
            hitdemen = Physics2D.IsTouching(HechizoL.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }
    }

    // Update is called once per frame
    public void Update()
    {
        if (transform.position.x > -12 && transform.position.x < 12)
        {
            rb.AddForce(new Vector2(Left, 0));
        }

        if (transform.position.x < -12 || transform.position.x > 12 || hitdemen == true)
        {
            Destroy(patronumL);
        }

        GameObject[] noCollision = GameObject.FindGameObjectsWithTag("Pared");
        GameObject[] noCollision2 = GameObject.FindGameObjectsWithTag("Harry");
        GameObject[] noCollision3 = GameObject.FindGameObjectsWithTag("Vidas");
        GameObject[] collisionDementor = GameObject.FindGameObjectsWithTag("Dementor");

        foreach (GameObject HechizoL in noCollision)
        {
            Physics2D.IgnoreCollision(HechizoL.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }

        foreach (GameObject HechizoL in noCollision2)
        {
            Physics2D.IgnoreCollision(HechizoL.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }

        foreach (GameObject HechizoL in noCollision3)
        {
            Physics2D.IgnoreCollision(HechizoL.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }

        foreach (GameObject HechizoL in collisionDementor)
        {
            hitdemen = Physics2D.IsTouching(HechizoL.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }
    }
    public void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Dementor"))
        {
            hitdemen = true;
        }
    }
}    
