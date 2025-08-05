using UnityEngine;

public class ShipMovement : MonoBehaviour
{
    // Movement
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float acceleration = 5f;
    [SerializeField] private float deceleration = 3f;
    [SerializeField] private float boostMultiplier = 1.5f; // Speed multiplier when boosting
    [SerializeField] private bool isBoosting = false;
    public bool IsBoosting => isBoosting;

    // Rotation
    [SerializeField] private float rotationSpeed = 200f;
    
    private float currentSpeed;
    private Rigidbody2D rb;
    private bool isMovingForward = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // Ensure gravity doesn't affect the ship
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovementInput();
        HandleRotation();
        Movement();
    }

    void HandleMovementInput()
    {
        // Check for boost input
        isBoosting = Input.GetKey(KeyCode.LeftShift);
        
        // Check for forward movement input
        if (Input.GetKey(KeyCode.W))
        {
            isMovingForward = true;
            float targetSpeed = isBoosting ? maxSpeed * boostMultiplier : maxSpeed;
            currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, targetSpeed);
        }
        // Check for backward movement input
        else if (Input.GetKey(KeyCode.S))
        {
            // Decelerate only when ship is moving
            if (currentSpeed > 0)
            {
                currentSpeed = Mathf.Max(currentSpeed - deceleration * Time.deltaTime, 0f);
                isMovingForward = currentSpeed > 0;
            }
            
        }
        // Decelerate when no movement keys are pressed
        else if (currentSpeed > 0)
        {
            currentSpeed = Mathf.Max(currentSpeed - deceleration * Time.deltaTime, 0f);
            if (currentSpeed <= 0.01f)
            {
                currentSpeed = 0f;
                isMovingForward = false;
            }
        }
        else
        {
            currentSpeed = 0f;
            isMovingForward = false;
        }
    }

    void Movement()
    {
        if (currentSpeed > 0f && isMovingForward)
        {
            Vector2 movement = transform.up * currentSpeed;
            rb.linearVelocity = movement;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    void HandleRotation()
    {
        float rotationInput = 0f;

        if (Input.GetKey(KeyCode.A))
        {
            rotationInput = 1f;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            rotationInput = -1f;
        }

        if (rotationInput != 0f)
        {
            float rotation = rotationInput * rotationSpeed * Time.deltaTime;
            rb.MoveRotation(rb.rotation + rotation);
        }
    }
}
