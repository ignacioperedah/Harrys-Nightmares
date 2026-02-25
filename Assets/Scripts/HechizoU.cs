using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HechizoU : MonoBehaviour
{
    public float Up;
    public GameObject patronumU;
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

        foreach (GameObject HechizoU in noCollision)
        {
            Physics2D.IgnoreCollision(HechizoU.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }

        foreach (GameObject HechizoU in noCollision2)
        {
            Physics2D.IgnoreCollision(HechizoU.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }

        foreach (GameObject HechizoU in noCollision3)
        {
            Physics2D.IgnoreCollision(HechizoU.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }

        foreach (GameObject HechizoU in collisionDementor)
        {
            hitdemen = Physics2D.IsTouching(HechizoU.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }

    }

    // Update is called once per frame
    public void Update()
    {
        if (transform.position.x > -12 && transform.position.x < 12)
        {
            rb.AddForce(new Vector2(0, Up));
        }

        if (transform.position.y < -10 || transform.position.y > 10 || hitdemen == true)
        {
            Destroy(patronumU);
        }

        GameObject[] noCollision = GameObject.FindGameObjectsWithTag("Pared");
        GameObject[] noCollision2 = GameObject.FindGameObjectsWithTag("Harry");
        GameObject[] noCollision3 = GameObject.FindGameObjectsWithTag("Vidas");
        GameObject[] collisionDementor = GameObject.FindGameObjectsWithTag("Dementor");

        foreach (GameObject HechizoU in noCollision)
        {
            Physics2D.IgnoreCollision(HechizoU.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }

        foreach (GameObject HechizoU in noCollision2)
        {
            Physics2D.IgnoreCollision(HechizoU.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }

        foreach (GameObject HechizoU in noCollision3)
        {
            Physics2D.IgnoreCollision(HechizoU.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }

        foreach (GameObject HechizoU in collisionDementor)
        {
            hitdemen = Physics2D.IsTouching(HechizoU.GetComponent<Collider2D>(), GetComponent<Collider2D>());
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