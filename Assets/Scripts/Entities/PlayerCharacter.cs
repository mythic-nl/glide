using KinematicCharacterController;
using NUnit.Framework.Internal.Commands;
using UnityEngine;

namespace Entities
{
    public enum CrouchInput
    {
        None, 
        Toggle, 
    }

    public enum Stance
    {
        Stand, 
        Crouch,
        Slide,
    }
    
    public struct CharacterInput
    {
        public Quaternion Rotation;
        public Vector2 MovementDirection;
        public bool Jump;
        public bool JumpSustain;
        public CrouchInput Crouch;
    }

    public struct CharacterState
    {
        public bool Grounded;
        public Stance Stance;
        public Vector3 Velocity;
        public Vector3 Acceleration;
    }
    
    [RequireComponent(typeof(CapsuleCollider), typeof(KinematicCharacterMotor))]
    public class PlayerCharacter : MonoBehaviour, ICharacterController
    {
        [SerializeField] private KinematicCharacterMotor motor;
        [SerializeField] private Transform root;
        [SerializeField] private Transform cameraTarget;
        
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 15f;
        [SerializeField] private float crouchSpeed = 7.5f;
        
        [SerializeField] private float jumpSpeed = 20f;
        [SerializeField] private float gravity = -90f;
        [SerializeField] private float jumpSustainGravityMultiplier = 0.4f;
        [SerializeField] private float coyoteTime = 0.2f;
        
        [SerializeField] private float airSpeed = 15f;
        [SerializeField] private float airAcceleration = 70f;
        
        [SerializeField] private float slideStartSpeed = 25f;
        [SerializeField] private float slideEndSpeed = 15f;
        [SerializeField] private float slideFriction = 0.8f;
        [SerializeField] private float slideSteerAcceleration = 5.0f;
        [SerializeField] private float slideGravity = -90f;
        
        [Header("Stance Settings")]
        [SerializeField] private float standHeight = 2.0f;
        [SerializeField] private float crouchHeight = 1.0f;
        [Range(0.0f, 1.0f)]
        [SerializeField] private float standCameraHeight = 0.9f;
        [Range(0.0f, 1.0f)]
        [SerializeField] private float crouchCameraHeight = 0.7f;

        [Header("Responsiveness Settings")] 
        [SerializeField] private float crouchHeightResponse = 15f;
        [SerializeField] private float walkResponse = 25f;
        [SerializeField] private float crouchResponse = 20f;

        private CharacterState _state;
        private CharacterState _lastState;
        private CharacterState _tempState;
        
        private Quaternion _requestedRotation;
        private Vector3 _requestedMovementDirection;
        private bool _requestedJump;
        private bool _requestedJumpSustain;
        private bool _requestedCrouch;
        private bool _requestedCrouchInAir;

        private float _timeSinceUngrounded;
        private float _timeSinceJumpRequest;
        private bool _ungroundedDueToJump;
        
        private Collider[] _uncrouchedColliders;
        
        public void Initialize()
        {
            _state.Stance = Stance.Stand;
            _lastState = _state;
            
            motor.CharacterController = this;
        }

        public Transform GetCameraTarget()
        {
            return cameraTarget;
        }

        public void UpdateInput(CharacterInput input)
        {
            _requestedRotation = input.Rotation;
            
            _requestedMovementDirection = new Vector3(input.MovementDirection.x, 0.0f, input.MovementDirection.y);
            _requestedMovementDirection = Vector3.ClampMagnitude(_requestedMovementDirection, 1.0f);
            _requestedMovementDirection = _requestedRotation * _requestedMovementDirection;

            var wasRequestingJump = _requestedJump;
            _requestedJump = _requestedJump || input.Jump;
            if (_requestedJump && wasRequestingJump is false) {
                _timeSinceJumpRequest = 0.0f;
            }
            
            _requestedJumpSustain = input.JumpSustain;
            
            var wasRequestingCrouch = _requestedCrouch;
            _requestedCrouch = input.Crouch switch
            {   
                CrouchInput.None => _requestedCrouch,
                CrouchInput.Toggle => !_requestedCrouch,
                _ => _requestedCrouch
            };

            if (_requestedCrouch && wasRequestingCrouch is false) {
                _requestedCrouchInAir = !_state.Grounded;
            } 
            else if (_requestedCrouch is false && wasRequestingCrouch) {
                _requestedCrouchInAir = false;
            }
        }
        
        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            var forward = Vector3.ProjectOnPlane(_requestedRotation * Vector3.forward, motor.CharacterUp);

            if (forward != Vector3.zero) {
                currentRotation = Quaternion.LookRotation(forward, motor.CharacterUp);
            }
        }

        public void UpdateBody(float deltaTime)
        {
            var currentHeight = motor.Capsule.height;
            var normalizedHeight = currentHeight / standHeight;
            
            var stanceHeight = _state.Stance is Stance.Stand 
                ? standCameraHeight 
                : crouchCameraHeight;

            var cameraTargetHeight = currentHeight * stanceHeight;
            var rootTargetScale = new Vector3(1.0f, normalizedHeight, 1.0f);

            cameraTarget.localPosition = Vector3.Lerp(
                cameraTarget.localPosition,
                Vector3.up * cameraTargetHeight,
                1.0f - Mathf.Exp(-crouchHeightResponse * deltaTime)
            );
            root.localScale = Vector3.Lerp(
                root.localScale,
                rootTargetScale,
                1.0f - Mathf.Exp(-crouchHeightResponse * deltaTime)
            );
        }

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            _state.Acceleration = Vector3.zero;
            
            if (motor.GroundingStatus.IsStableOnGround) {
                _timeSinceUngrounded = 0.0f;
                _ungroundedDueToJump = false;
                
                var groundedMovement = motor.GetDirectionTangentToSurface(_requestedMovementDirection, motor.GroundingStatus.GroundNormal);
                groundedMovement *= _requestedMovementDirection.magnitude;       
                
                // Slide movement
                {
                    if (groundedMovement.sqrMagnitude > 0.0f && _state.Stance is Stance.Crouch && (_lastState.Stance is Stance.Stand || _lastState.Grounded is false)) {
                        _state.Stance = Stance.Slide;

                        if (_lastState.Grounded is false) {
                            currentVelocity = Vector3.ProjectOnPlane(_lastState.Velocity, motor.GroundingStatus.GroundNormal);
                        }

                        var effectiveSlideStartSpeed = slideStartSpeed;
                        if (_lastState.Grounded is false && _requestedCrouchInAir is false) {
                            effectiveSlideStartSpeed = 0.0f;
                            _requestedCrouchInAir = false;
                        }
                        
                        var slideSpeed = Mathf.Max(effectiveSlideStartSpeed, currentVelocity.magnitude);
                        currentVelocity = motor.GetDirectionTangentToSurface(
                            currentVelocity,
                            motor.GroundingStatus.GroundNormal
                        ) * slideSpeed;
                    }
                }
                
                // Walk/Crouch movement
                if (_state.Stance is Stance.Stand or Stance.Crouch)
                {
                    var speed = _state.Stance == Stance.Stand
                        ? walkSpeed
                        : crouchSpeed;
                
                    var response = _state.Stance is Stance.Stand
                        ? walkResponse
                        : crouchResponse;

                    var targetVelocity = groundedMovement * speed;
                    var moveVelocity = Vector3.Lerp(
                        currentVelocity,
                        targetVelocity,
                        1.0f - Mathf.Exp(-response * deltaTime)
                    );

                    _state.Acceleration = (moveVelocity - currentVelocity) / deltaTime;
                    currentVelocity = moveVelocity;
                }
                else {
                    currentVelocity -= currentVelocity * (slideFriction * deltaTime);
                    
                    // Slope movmenet
                    {
                        var force = Vector3.ProjectOnPlane(
                            -motor.CharacterUp,
                            motor.GroundingStatus.GroundNormal
                        ) * slideGravity;
                        
                        currentVelocity -= force * deltaTime;
                    }
                    
                    // Steering
                    {
                        var currentSpeed = currentVelocity.magnitude;
                        var targetVelocity = groundedMovement * currentSpeed;
                        var steerVelocity = currentVelocity;
                        var steerForce = (targetVelocity - currentVelocity) * slideSteerAcceleration * deltaTime;
                        
                        steerVelocity += steerForce ;
                        steerVelocity = Vector3.ClampMagnitude(steerVelocity, currentSpeed);
                        
                        _state.Acceleration = (steerVelocity - currentVelocity) / deltaTime;
                        currentVelocity = steerVelocity;
                    }
                    
                    if (currentVelocity.magnitude < slideEndSpeed) {
                        _state.Stance = Stance.Crouch;
                    }
                }
            }
            else {
                _timeSinceUngrounded += deltaTime;
                
                if (_requestedMovementDirection.sqrMagnitude > 0.0f) {
                    var planarMovement = Vector3.ProjectOnPlane(
                        _requestedMovementDirection,
                        motor.CharacterUp
                    ).normalized * _requestedMovementDirection.magnitude;
                    
                    var currentPlanarVelocity = Vector3.ProjectOnPlane(
                        currentVelocity,
                        motor.CharacterUp
                    );

                    var movementForce = planarMovement * airAcceleration * deltaTime;

                    if (currentPlanarVelocity.magnitude < airSpeed) {
                        var targetPlanarVelocity = currentPlanarVelocity + movementForce;
                        targetPlanarVelocity = Vector3.ClampMagnitude(targetPlanarVelocity, airSpeed);
                        movementForce = targetPlanarVelocity - currentPlanarVelocity;
                        
                    }
                    else if (Vector3.Dot(currentPlanarVelocity, movementForce) > 0.0f) {
                        var constrainedMovementForce = Vector3.ProjectOnPlane(
                            movementForce,
                            currentPlanarVelocity.normalized
                        );
                        
                        movementForce = constrainedMovementForce;
                    }

                    if (motor.GroundingStatus.FoundAnyGround) {
                        if (Vector3.Dot(movementForce, currentVelocity + movementForce) > 0.0f) {
                            var obstructionNormal = Vector3.Cross(
                                motor.CharacterUp,
                                Vector3.Cross(motor.CharacterUp, motor.GroundingStatus.GroundNormal)
                            ).normalized;

                            movementForce = Vector3.ProjectOnPlane(movementForce, obstructionNormal);
                        }
                    }
                    
                    currentVelocity += movementForce;
                }
                
                var effectiveGravity = gravity;
                var verticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
                if (_requestedJumpSustain && verticalSpeed > 0.0f) {
                    effectiveGravity *= jumpSustainGravityMultiplier;
                }
                
                currentVelocity += motor.CharacterUp * effectiveGravity * deltaTime;
            }

            if (_requestedJump) {
                var grounded = motor.GroundingStatus.IsStableOnGround;
                var canCoyoteJump = _timeSinceUngrounded < coyoteTime && _ungroundedDueToJump is false;
                
                if (grounded || canCoyoteJump) {
                    _requestedJump = false;
                    _requestedCrouch = false;
                    _requestedCrouchInAir = false;
                
                    motor.ForceUnground(0.0f);
                    _ungroundedDueToJump = true;

                    var currentVerticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
                    var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpSpeed);
                    currentVelocity += motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
                }
                else {
                    _timeSinceJumpRequest += deltaTime;
                    
                    var canJumpLater = _timeSinceJumpRequest < coyoteTime;
                    _requestedJump = canJumpLater;
                }
            }
        } 

        public void BeforeCharacterUpdate(float deltaTime)
        {
            _tempState = _state;
            
            if (_requestedCrouch && _state.Stance is Stance.Stand) {
                _state.Stance = Stance.Crouch;
                
                motor.SetCapsuleDimensions(
                    radius: motor.Capsule.radius,
                    height: crouchHeight,
                    yOffset: crouchHeight * 0.5f
                );
            }
        }

        public void PostGroundingUpdate(float deltaTime)
        {
            if (motor.GroundingStatus.IsStableOnGround is false && _state.Stance is Stance.Slide) {
                _state.Stance = Stance.Crouch;
            }
        }

        public void AfterCharacterUpdate(float deltaTime)
        {
            if (_requestedCrouch is false && _state.Stance is Stance.Crouch) {
                motor.SetCapsuleDimensions(
                    radius: motor.Capsule.radius,
                    height: standHeight,
                    yOffset: standHeight * 0.5f
                );

                if (motor.CharacterOverlap(
                    motor.TransientPosition,
                    motor.TransientRotation,
                    _uncrouchedColliders,
                    motor.CollidableLayers,
                    QueryTriggerInteraction.Ignore
                ) > 0) {
                    _requestedCrouch = true;
                    
                    motor.SetCapsuleDimensions(
                        radius: motor.Capsule.radius,
                        height: crouchHeight,
                        yOffset: crouchHeight * 0.5f
                    );
                }
                else {
                    _state.Stance = Stance.Stand;
                }
            }

            _state.Grounded = motor.GroundingStatus.IsStableOnGround;
            _state.Velocity = motor.Velocity;
            _lastState = _tempState;
        }

        public bool IsColliderValidForCollisions(Collider coll)
        {
            return true;
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            
        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            
        }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
            
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
            
        }

        public void SetPosition(Vector3 position, bool killVelocity = true)
        {
            motor.SetPosition(position);
            if (killVelocity) {
                motor.BaseVelocity = Vector3.zero;
            }
        }

        public CharacterState GetState()
        {
            return _state;
        }

        public CharacterState GetLastState()
        {
            return _lastState;
        }
    }
}
