using UnityEngine;

namespace Player.Structs
{
    public struct CharacterInfo
    {
        public Quaternion Rotation;
        public Vector3 Velocity;

        public Vector3 Up => Rotation * Vector3.up;
    }
}