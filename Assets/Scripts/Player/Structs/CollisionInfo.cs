using UnityEngine;

namespace Player.Structs
{
    public struct CollisionInfo
    {
        public bool Grounded;
        public Vector3 Point;
        public Vector3 Normal;
        public float Angle;
    }
}