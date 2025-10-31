using UnityEngine;
using CharacterInfo = Player.Structs.CharacterInfo;
using CollisionInfo = Player.Structs.CollisionInfo;

namespace Player
{
    public abstract class KinematicBehaviour : MonoBehaviour, IKinematicBehaviour
    {
        public CollisionInfo CollisionInfo { get; private set; } = new();
        public CharacterInfo CharacterInfo { get; private set;  } = new();

        [HideInInspector] public Vector3 velocity;
        [HideInInspector] public Quaternion rotation;

        public CharacterController character;
        
        /// <summary>
        /// Set the inputs for the character controller.
        /// </summary>
        public abstract void OnUpdateUserInput();

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
        /// Get the velocity after applying friction.
        /// </summary>
        /// <param name="currentVelocity"></param>
        /// <param name="friction"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public Vector3 GetVelocityAfterFriction(Vector3 currentVelocity, float friction, float deltaTime)
        {
            currentVelocity -= currentVelocity * (friction * deltaTime);

            return currentVelocity;
        }
        
        /// <summary>
        /// Handle physics update for the character controller.
        /// </summary>
        public void HandleUpdatePhysics()
        {
            Vector3 origin = transform.position + CharacterInfo.Up * (character.radius + character.skinWidth);
            bool collided = Physics.SphereCast(
                origin:      origin, 
                radius:      character.radius, 
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
            character.transform.rotation = rotation;
        }

        /// <summary>
        /// Handle updating the character info.
        /// </summary>
        public void HandleUpdateCharacterInfo()
        {
            CharacterInfo info = CharacterInfo;
            
            info.Rotation = rotation;
            info.Velocity = velocity;
            
            CharacterInfo = info;
        }
    }
}