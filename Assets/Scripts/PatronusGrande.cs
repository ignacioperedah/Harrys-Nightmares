using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatronusGrande : MonoBehaviour
{
    public float Right;
    public GameObject patronumR;
    private Rigidbody2D rb;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    public void Update()
    {
        if (transform.position.x > -12 && transform.position.x < 12)
        {
            rb.AddForce(new Vector2(Right, 0));
        }

        if (transform.position.x < -12 || transform.position.x > 12)
        {
            Destroy(patronumR);
        }
    }
}
