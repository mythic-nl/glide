using Player.Structs;
using UnityEngine;
using CharacterInfo = Player.Structs.CharacterInfo;
using CollisionInfo = Player.Structs.CollisionInfo;

namespace Player
{
    public interface IKinematicBehaviour
    {
        /// <summary>
        /// Get the collision information for the character controller.
        /// </summary>
        public CollisionInfo CollisionInfo { get; }
        
        /// <summary>
        /// Get the character information for the character controller.
        /// </summary>
        public CharacterInfo CharacterInfo { get; }

        /// <summary>
        /// Lifecycle method called to update user input.
        /// </summary>
        public void OnUpdateUserInput();

        /// <summary>
        /// Get the interpolation time based on a response value and delta time.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public float GetInterpolationTime(float response, float deltaTime);

        /// <summary>
        /// Get the direction vector tangent to a given surface normal. Allowing for better movement along slopes.
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="surfaceNormal"></param>
        /// <returns></returns>
        public Vector3 GetDirectionTangentToSurface(Vector3 direction, Vector3 surfaceNormal);

        /// <summary>
        /// Handle physics update for the character controller.
        /// </summary>
        public void HandleUpdatePhysics();

        /// <summary>
        /// Handle the transform update for the character controller.
        /// </summary>
        public void HandleUpdateTransform();

        /// <summary>
        /// Handle updating the character info.
        /// </summary>
        public void HandleUpdateCharacterInfo();
    }
}