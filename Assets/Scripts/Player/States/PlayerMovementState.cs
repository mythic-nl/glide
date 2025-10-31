using Player.States._Base;
using UnityEngine;

namespace Player.States
{
    public class PlayerMovementState : PlayerState<PlayerContext>
    {
        protected override void OnUpdateVelocity(ref Vector3 velocity, float deltaTime)
        {
            Vector3 planarVelocity = Context.GetDirectionTangentToSurface(Context.InputRequest.MovementDirection, Context.CollisionInfo.Normal);
            Vector3 groundedMovement = planarVelocity * Context.InputRequest.MovementDirection.magnitude;

            Vector3 targetVelocity = groundedMovement * Context.walkSpeed;
            velocity = Vector3.Lerp(
                a: velocity,
                b: targetVelocity,
                t: Context.GetInterpolationTime(Context.walkAccelerationResponse, deltaTime)
            );
        }
        
        protected override void SetTransitions()
        {
            AddTransition<PlayerIdleState>(() => Context.InputRequest.MovementDirection.magnitude <= 0.1f);
            AddTransition<PlayerJumpState>(() => Context.InputRequest.IsJumping);
        }
    }
}