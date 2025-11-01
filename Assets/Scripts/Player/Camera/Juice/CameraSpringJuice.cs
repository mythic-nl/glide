using ScriptableObjects;
using UnityEngine;

namespace Player.Camera.Juice
{
    public class CameraSpringJuice : MonoBehaviour
    {
        [SerializeField] private FloatVariable halfLife;
        [SerializeField] private FloatVariable frequency;
        [SerializeField] private FloatVariable angularDisplacement;
        [SerializeField] private FloatVariable linearDisplacement;
        
        private Vector3 _springPosition;
        private Vector3 _sprintVelocity;
        
        public void Start()
        {
            _springPosition = transform.position;
            _sprintVelocity = Vector3.zero;
        }

        public void LateUpdate()
        {
            float deltaTime = Time.deltaTime;
            
            transform.localPosition = Vector3.zero;
            Spring(ref _springPosition, ref _sprintVelocity, transform.position, halfLife.Value, frequency.Value, deltaTime);

            Vector3 localSpringPosition = _springPosition - transform.position;
            float springHeight = Vector3.Dot(localSpringPosition, Vector3.up);

            transform.localEulerAngles = new Vector3(-springHeight * angularDisplacement.Value, 0.0f, 0.0f);
            transform.localPosition = -localSpringPosition * linearDisplacement.Value;
        }
        
        // https://allenchou.net/2015/04/game-math-more-on-numeric-springing/
        private static void Spring(ref Vector3 current, ref Vector3 velocity, Vector3 target, float halfLife, float frequency, float timeStep)
        {
            float dampingRatio = -Mathf.Log(0.5f) / (frequency * halfLife);
            float f = 1.0f + 2.0f * timeStep * dampingRatio * frequency;
            float oo = frequency * frequency;
            float hoo = timeStep * oo;
            float hhoo = timeStep * hoo;
            float detInv = 1.0f / (f + hhoo);
            Vector3 detX = f * current + timeStep * velocity + hhoo * target;
            Vector3 detV = velocity + hoo * (target - current);
            
            current = detX * detInv;
            velocity = detV * detInv;
        }
    }
}