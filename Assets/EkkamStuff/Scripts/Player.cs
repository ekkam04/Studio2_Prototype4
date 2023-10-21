using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    Rigidbody rb;
    Animator anim;

    public Transform orientation;
    public Transform cameraObj;
    public float rotationSpeed = 3f;
    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;
    public float speed = 1.0f;
    public float maxSpeed = 5.0f;
    public float groundDrag = 3f;

    public float groundDistance = 0.2f;
    public bool isGrounded;
    public bool isJumping;
    public bool allowDoubleJump;
    public bool doubleJumped;
    public bool hasLanded;

    public float jumpHeightApex = 2f;
    public float jumpDuration = 1f;
    float currentJumpDuration;
    public float downwardsGravityMultiplier = 1f;
    float gravity;
    float initialJumpVelocity;
    float jumpStartTime;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        ChangePlayerColor(new Color32(0, 0, 255, 255));
    }

    void Update()
    {
        // Movement
        // Vector3 viewDirection = transform.position - new Vector3(cameraObj.position.x, transform.position.y, cameraObj.position.z);
        // orientation.forward = viewDirection.normalized;
        orientation.forward = Vector3.zero;

        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
         
        if(moveDirection != Vector3.zero)
        {
            transform.forward = Vector3.Slerp(transform.forward, moveDirection.normalized, Time.deltaTime * rotationSpeed);
            anim.SetBool("isMoving", true);
        }
        else
        {
            anim.SetBool("isMoving", false);
        }

        // Control speed
        ControlSpeed();

        // Ground check
        RaycastHit hit;
        if (Physics.Raycast(transform.position + new Vector3(0, 0.5f, 0), Vector3.down, out hit, groundDistance + 0.1f))
        {
            isGrounded = true;
            rb.drag = groundDrag;

            if (!hasLanded)
            {
                hasLanded = true;
            }
        }
        else
        {
            isGrounded = false;
            rb.drag = 0;
        }
        Debug.DrawRay(transform.position + new Vector3(0, 1, 0), Vector3.down * (groundDistance + 0.1f), Color.red);

        // Jump
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isGrounded && allowDoubleJump && !doubleJumped)
            {
                doubleJumped = true;
                StartJump(jumpHeightApex, jumpDuration);
            }
            else if (isGrounded)
            {
                doubleJumped = false;
                StartJump(jumpHeightApex, jumpDuration);
            }
        }

    }

    void FixedUpdate()
    {
        // Move player
        MovePlayer();

        // Jumping
        if (isJumping)
        {
            rb.AddForce(Vector3.up * gravity, ForceMode.Acceleration);

            if (Time.time - jumpStartTime >= currentJumpDuration)
            {
                isJumping = false;
                hasLanded = false;
            }
        }
        else
        {
            rb.AddForce(Vector3.down * -gravity * downwardsGravityMultiplier, ForceMode.Acceleration);
        }
    }

    void MovePlayer()
    {
        // Calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        rb.AddForce(moveDirection * speed * 10f, ForceMode.Force);
    }

    void ControlSpeed()
    {
        // Limit velocity if needed
        Vector3 flatVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        if (flatVelocity.magnitude > maxSpeed)
        {
            Vector3 limitedVelocity = flatVelocity.normalized * maxSpeed;
            rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
        }
    }

    void StartJump(float heightApex, float duration)
    {
        // Recalculate gravity and initial velocity
        gravity = -2 * heightApex / (duration * duration);
        initialJumpVelocity = Mathf.Abs(gravity) * duration;
        currentJumpDuration = duration;

        isJumping = true;
        anim.SetBool("isJumping", true);
        jumpStartTime = Time.time;
        rb.velocity = Vector3.up * initialJumpVelocity;
    }

    public void ChangePlayerColor(Color32 color)
    {
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Renderer>() != null)
            {
                Material playerMaterial = new Material(child.GetComponent<Renderer>().material);
                child.GetComponent<Renderer>().material = playerMaterial;
                playerMaterial.SetColor("_Color01", color);
                playerMaterial.SetColor("_Color02", color);
                playerMaterial.SetColor("_Color03", color);
            }
        }
    }
}
