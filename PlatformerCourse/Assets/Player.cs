using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    private Rigidbody2D rb;

    [SerializeField] private float moveSpeed;

    private float xInput;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();  
    }

    private void Update()
    {
        xInput = Input.GetAxisRaw("Horizontal");

        HandleMovement();

    }

    private void HandleMovement()
    {


        rb.velocity = new Vector2(xInput * moveSpeed, rb.velocity.y);
    }
}
