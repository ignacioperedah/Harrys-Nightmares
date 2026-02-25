using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Controller : MonoBehaviour
{
    public Transform player;
    public Joystick JSmove;
    public Rigidbody2D rb;
    public GameObject background;

    public GameManager gm;

    public float speed = 5f;
    public float prueba;
    public float prueba1;

    private void Start()
    {
    }

    void Move()
    {
        rb.velocity = new Vector3(JSmove.Horizontal * speed, rb.velocity.y, 0f);
    }

    private void Update()
    {
        prueba = JSmove.Horizontal;
        prueba1 = JSmove.Vertical;

        if(gm.vidas < 1)
        {
            JSmove.input = Vector2.zero;
            background.SetActive(false);
        }
        else 
        {
            Move();
        }

    }
}