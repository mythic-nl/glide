using UnityEngine;

public class FirstPersonMovement : MonoBehaviour
{
    [Header("Temporal Variables")]
    [SerializeField] private float gravity;
    [SerializeField] private float maxVerticalSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float groundedAcceleration;
    [SerializeField] private float maxGroundedAcceleration;
    [SerializeField] private float airborneAcceleration;
    [SerializeField] private float maxAirborneAcceleration;
    [SerializeField] private float friction;
    
    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform mainCamera;

    private Vector3 _velocity;

    private void Awake()
    {
        if (characterController == null) {
            Debug.LogError("There is no CharacterController set to the FirstPersonMovement script.");
        }

        if (mainCamera == null) {
            Debug.LogError("There is no MainCamera set to the FirstPersonMovement script.");
        }
    }
    
    private void Update()
    {
        RotateWithCamera();

        Movement();
    }

    private void Movement()
    {
        Vector3 wishDirection = CalculateWishDirection();
        Debug.Log($"Wish direction: {wishDirection}");
        
        _velocity = characterController.isGrounded 
            ? CalculateGroundedMovement(wishDirection, _velocity) 
            : CalculateAirborneMovement(wishDirection, _velocity);

        characterController.Move(_velocity * Time.deltaTime);
    }

    private Vector3 Accelerate(Vector3 wishDirection, Vector3 currentVelocity, float acceleration, float maxSpeed)
    {
        float wishSpeed = Vector3.Dot(wishDirection, currentVelocity);
        float accelerationSpeed = acceleration * Time.deltaTime;

        if (wishSpeed + accelerationSpeed > maxSpeed) {
            accelerationSpeed = maxSpeed - wishSpeed;
        }

        return currentVelocity + wishDirection * accelerationSpeed;
    }

    private Vector3 CalculateGroundedMovement(Vector3 wishDirection, Vector3 currentVelocity)
    {
        float speed = currentVelocity.magnitude;
        if (speed != 0.0f) {
            float drop = speed * friction * Time.deltaTime;
            currentVelocity *= Mathf.Max(speed - drop, 0.0f) / speed;
        }

        return Accelerate(wishDirection, currentVelocity, groundedAcceleration, maxGroundedAcceleration);
    }

    private Vector3 CalculateAirborneMovement(Vector3 wishDirection, Vector3 currentVelocity)
    {
        return Accelerate(wishDirection, currentVelocity, airborneAcceleration, maxAirborneAcceleration);
    }

    private Vector3 CalculateWishDirection()
    {
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        Vector3 horizontalVelocity = CalculateCameraDependantDirection(input);
        
        return new Vector3(horizontalVelocity.x, CalculateVerticalSpeed(), horizontalVelocity.z);
    }

    private Vector3 CalculateCameraDependantDirection(Vector3 input)
    {
        Vector3 cameraForward = mainCamera.forward;
        Vector3 cameraRight = mainCamera.right;
        
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        
        cameraForward.Normalize();
        cameraRight.Normalize();
        
        input = cameraForward * input.z + cameraRight * input.x;
        return input.normalized;
    }
    
    private float CalculateVerticalSpeed()
    {
        if (characterController.isGrounded) {
            if (Input.GetKey(KeyCode.Space)) {
                Debug.Log("Returning jump force");
                return jumpForce;
            }
            
            if (_velocity.y < 0f) {
                return -1.0f;
            }
        }

        float verticalSpeed = _velocity.y - gravity * Time.deltaTime;
        return Mathf.Clamp(verticalSpeed, -maxVerticalSpeed, maxVerticalSpeed);
    }

    private void RotateWithCamera()
    {
        transform.rotation = Quaternion.Euler(0f, mainCamera.transform.rotation.eulerAngles.y, 0f);
    }
}