using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class Player : MonoBehaviour
{

    private Rigidbody2D rb;
    private Animator anim;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;


    [Header("Collision info")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private LayerMask whatIsGround;
    private bool isGrounded;
    

    private float xInput;
   
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();  
        anim = GetComponentInChildren<Animator>();
    }

    private void Update()
    {


        if (Input.GetKeyDown(KeyCode.C))
            Flip();
        HandleCollision();
        HandelInput();
        HandleMovement();
        HandleAnimations();
    }

    private void HandelInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");


        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)  
            Jump();
            
        
        
    }

    private void Jump() =>rb.velocity = new Vector2(rb.velocity.x, jumpForce);


    private void HandleCollision()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);

    }

    private void HandleAnimations()
    {
        anim.SetFloat("xVelocity", rb.velocity.x );
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool("isGrounded", isGrounded);
    }

    private void HandleMovement()
    {

        rb.velocity = new Vector2(xInput * moveSpeed, rb.velocity.y);
        //former is x,later is y
    }

    private void Flip()
    {
        transform.Rotate(0, 180, 0);
    }


    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y-groundCheckDistance));
    }
}
