using ScriptableObjects;
using UnityEngine;
using Utils;

namespace Player.Camera.Juice
{
    public class CameraFovJuice : MonoBehaviour
    {
        [Tooltip("Minimum and maximum FOV value")]
        public FloatVariable minFov;
        public FloatVariable maxFov;
        
        [Tooltip("How much the FOV changes based on the target velocity")]
        public FloatVariable strength;
        
        [Tooltip("How quickly the FOV changes to the target value")]
        public FloatVariable response;
        
        [Tooltip("The target velocity to base the FOV changes on")]
        public Vector3Variable targetVelocity;
        
        [Tooltip("At what speed the effect should start to trigger")]
        public FloatVariable triggerSpeed;
        
        private UnityEngine.Camera _camera;

        private void Start()
        {
            _camera = UnityEngine.Camera.main;
        }

        private void LateUpdate()
        {
            float deltaTime = Time.deltaTime;
            float targetFOV = minFov.Value;
            
            if (Vector3.ProjectOnPlane(targetVelocity.Value, Vector3.up).magnitude > triggerSpeed.Value) {
                targetFOV += (Vector3.ProjectOnPlane(targetVelocity.Value, Vector3.up).magnitude - triggerSpeed.Value) * strength.Value; 
                targetFOV = Mathf.Clamp(targetFOV, minFov.Value, maxFov.Value);
            }

            Debug.Log(targetFOV);
            
            
            _camera.fieldOfView = Mathf.Lerp(
                a: _camera.fieldOfView,
                b: targetFOV,
                t: Common.GetInterpolationTime(response.Value, deltaTime)
            );
        }
    }
}