using UnityEngine;
using CharacterInfo = Player.Structs.CharacterInfo;
using CollisionInfo = Player.Structs.CollisionInfo;

namespace Player
{
    [RequireComponent(typeof(CharacterController))]
    public abstract class KinematicController : MonoBehaviour, IKinematicBehaviour
    {
        public CollisionInfo CollisionInfo { get; private set; } = new();
        public CharacterInfo CharacterInfo { get; private set; } = new();
        
        private CharacterController _character;
        
        private Vector3 _velocity;
        private Quaternion _rotation;
        
        /// <summary>
        /// Start ordered initialization.
        /// </summary>
        private void Start()
        {
            _character = GetComponent<CharacterController>();
            if (_character == false) {
                _character = gameObject.AddComponent<CharacterController>();
            }
            
            OnInit();
        }
        
        /// <summary>
        /// Update loop for the kinematic controller. Ordered to ensure proper updates.
        /// </summary>
        private void Update()
        {
            float deltaTime = Time.deltaTime;
            
            HandleUpdateTransform();
            HandleUpdateCharacterInfo();
            HandleUpdatePhysics();
            
            OnUpdateUserInput();
            OnUpdateRotation(ref _rotation, deltaTime);
            OnUpdateVelocity(ref _velocity, deltaTime);
            
            _character.Move(_velocity * deltaTime);
        }

        /// <summary>
        /// Lifecycle method called on initialization.
        /// </summary>
        public abstract void OnInit();

        /// <summary>
        /// Lifecycle method called to update user input.
        /// </summary>
        public abstract void OnUpdateUserInput();

        /// <summary>
        /// Lifecycle method called to update velocity.
        /// </summary>
        /// <param name="velocity"></param>
        /// <param name="deltaTime"></param>
        public abstract void OnUpdateVelocity(ref Vector3 velocity, float deltaTime);

        /// <summary>
        /// Lifecycle method called to update rotation.
        /// </summary>
        /// <param name="rotation"></param>
        /// <param name="deltaTime"></param>
        public abstract void OnUpdateRotation(ref Quaternion rotation, float deltaTime);
        
        /// <summary>
        /// Get the interpolation time based on a response value and delta time.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public float GetInterpolationTime(float response, float deltaTime)
        {
            return 1f - Mathf.Exp(-response * deltaTime);
        }
        
        /// <summary>
        /// Get the direction vector tangent to a given surface normal. Allowing for better movement along slopes.
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="surfaceNormal"></param>
        /// <returns></returns>
        public Vector3 GetDirectionTangentToSurface(Vector3 direction, Vector3 surfaceNormal)
        {
            Vector3 right = Vector3.Cross(direction, surfaceNormal);
            Vector3 tangent = Vector3.Cross(surfaceNormal, right);
            
            return tangent.normalized;
        }

        /// <summary>
        /// Get the angle between the ground normal and the character's up direction.
        /// </summary>
        /// <returns></returns>
        public float GetGroundNormalAngle()
        {
            if (CollisionInfo.Grounded == false) return 0f;
            
            float angle = Vector3.Angle(CollisionInfo.Normal, CharacterInfo.Up);
            return angle;
        }

        /// <summary>
        /// Handle physics update for the character controller.
        /// </summary>
        public void HandleUpdatePhysics()
        {
            Vector3 origin = transform.position + CharacterInfo.Up * (_character.radius + _character.skinWidth);
            bool collided = Physics.SphereCast(
                origin:      origin, 
                radius:      _character.radius, 
                direction:   -CharacterInfo.Up, 
                hitInfo:     out RaycastHit hitInfo, 
                maxDistance: 0.2f
            );

            CollisionInfo info = CollisionInfo;
            
            info.Grounded = collided;
            info.Point    = hitInfo.point;
            info.Normal   = hitInfo.normal;
            
            CollisionInfo = info;
        }
        
        /// <summary>
        /// Handle the transform update for the character controller.
        /// </summary>
        public void HandleUpdateTransform()
        {
            _character.transform.rotation = _rotation;
        }

        /// <summary>
        /// Handle updating the character info.
        /// </summary>
        public void HandleUpdateCharacterInfo()
        {
            CharacterInfo info = CharacterInfo;
            
            info.Rotation = _rotation;
            info.Velocity = _velocity;
            
            CharacterInfo = info;
        }
        
        private void OnDrawGizmosSelected()
        {
            Ray velocityRay = new Ray(transform.position, CharacterInfo.Velocity);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(velocityRay);
            
            Debug.Log("Velocity: " + CharacterInfo.Velocity);
        }
    }
}