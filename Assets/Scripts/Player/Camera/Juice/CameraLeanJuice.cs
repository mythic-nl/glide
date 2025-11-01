using ScriptableObjects;
using UnityEngine;

namespace Player.Camera.Juice
{
    public class CameraLeanJuice : MonoBehaviour
    {
        [SerializeField] private FloatVariable attackDamping;
        [SerializeField] private FloatVariable decayDamping;
        [SerializeField] private FloatVariable strength;
        [SerializeField] private Vector3Variable velocity;
        
        private Vector3 _dampedAcceleration;
        private Vector3 _dampedAccelerationVelocity;
        
        private void LateUpdate()
        {
            float deltaTime = Time.deltaTime;
            
            Vector3 planarAcceleration = Vector3.ProjectOnPlane(velocity.Value, Vector3.up);
            float damping = planarAcceleration.magnitude > _dampedAcceleration.magnitude
                ? attackDamping.Value
                : decayDamping.Value;
            
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
            transform.rotation = Quaternion.AngleAxis(-_dampedAcceleration.magnitude * strength.Value, leanAxis) * transform.rotation;
        }
    }
}