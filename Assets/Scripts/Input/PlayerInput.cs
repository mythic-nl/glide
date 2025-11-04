using System;
using UnityEngine;

namespace Input
{
    public class PlayerInput : MonoBehaviour
    {
        private InputSystemActions _input;
        
        /// <summary>
        /// Enables the input system when the object is enabled.
        /// </summary>
        private void OnEnable()
        {
            if (_input == null) {
                _input = new InputSystemActions();
                _input.Enable();
            }
        }
        
        /// <summary>
        /// Disables the input system when the object is disabled.
        /// </summary>
        private void OnDisable()
        {
            _input.Dispose();
        }
        
        public Vector2 Move => _input.Player.Move.ReadValue<Vector2>();
        public Vector2 Look => _input.Player.Look.ReadValue<Vector2>();
        
        public bool Jump     => _input.Player.Jump.IsPressed();
        public bool Sprint   => _input.Player.Sprint.IsPressed();
        public bool Attack   => _input.Player.Attack.IsPressed();
        public bool Crouch   => _input.Player.Crouch.IsPressed();
        public bool Interact => _input.Player.Interact.IsPressed();
    }
}