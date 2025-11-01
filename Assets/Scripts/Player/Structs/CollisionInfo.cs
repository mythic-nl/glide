using UnityEngine;

namespace Player.Structs
{
    public struct CollisionInfo
    {
        public float Angle;
        public bool IsStable;
        public bool Grounded;
        public Vector3 Point;
        public Vector3 Normal;
    }
}