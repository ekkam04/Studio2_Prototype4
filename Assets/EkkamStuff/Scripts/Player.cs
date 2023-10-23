using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class Player : MonoBehaviour
{
    public Rigidbody rb;
    public Animator anim;

    public int playerNumber;
    public bool allowMovement = true;
    public bool isReady = false;
    public bool isEliminated = false;
    public bool allowFall = true;

    public Transform orientation;
    public Transform cameraObj;
    public float rotationSpeed = 3f;
    public float horizontalInput = 0f;
    public float verticalInput = 0f;
    Vector3 moveDirection;
    public float speed = 1.0f;
    public float maxSpeed = 5.0f;
    public float groundDrag = 3f;

    public float groundDistance = 0.2f;
    public float groundDistanceLandingOffset = 0.2f;
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

    public TMP_Text readyText;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        anim.SetTrigger("roll");

        cameraObj = GameObject.FindObjectOfType<Camera>().transform;

        gravity = -2 * jumpHeightApex / (jumpDuration * jumpDuration);
        initialJumpVelocity = Mathf.Abs(gravity) * jumpDuration;

        DontDestroyOnLoad(this);

        UpdateReadyText();
    }

    void Update()
    {
        // Movement
        if (!allowMovement)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        // Vector3 viewDirection = transform.position - new Vector3(cameraObj.position.x, transform.position.y, cameraObj.position.z);
        // orientation.forward = viewDirection.normalized;
        orientation.forward = Vector3.zero;

        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
         
        if(moveDirection != Vector3.zero && allowMovement)
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
        RaycastHit hit1;
        if (Physics.Raycast(transform.position + new Vector3(0, 0.5f, 0), Vector3.down, out hit1, groundDistance + 0.1f))
        {
            isGrounded = true;
            rb.drag = groundDrag;

            if (!hasLanded)
            {
                hasLanded = true;
                anim.SetBool("isJumping", false);
            }
        }
        else
        {
            isGrounded = false;
            rb.drag = 0;
        }
        Debug.DrawRay(transform.position + new Vector3(0, 1, 0), Vector3.down * (groundDistance + 0.1f), Color.red);

        RaycastHit hit2;
        if (Physics.Raycast(transform.position + new Vector3(0, 0.5f, 0), Vector3.down, out hit2, groundDistanceLandingOffset + 0.1f))
        {
            if (!isGrounded && !isJumping)
            {
                anim.SetBool("isJumping", false);
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
            if (!allowFall) return;
            rb.AddForce(Vector3.down * -gravity * downwardsGravityMultiplier, ForceMode.Acceleration);
            print("Gravity: " + gravity);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!allowMovement) return;
        Vector2 input = context.ReadValue<Vector2>();
        horizontalInput = input.x;
        verticalInput = input.y;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (GameManager.instance.gameStarted == false && GameManager.instance.loadingLevel == false) {
                ToggleReady();
                return;
            }

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

    public void OnLeave(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (GameManager.instance.gameStarted) return;
            GameManager.instance.RemovePlayer(GetComponent<PlayerInput>());
        }
    }

    void MovePlayer()
    {
        if (!allowMovement) return;
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

    public async void Teleport(Vector3 position)
    {
        transform.position = position;
        await Task.Delay(100);

        // Workaround for teleporting not working sometimes
        for (int i = 0; i < 50; i++)
        {
            if (transform.position.x > position.x + 0.1f || transform.position.x < position.x - 0.1f || transform.position.z > position.z + 0.1f || transform.position.z < position.z - 0.1f)
            {
                print("Position messed up, Trying to teleport back...");
                transform.position = position;
                rb.velocity = Vector3.zero;
                anim.SetBool("isJumping", false);
                anim.SetBool("isMoving", false);
            }
            await Task.Delay(10);
        }
    }

    public void GetEliminated()
    {
        allowMovement = false;
        isEliminated = true;
        rb.velocity = Vector3.zero;
    }

    public void ToggleReady()
    {
        if (GameManager.instance.loadingLevel) return;
        isReady = !isReady;
        anim.SetTrigger("roll");
        UpdateReadyText();
        GameManager.instance.PlayerPressedReady();
    }

    public void UpdateReadyText()
    {
        if (isReady)
        {
            readyText.text = "Ready";
            readyText.color = new Color32(0, 190, 0, 255);
        }
        else
        {
            readyText.text = "Not Ready";
            readyText.color = new Color32(190, 0, 0, 255);
        }
    }

    public void ChangePlayerColor(Color32 color)
    {
        foreach (Renderer child in GetComponentsInChildren<Renderer>(includeInactive: true))
        {
            if (child.GetComponent<Renderer>() != null)
            {
                Material playerMaterial = new Material(child.material);
                child.GetComponent<Renderer>().material = playerMaterial;
                playerMaterial.SetColor("_Color01", color);
                playerMaterial.SetColor("_Color02", color);
                playerMaterial.SetColor("_Color03", color);
            }
        }
    }
}
