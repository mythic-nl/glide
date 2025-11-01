using Player.Structs;
using ScriptableObjects;
using Stateforge.Runtime.Interfaces;
using UnityEngine;
using PlayerInput = Input.PlayerInput;

namespace Player
{
    public class PlayerContext : KinematicBehaviour, IContext
    {
        [Header("References")] 
        [SerializeField] private PlayerInput input;
        [SerializeField] private Transform cameraTransform;

        [Header("Gravity")] 
        public FloatVariable gravity;
        public FloatVariable gravityResponse;

        [Header("Friction")] 
        public FloatVariable baseFriction;
        public FloatVariable slideFriction;

        [Header("Speed")] 
        public FloatVariable walkSpeed;
        public FloatVariable sprintSpeed;
        public FloatVariable airborneSpeed;
        public FloatVariable slideEndSpeed;
        public FloatVariable rotationSpeed;

        [Header("Acceleration")] 
        public FloatVariable walkAccelerationResponse;
        public FloatVariable sprintAccelerationResponse;
        public FloatVariable airborneAcceleration;

        [Header("Forces")] 
        public FloatVariable upwardForce;
        public FloatVariable downwardForce;

        [HideInInspector] 
        public InputRequest InputRequest;

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
            Gizmos.DrawRay(origin, velocity.Value);
            Gizmos.DrawSphere(origin + velocity.Value, 0.05f);

            Vector3 force = Vector3.ProjectOnPlane(-CharacterInfo.Up, CollisionInfo.Normal) * downwardForce.Value;
            Gizmos.color = Color.red;
            Gizmos.DrawRay(origin, force);
            Gizmos.DrawSphere(origin + force, 0.05f);

#if UNITY_EDITOR
            UnityEditor.Handles.color = Color.white;
            UnityEditor.Handles.Label(origin + velocity.Value, $"Speed: {velocity.Value.magnitude:F2}");
            UnityEditor.Handles.Label(origin + force, $"Force: {force.magnitude:F2}");
#endif
        }
    }
}