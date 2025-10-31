using Player.States._Base;
using UnityEngine;

namespace Player.States
{
    public class PlayerAirStrafeState : PlayerState<PlayerContext>
    {
        protected override void OnUpdateVelocity(ref Vector3 velocity, float deltaTime)
        {
            Vector3 planarDirection = Vector3.ProjectOnPlane(Context.InputRequest.MovementDirection, Context.CharacterInfo.Up);
            Vector3 airborneMovement = planarDirection * Context.InputRequest.MovementDirection.magnitude;
                
            Vector3 currentPlanarVelocity = Vector3.ProjectOnPlane(velocity, Context.CharacterInfo.Up);
            Vector3 movementForce = airborneMovement * Context.airborneAcceleration * deltaTime;

            if (currentPlanarVelocity.magnitude < Context.airborneSpeed) {
                Vector3 targetPlanarVelocity = currentPlanarVelocity + movementForce;
                targetPlanarVelocity = Vector3.ClampMagnitude(targetPlanarVelocity, Context.airborneSpeed);
                movementForce = targetPlanarVelocity - currentPlanarVelocity;
            } else if (Vector3.Dot(currentPlanarVelocity, movementForce) > 0f) {
                movementForce = Vector3.ProjectOnPlane(movementForce, currentPlanarVelocity.normalized);
            }
                
            velocity += movementForce;
        }
        
        protected override void SetTransitions()
        {
        }
    }
}