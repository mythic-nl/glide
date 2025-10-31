using Player.Structs;
using Stateforge.Runtime.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerContext : KinematicBehaviour, IContext
    {
        [Header("Gravity")]
        public float gravity = 45f;
        public float gravityResponse = 6f;

        [Header("Friction")] 
        public float baseFriction = 3f;

        [Header("Speed")] 
        public float walkSpeed = 12f;
        public float sprintSpeed = 16f;
        public float airborneSpeed = 10f;

        [Header("Acceleration")] 
        public float walkAccelerationResponse = 10f;
        public float sprintAccelerationResponse = 13f;
        public float airborneAcceleration = 30f;

        [Header("Forces")] 
        public float upwardForce = 40f;
        public float downwardForce = 60f;
        
        [HideInInspector] public InputRequest InputRequest;
        
        public override void OnUpdateUserInput()
        {
            Vector3 movementDirection = Vector3.zero;
            
            if (Keyboard.current.wKey.isPressed) movementDirection.z += 1f;
            if (Keyboard.current.sKey.isPressed) movementDirection.z -= 1f;
            if (Keyboard.current.aKey.isPressed) movementDirection.x -= 1f;
            if (Keyboard.current.dKey.isPressed) movementDirection.x += 1f;
            
            InputRequest.MovementDirection = movementDirection.normalized;
            InputRequest.IsSprinting       = Keyboard.current.leftShiftKey.isPressed;
            InputRequest.IsJumping         = Keyboard.current.spaceKey.isPressed;
        }
    }
}