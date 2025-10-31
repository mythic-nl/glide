using System;
using KinematicCharacterController;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public struct CharacterMotorInfo
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Velocity;
        public CharacterGroundInfo GroundInfo;

        public Vector3 CharacterUp => Rotation * Vector3.up;
    }

    public struct CharacterInputRequest
    {
        public Vector3 MovementDirection;
        // public Vector2 LookDirection;
        public bool IsJumping;
        public bool IsCrouching;
        public bool IsSprinting;
    }
    
    public struct CharacterGroundInfo
    {
        public bool IsGrounded;
        public Vector3 GroundNormal;
        public Vector3 GroundPoint;
        public float GroundAngle;
    }

    public enum CharacterState
    {
        Standing,
        Sliding,
        Airborne
    }
    
    public class PlayerControllerTemp : MonoBehaviour
    {
        #region Variables
        [SerializeField] private CharacterController character;

        [Header("Jump/Airborne Settings")]
        [SerializeField] private float upwardForce = 40f;
        [SerializeField] private float downwardForce = 25f;
        [SerializeField] private float downwardForceResponse = 6f;

        [Header("Movement Settings")]
        [SerializeField] private float walkMovevementSpeed = 8f;
        [SerializeField] private float sprintMovementSpeed = 14f;
        [SerializeField] private float walkMovementResponse = 15f;
        [SerializeField] private float sprintMovementResponse = 10f;
        [Space]
        [SerializeField] private float airborneSpeed = 6f;
        [SerializeField] private float airborneAcceleration = 30f;
        [Space]
        [SerializeField] private float initialSlideSpeed = 25f;
        [SerializeField] private float slideEndSpeed = 15f;
        [SerializeField] private float slideFriction = 0.8f;
        [SerializeField] private float slideSteerAcceleration = 5.0f;

        [Header("Rotation Settings")]
        [SerializeField] private float rotationSpeed = 720f;

        [Header("Camera Settings")] 
        [SerializeField] private Transform cameraTransform;
        
        [Header("Model Settings")]
        [SerializeField] private Transform targetTransform;
        
        private CharacterMotorInfo _motorInfo;
        private CharacterInputRequest _inputRequest;
        private CharacterState _currentState;
        #endregion

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            
            if (character == false) {
                character = GetComponent<CharacterController>();
                if (character == false) {
                    character = gameObject.AddComponent<CharacterController>();
                }
            }

            _motorInfo = new CharacterMotorInfo();
            _inputRequest = new CharacterInputRequest();
        }
        
        private void Update()
        {
            float deltaTime = Time.deltaTime;
            
            RecalculateMotorInfo();
            
            HandleUserInput();
            
            HandleUpdateRotation(ref _motorInfo.Rotation, deltaTime);
            HandleUpdateVelocity(ref _motorInfo.Velocity, deltaTime);
            
            character.Move(_motorInfo.Velocity * Time.deltaTime);
        }

        /// <summary>
        /// Calculate all necessary variables for the character controller motor
        /// </summary>
        private void RecalculateMotorInfo()
        {
            Transform currentTransform = character.transform;
            
            currentTransform.rotation = _motorInfo.Rotation;
            
            _motorInfo.Position = currentTransform.position;
            _motorInfo.Rotation = currentTransform.rotation;
            
            bool isGrounded = IsGrounded(out RaycastHit collisionInfo);
            
            _motorInfo.GroundInfo.IsGrounded   = isGrounded;
            _motorInfo.GroundInfo.GroundNormal = isGrounded ? collisionInfo.normal : _motorInfo.CharacterUp;
            _motorInfo.GroundInfo.GroundPoint  = isGrounded ? collisionInfo.point : Vector3.zero;
            _motorInfo.GroundInfo.GroundAngle  = isGrounded ? Vector3.Angle(_motorInfo.CharacterUp, collisionInfo.normal) : 90f;
            
            Debug.Log("Ground info: " + 
                      "\n IsGrounded: " + _motorInfo.GroundInfo.IsGrounded + 
                      "\n GroundNormal: " + _motorInfo.GroundInfo.GroundNormal + 
                      "\n GroundPoint: " + _motorInfo.GroundInfo.GroundPoint + 
                      "\n GroundAngle: " + _motorInfo.GroundInfo.GroundAngle);
        }

        /// <summary>
        /// TODO: Handle user input by making use of the new Input System
        /// </summary>
        private void HandleUserInput()
        {
            Vector3 movementDirection = Vector3.zero;
            if (Keyboard.current.wKey.isPressed) movementDirection.z += 1f;
            if (Keyboard.current.sKey.isPressed) movementDirection.z -= 1f;
            if (Keyboard.current.aKey.isPressed) movementDirection.x -= 1f;
            if (Keyboard.current.dKey.isPressed) movementDirection.x += 1f;
            _inputRequest.MovementDirection = movementDirection.normalized;
            
            Vector3 cameraForward = Vector3.ProjectOnPlane(cameraTransform.forward, _motorInfo.CharacterUp).normalized;
            Vector3 cameraRight = Vector3.ProjectOnPlane(cameraTransform.right, _motorInfo.CharacterUp).normalized;
            _inputRequest.MovementDirection = (cameraForward * _inputRequest.MovementDirection.z + cameraRight * _inputRequest.MovementDirection.x);
            
            _inputRequest.IsJumping = Keyboard.current.spaceKey.isPressed;
            _inputRequest.IsCrouching = Keyboard.current.leftCtrlKey.isPressed;
            _inputRequest.IsSprinting = Keyboard.current.leftShiftKey.isPressed;
        }

        private void HandleGravityPass(ref Vector3 currentVelocity, float deltaTime)
        {
            Vector3 planarVelocity = Vector3.ProjectOnPlane(currentVelocity, _motorInfo.CharacterUp);
            float verticalSpeed = Vector3.Dot(currentVelocity, _motorInfo.CharacterUp);            
            
            if (_motorInfo.GroundInfo.IsGrounded == false) {
                float targetVerticalSpeed = -downwardForce;
                verticalSpeed = Mathf.Lerp(
                    a: verticalSpeed, 
                    b: targetVerticalSpeed, 
                    t: InterpolationTime(downwardForceResponse, deltaTime)
                );
            }
            
            currentVelocity = planarVelocity + (_motorInfo.CharacterUp * verticalSpeed);
        }

        private void HandleUpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            if (_inputRequest.MovementDirection.sqrMagnitude > 0.01f) {
                Vector3 planarVelocity = Vector3.ProjectOnPlane(_motorInfo.Velocity, _motorInfo.CharacterUp);
                Quaternion targetRotation = Quaternion.LookRotation(planarVelocity, _motorInfo.CharacterUp);
                currentRotation = Quaternion.RotateTowards(
                    from: currentRotation,
                    to: targetRotation,
                    maxDegreesDelta: rotationSpeed * deltaTime
                );
            }
        }
        
        private void HandleUpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            HandleGravityPass(ref currentVelocity, deltaTime);

            if (_inputRequest.IsCrouching && _motorInfo.GroundInfo.IsGrounded) {
                _currentState = CharacterState.Sliding;
            }
            
            switch (_currentState) {
                case CharacterState.Standing:
                    HandleStandingMovement(ref currentVelocity, deltaTime);
                    break; 
                case CharacterState.Sliding:
                    HandleSlidingMovement(ref currentVelocity, deltaTime);
                    break;
            }
            
            
            
            HandleJumpingMovement(ref currentVelocity, deltaTime);
        }

        private void HandleStandingMovement(ref Vector3 currentVelocity, float deltaTime)
        {
            if (_motorInfo.GroundInfo.IsGrounded == false) return;
            
            Vector3 adjustedDirection = GetDirectionTangentToSurface(_inputRequest.MovementDirection, _motorInfo.GroundInfo.GroundNormal);
            Vector3 groundedMovement = adjustedDirection * _inputRequest.MovementDirection.magnitude;
                
            float targetSpeed = _inputRequest.IsSprinting ? sprintMovementSpeed : walkMovevementSpeed;
            float targetResponse = _inputRequest.IsSprinting ? sprintMovementResponse : walkMovementResponse;
                
            Vector3 targetVelocity = groundedMovement * targetSpeed;
            currentVelocity = Vector3.Lerp(
                a: currentVelocity,
                b: targetVelocity,
                t: InterpolationTime(targetResponse, deltaTime)
            );
        }

        private void HandleSlidingMovement(ref Vector3 currentVelocity, float deltaTime)
        {
            if (_motorInfo.GroundInfo.IsGrounded == false) return;
            
            Vector3 adjustedDirection = GetDirectionTangentToSurface(_inputRequest.MovementDirection, _motorInfo.GroundInfo.GroundNormal);
            Vector3 groundedMovement = adjustedDirection * _inputRequest.MovementDirection.magnitude;

            if (groundedMovement.sqrMagnitude > 1e-6f) {
                if (_motorInfo.GroundInfo.IsGrounded == false) {
                    currentVelocity = Vector3.ProjectOnPlane(_motorInfo.Velocity, _motorInfo.GroundInfo.GroundNormal);
                }

                float effectiveSpeed = initialSlideSpeed;
                if (_motorInfo.GroundInfo.IsGrounded == false) {
                    effectiveSpeed = 0.0f;
                }
                
                var slideSpeed = Mathf.Max(effectiveSpeed, currentVelocity.magnitude);
                currentVelocity = GetDirectionTangentToSurface(currentVelocity, _motorInfo.GroundInfo.GroundNormal) * slideSpeed;
            }

            currentVelocity -= currentVelocity * (slideFriction * deltaTime);

            Vector3 slopeDownwardForce = Vector3.ProjectOnPlane(-_motorInfo.CharacterUp, _motorInfo.GroundInfo.GroundNormal) * downwardForce;
            currentVelocity -= slopeDownwardForce * deltaTime;
            
            float currentSpeed = currentVelocity.magnitude;
            Vector3 targetVelocity = groundedMovement * currentSpeed;
            Vector3 steerVelocity = currentVelocity;
            Vector3 steerForce = (targetVelocity - currentVelocity) * slideSteerAcceleration * deltaTime;

            steerVelocity += steerForce;
            steerVelocity = Vector3.ClampMagnitude(steerVelocity, currentSpeed);
            
            currentVelocity = steerVelocity;
            
            if (currentVelocity.magnitude < slideEndSpeed) {
                _currentState = CharacterState.Standing;
            }
        }

        private void HandleJumpingMovement(ref Vector3 currentVelocity, float deltaTime)
        {
            if (_motorInfo.GroundInfo.IsGrounded == false) {
                Vector3 planarDirection = Vector3.ProjectOnPlane(_inputRequest.MovementDirection, _motorInfo.CharacterUp);
                Vector3 airborneMovement = planarDirection * _inputRequest.MovementDirection.magnitude;
                
                Vector3 currentPlanarVelocity = Vector3.ProjectOnPlane(currentVelocity, _motorInfo.CharacterUp);
                Vector3 movementForce = airborneMovement * airborneAcceleration * deltaTime;

                if (currentPlanarVelocity.magnitude < airborneSpeed) {
                    Vector3 targetPlanarVelocity = currentPlanarVelocity + movementForce;
                    targetPlanarVelocity = Vector3.ClampMagnitude(targetPlanarVelocity, airborneSpeed);
                    movementForce = targetPlanarVelocity - currentPlanarVelocity;
                }
                else if (Vector3.Dot(currentPlanarVelocity, movementForce) > 0f) {
                    movementForce = Vector3.ProjectOnPlane(movementForce, currentPlanarVelocity.normalized);
                }
                
                currentVelocity += movementForce;
            }
            
            if (_motorInfo.GroundInfo.IsGrounded && _inputRequest.IsJumping) {
                float currentVerticalSpeed = Vector3.Dot(currentVelocity, _motorInfo.CharacterUp);
                float targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, upwardForce);
                
                currentVelocity += _motorInfo.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
            }
        }

        private float InterpolationTime(float response, float deltaTime)
        {
            return 1.0f - Mathf.Exp(-response * deltaTime); 
        }

        private bool IsGrounded(out RaycastHit collisionInfo)
        {
            bool hasCollided = Physics.SphereCast(
                origin: _motorInfo.Position + _motorInfo.CharacterUp * (character.radius + 0.1f),
                radius: character.radius,
                direction: -_motorInfo.CharacterUp,
                hitInfo: out RaycastHit hitInfo,
                maxDistance: 0.2f
            );   
            
            collisionInfo = hitInfo;

            return hasCollided;
        }

        private Vector3 GetDirectionTangentToSurface(Vector3 direction, Vector3 normal)
        {
            Vector3 directionRight = Vector3.Cross(direction, normal);
            return Vector3.Cross(normal, directionRight).normalized;
        }
        
        private void OnDrawGizmos()
        {
            if (IsGrounded(out RaycastHit collisionInfo)) {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(_motorInfo.Position, collisionInfo.point);
                Gizmos.DrawSphere(collisionInfo.point, 0.1f);
            } else {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(_motorInfo.Position, _motorInfo.Position - _motorInfo.CharacterUp * 0.2f);
            }
            Gizmos.DrawSphere(_motorInfo.Position + _motorInfo.CharacterUp * (character.radius + 0.1f), character.radius);
        }
       
    }
}
