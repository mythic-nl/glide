using Player.Structs;
using Stateforge.Runtime.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;
using PlayerInput = Input.PlayerInput;

namespace Player
{
    public class PlayerContext : KinematicBehaviour, IContext
    {
        [Header("References")] 
        [SerializeField] private PlayerInput input;
        [SerializeField] private Transform cameraTransform;
        
        [Header("Gravity")]
        public float gravity = 45f;
        public float gravityResponse = 6f;

        [Header("Friction")] 
        public float baseFriction = 3f;
        public float slideFriction = 0.9f;

        [Header("Speed")] 
        public float walkSpeed = 12f;
        public float sprintSpeed = 16f;
        public float airborneSpeed = 10f;
        public float slideEndSpeed = 9f;
        public float rotationSpeed = 50f;

        [Header("Acceleration")] 
        public float walkAccelerationResponse = 10f;
        public float sprintAccelerationResponse = 13f;
        public float airborneAcceleration = 30f;

        [Header("Forces")] 
        public float upwardForce = 40f;
        public float downwardForce = 60f;

        [HideInInspector] 
        public InputRequest InputRequest;
        public Transform CameraTransform => cameraTransform;

        public override void OnUpdateUserInput()
        {
            Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, CharacterInfo.Up).normalized;
            Vector3 right   = Vector3.ProjectOnPlane(cameraTransform.right, CharacterInfo.Up).normalized;
            Vector3 movementDirection = (forward * input.Move.y) + (right * input.Move.x);
            InputRequest.MovementDirection = Vector3.ClampMagnitude(movementDirection, 1f);;
            
            InputRequest.IsSprinting       = input.Sprint;
            InputRequest.IsJumping         = input.Jump;
        }
        
        public void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(
                character.transform.position, 
                character.transform.position + CollisionInfo.Normal);
            
            Vector3 origin = transform.position;

            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(origin, velocity);
            Gizmos.DrawSphere(origin + velocity, 0.05f);

            Vector3 force = Vector3.ProjectOnPlane(-CharacterInfo.Up, CollisionInfo.Normal) * downwardForce;
            Gizmos.color = Color.red;
            Gizmos.DrawRay(origin, force);
            Gizmos.DrawSphere(origin + force, 0.05f);

#if UNITY_EDITOR
            UnityEditor.Handles.color = Color.white;
            UnityEditor.Handles.Label(origin + velocity, $"Speed: {velocity.magnitude:F2}");
            UnityEditor.Handles.Label(origin + force, $"Force: {force.magnitude:F2}");
#endif
        }
    }
}