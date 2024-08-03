using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.Compilation;
using UnityEditor.Tilemaps;
using UnityEngine;

public class Player : MonoBehaviour
{

    private Rigidbody2D rb;
    private Animator anim;
    private CapsuleCollider2D cd;

    private bool canBeControlled=false;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float doubleJumpForce;
    private float defaultGravityScale;
    private bool canDoubleJump;


    [Header("Buffer & Coyote jump")]
    [SerializeField] private float bufferJumpWindow=0.25f;
    private float bufferJumpActivated = -1;
    [SerializeField] private float coyoteJumpWindow = 0.5f;
    private float coyoteJumpActivated = -1;

    [Header("Wall interactions")]
    [SerializeField] private float wallJumpDuration = 0.6f;
    [SerializeField] private Vector2 wallJumpForce;
    private bool isWallJumping;



    [Header("Konckback")]
    [SerializeField] private float knockbackDuration = 1;
    [SerializeField] private Vector2 knockbackPower;
    private bool isKnocked;

   


    [Header("Collision ")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private LayerMask whatIsGround;
    private bool isGrounded;
    private bool isAirborne;
    private bool isWallDetected;

    private float xInput;
    private float yInput;

    private bool facingRight = true;
    private int facingDir = 1;


    [Header("VFX")]
    [SerializeField] private GameObject deathVfx;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cd = GetComponent<CapsuleCollider2D>();
        anim = GetComponentInChildren<Animator>();

       
    }


    private void Start()
    {
        defaultGravityScale = rb.gravityScale;
        RespawnFinished(false);
    }


    private void Update()
    {


        UpdateAirbornStatus();

        if (canBeControlled == false)
            return;

        if (isKnocked)
            return;


        HandelInput();
        HandleWallSlide();
        HandleMovement();
        HandleFlip();
        HandleCollision();
        HandleAnimations();

        
    }

    public void RespawnFinished(bool finished)
    {
        

        if (finished)
        {
            rb.gravityScale = defaultGravityScale;
            canBeControlled = true;
            cd.enabled = true;
        }
        else
        {
            rb.gravityScale = 0;
            canBeControlled = false;
            cd.enabled = false;
        }
    }



    public void Knockback(float sourceDamageXPosition)
    {
        float knockbackDir = 1;
        

        if (transform.position.x < sourceDamageXPosition)
            knockbackDir = -1;


        if (isKnocked)
            return;

        StartCoroutine(KnockbackRoutine());
        
        rb.velocity = new Vector2(knockbackPower.x * knockbackDir, knockbackPower.y);
    }


    public void Die()
    {
        GameObject newDeathVfx = Instantiate(deathVfx, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }


    private IEnumerator KnockbackRoutine()
    {

        isKnocked = true;
        anim.SetBool("isKnocked", true);

        yield return new WaitForSeconds(knockbackDuration);


        isKnocked = false;
        anim.SetBool("isKnocked", false);
    }


    private void UpdateAirbornStatus()
    {
        //the conditional 'isAirborne' is to make sure the if block
        //just work once
        if (isGrounded && isAirborne)
            HandleLanding();

        if (!isGrounded && !isAirborne)
            BecomeAirborne();
    }

    private void BecomeAirborne()
    {
        isAirborne = true;

        if (rb.velocity.y < 0)
        
            ActivateCoyoteJump();
        
    }

    private void HandleLanding()
    {
        isAirborne = false;
        canDoubleJump = true;

        AttemptBufferJump();
    }

    

    private void HandelInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            JumpButton();
            RequestBufferJump();
        }
            
            
        
        
    }

    #region Buffer & Coyote Jump
    private void RequestBufferJump()
    {
        if (isAirborne)
            bufferJumpActivated = Time.time;
    }

    private void AttemptBufferJump()
    {
        if (Time.time < bufferJumpActivated + bufferJumpWindow)
        {
            bufferJumpActivated = Time.time - 1;
            Jump();
        }

    }


    private void ActivateCoyoteJump() =>coyoteJumpActivated= Time.time;
    private void CancelCoyoteJump() => coyoteJumpActivated = Time.time - 1;


    #endregion
    private void JumpButton()
    {

        bool coyoteJumpAvailable = Time.time < coyoteJumpActivated + coyoteJumpWindow;
        if (isGrounded||coyoteJumpAvailable) {
           
            Jump();
        }
        else if (isWallDetected && !isGrounded)
        {
            WallJump();
        }
        else if (canDoubleJump)
        {
            DoubleJump();
            
        }

        CancelCoyoteJump();
    }

    private void Jump() =>rb.velocity = new Vector2(rb.velocity.x, jumpForce);

    private void DoubleJump()
    {
        isWallJumping = false;
        canDoubleJump = false;
        rb.velocity = new Vector2(rb.velocity.x, doubleJumpForce);
    }

    private void WallJump()
    {
        canDoubleJump = true;
        rb.velocity = new Vector2(wallJumpForce.x * -facingDir, wallJumpForce.y);
        Flip();

        StopAllCoroutines();
        StartCoroutine(WallJumpRoutine());
    }

    private void HandleWallSlide()
    {
        bool canWallSlide = isWallDetected && rb.velocity.y < 0;

        float yModifer = yInput < 0 ? 1 : 0.5f;

        if (canWallSlide == false)
            return;




        rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * yModifer);

    }
    private IEnumerator WallJumpRoutine()
    {
        isWallJumping = true;

        yield return new WaitForSeconds(wallJumpDuration);

        isWallJumping = false;
    }


    private void HandleCollision()
    {
        
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
        isWallDetected = Physics2D.Raycast(transform.position, Vector2.right * facingDir, wallCheckDistance, whatIsGround);
    }

    private void HandleAnimations()
    {
        anim.SetFloat("xVelocity", rb.velocity.x );
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isWallDetected", isWallDetected);
    }

    private void HandleMovement()
    {   
        
        //if delete this if block,
        //then when the idle touches the wall,he will not stop
        if (isWallDetected)
            return;

        if (isWallJumping)
            return;

        rb.velocity = new Vector2(xInput * moveSpeed, rb.velocity.y);
        //former is x,later is y
    }
    private void HandleFlip()
    {
        
        if (xInput < 0 && facingRight || xInput > 0 && !facingRight)
            Flip();
    }
    private void Flip()
    {
        facingDir = facingDir * -1;
        transform.Rotate(0, 180, 0);
        facingRight = !facingRight;
    }


    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y-groundCheckDistance));
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x + (wallCheckDistance * facingDir), transform.position.y));
    }
}
