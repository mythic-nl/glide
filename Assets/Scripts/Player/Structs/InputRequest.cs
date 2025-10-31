using UnityEngine;

namespace Player.Structs
{
    public struct InputRequest
    {
        public Vector3 MovementDirection;
        public Vector2 LookDirection;
        public bool IsJumping;
        public bool IsSprinting;
    }
}