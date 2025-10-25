using UnityEngine;

namespace Entities
{
    public class CameraFOV : MonoBehaviour
    {
        [SerializeField] private float maxSlidingFOV = 100f;
        [SerializeField] private float strength = 0.5f;
        [SerializeField] private float response = 15f;
        private float _defaultFOV;
        
        
        
        private Camera _camera;
        
        public void Initialize()
        {
            _camera = Camera.main;
            _defaultFOV = _camera!.fieldOfView;
        }

        public void UpdateFOV(float deltaTime, float acceleration, bool sliding)
        {
            float targetFOV = _defaultFOV;
            if (sliding)
            {
                targetFOV += acceleration * strength; 
                targetFOV = Mathf.Clamp(targetFOV, _defaultFOV, maxSlidingFOV);
            }
            
            _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, targetFOV, 1f - Mathf.Exp(-response * deltaTime));
        }
    }
}