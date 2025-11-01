using ScriptableObjects;
using UnityEngine;

namespace Player.Camera.Juice
{
    public class CameraLeanJuice : MonoBehaviour
    {
        [SerializeField] private float attackDamping = 0.5f;
        [SerializeField] private float decayDamping = 0.3f;
        [SerializeField] private float strength = 0.075f;
        [SerializeField] private Vector3Variable velocity;
        
        private Vector3 _dampedAcceleration;
        private Vector3 _dampedAccelerationVelocity;
        
        private void LateUpdate()
        {
            float deltaTime = Time.deltaTime;
            
            var planarAcceleration = Vector3.ProjectOnPlane(velocity.Value, Vector3.up);
            var damping = planarAcceleration.magnitude > _dampedAcceleration.magnitude
                ? attackDamping
                : decayDamping;
            
            _dampedAcceleration = Vector3.SmoothDamp(
                _dampedAcceleration,
                planarAcceleration,
                ref _dampedAccelerationVelocity,
                damping,
                float.PositiveInfinity,
                deltaTime
            );

            var leanAxis = Vector3.Cross(_dampedAcceleration.normalized, Vector3.up).normalized;
            transform.localRotation = Quaternion.identity;
            transform.rotation = Quaternion.AngleAxis(-_dampedAcceleration.magnitude * strength, leanAxis) * transform.rotation;
        }
    }
}