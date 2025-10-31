using Player.Structs;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public enum Statess
    {
        Idle,
        Walking,
        Sprinting,
        Sliding
    };
    
    public class PlayerBehaviour : KinematicController
    {
        [Header("Gravity Settings")]
        [SerializeField] private float gravity = 30f;
        [SerializeField] private float gravityResponse = 2.3f;
        [Space]
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 8f;
        [SerializeField] private float walkAccelerationResponse = 8f;
        [SerializeField] private float sprintSpeed = 14f;
        [SerializeField] private float sprintAccelerationResponse = 10f;
        [SerializeField] private float movementFriction = 1.5f;
        [Space]
        [SerializeField] private float jumpForce = 30f;
        [SerializeField] private float airborneSpeed = 6f;
        [SerializeField] private float airborneAcceleration = 30f;
        [Space] 
        [SerializeField] private float slideFriction = 0.6f;
        
        private InputRequest _inputRequest;
        [SerializeField] private Statess _state;
        
        public override void OnInit() { }

        public override void OnUpdateUserInput()
        {
            Vector3 movementDirection = Vector3.zero;

            if (Keyboard.current.wKey.isPressed) movementDirection.z += 1f;
            if (Keyboard.current.sKey.isPressed) movementDirection.z -= 1f;
            if (Keyboard.current.aKey.isPressed) movementDirection.x -= 1f;
            if (Keyboard.current.dKey.isPressed) movementDirection.x += 1f;
            
            _inputRequest.MovementDirection = movementDirection.normalized;
            _inputRequest.IsJumping = Keyboard.current.spaceKey.isPressed;
            _inputRequest.IsSprinting = Keyboard.current.leftShiftKey.isPressed;
        }

        public override void OnUpdateVelocity(ref Vector3 velocity, float deltaTime)
        {
            if (CollisionInfo.Grounded) {
                if (GetGroundNormalAngle() >= 45f) {
                    _state = Statess.Sliding;
                }
                else {
                    if (_inputRequest.MovementDirection != Vector3.zero) {
                        _state = _inputRequest.IsSprinting ? Statess.Sprinting : Statess.Walking;
                    } else {
                        _state = Statess.Idle;
                    }
                }
            }
            
            HandleUpdateGravity(ref velocity, deltaTime);
            HandleUpdateIdle(ref velocity, deltaTime);
            HandleUpdateMovement(ref velocity, deltaTime);
            HandleUpdateSliding(ref velocity, deltaTime);
            HandleUpdateAirborne(ref velocity, deltaTime);
        }

        public override void OnUpdateRotation(ref Quaternion rotation, float deltaTime)
        {
            rotation = transform.rotation;
        }

        private void HandleUpdateGravity(ref Vector3 velocity, float deltaTime)
        {
            Vector3 planarVelocity = Vector3.ProjectOnPlane(velocity, CharacterInfo.Up);
            float verticalSpeed = Vector3.Dot(velocity, CharacterInfo.Up);

            if (CollisionInfo.Grounded == false) {
                float targetVerticalSpeed = -gravity;
                verticalSpeed = Mathf.Lerp(
                    verticalSpeed,
                    targetVerticalSpeed,
                    GetInterpolationTime(gravityResponse, deltaTime)
                );
            }

            velocity = planarVelocity + (CharacterInfo.Up * verticalSpeed);
        }

        private void HandleUpdateIdle(ref Vector3 velocity, float deltaTime)
        {
            if (_state != Statess.Idle) return;
            if (CollisionInfo.Grounded == false) return;
            if (velocity.magnitude < 0.1f) return;

            velocity = GetVelocityAfterFriction(velocity, movementFriction, deltaTime);
        }
        
        private void HandleUpdateMovement(ref Vector3 velocity, float deltaTime)
        {
            if (_state is not (Statess.Walking or Statess.Sprinting)) return;
            if (CollisionInfo.Grounded == false) return;

            // velocity = GetVelocityAfterFriction(velocity, movementFriction, deltaTime);

            Vector3 planarDirection = GetDirectionTangentToSurface(_inputRequest.MovementDirection, CollisionInfo.Normal);
            Vector3 groundedMovement = planarDirection * _inputRequest.MovementDirection.magnitude;

            float targetSpeed = _inputRequest.IsSprinting ? sprintSpeed : walkSpeed;
            float targetResponse = _inputRequest.IsSprinting ? sprintAccelerationResponse : walkAccelerationResponse;
            
            Vector3 targetVelocity = groundedMovement * targetSpeed;
            velocity = Vector3.Lerp(
                a: velocity,
                b: targetVelocity,
                t: GetInterpolationTime(targetResponse, deltaTime)
            );
        }
        
        private void HandleUpdateSliding(ref Vector3 velocity, float deltaTime)
        {
            if (_state != Statess.Sliding) return;
            if (CollisionInfo.Grounded == false) return;

            velocity = GetVelocityAfterFriction(velocity, slideFriction, deltaTime);
            
            float angle = GetGroundNormalAngle();
            float minSlopeAngleForSliding = 45f;
            
            if (angle <= minSlopeAngleForSliding) return;
            
            Vector3 force = Vector3.ProjectOnPlane(-CharacterInfo.Up, CollisionInfo.Normal) * gravity;
            velocity += force * deltaTime;
        }

        private void HandleUpdateAirborne(ref Vector3 velocity, float deltaTime)
        {
            if (CollisionInfo.Grounded == false) {
                Vector3 planarDirection = Vector3.ProjectOnPlane(_inputRequest.MovementDirection, CharacterInfo.Up);
                Vector3 airborneMovement = planarDirection * _inputRequest.MovementDirection.magnitude;
                
                Vector3 currentPlanarVelocity = Vector3.ProjectOnPlane(velocity, CharacterInfo.Up);
                Vector3 movementForce = airborneMovement * airborneAcceleration * deltaTime;

                if (currentPlanarVelocity.magnitude < airborneSpeed) {
                    Vector3 targetPlanarVelocity = currentPlanarVelocity + movementForce;
                    targetPlanarVelocity = Vector3.ClampMagnitude(targetPlanarVelocity, airborneSpeed);
                    movementForce = targetPlanarVelocity - currentPlanarVelocity;
                } else if (Vector3.Dot(currentPlanarVelocity, movementForce) > 0f) {
                    movementForce = Vector3.ProjectOnPlane(movementForce, currentPlanarVelocity.normalized);
                }
                
                velocity += movementForce;
            }
            
            if (CollisionInfo.Grounded && _inputRequest.IsJumping) {
                float currentVerticalSpeed = Vector3.Dot(velocity, CharacterInfo.Up);
                float targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpForce);
                
                velocity += CharacterInfo.Up * (targetVerticalSpeed - currentVerticalSpeed);
            }
        }
        
        private Vector3 GetVelocityAfterFriction(Vector3 velocity, float friction, float deltaTime)
        {
            velocity -= velocity * (friction * deltaTime);

            return velocity;
        }
    }
}