using UnityEngine;

namespace Entities
{
    public class CameraLean : MonoBehaviour
    {
        [SerializeField] private float attackDamping = 0.5f;
        [SerializeField] private float decayDamping = 0.3f;
        [SerializeField] private float strength = 0.075f;
        
        private Vector3 _dampedAcceleration;
        private Vector3 _dampedAccelerationVelocity;
        
        public void Initialize()
        {
            
        }

        public void UpdateLean(float deltaTime, Vector3 accelaration, Vector3 up)
        {
            var planarAcceleration = Vector3.ProjectOnPlane(accelaration, up);
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

            var leanAxis = Vector3.Cross(_dampedAcceleration.normalized, up).normalized;
            transform.localRotation = Quaternion.identity;
            transform.rotation = Quaternion.AngleAxis(-_dampedAcceleration.magnitude * strength, leanAxis) * transform.rotation;
        }
    }
}