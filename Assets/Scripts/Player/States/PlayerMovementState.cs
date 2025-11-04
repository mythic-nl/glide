using Player.States._Base;
using UnityEngine;
using Utils;

namespace Player.States
{
    public class PlayerMovementState : PlayerState<PlayerContext>
    {
        protected override void OnUpdateVelocity(ref Vector3 velocity, float deltaTime)
        {
            Vector3 planarVelocity = Context.GetDirectionTangentToSurface(Context.InputRequest.MovementDirection, Context.CollisionInfo.Normal);
            Vector3 groundedMovement = planarVelocity * Context.InputRequest.MovementDirection.magnitude;

            float speed = (Context.InputRequest.IsSprinting) 
                ? Context.sprintSpeed.Value 
                : Context.walkSpeed.Value;
            
            float response = (Context.InputRequest.IsSprinting) 
                ? Context.sprintAccelerationResponse.Value 
                : Context.walkAccelerationResponse.Value;

            Vector3 targetVelocity = groundedMovement * speed;
            velocity = Vector3.Lerp(
                a: velocity,
                b: targetVelocity,
                t: Common.GetInterpolationTime(response, deltaTime)
            );
        }
        
        protected override void SetTransitions()
        {
            AddTransition<PlayerCrouchSlideState>(() => Context.InputRequest.IsCrouching && Context.CollisionInfo.Angle < 45f);
            AddTransition<PlayerSlideState>(() => Context.CollisionInfo.Angle >= 45f);
            AddTransition<PlayerIdleState>(() => Context.InputRequest.MovementDirection.magnitude <= 0.1f);
            AddTransition<PlayerJumpState>(() => Context.InputRequest.IsJumping);
        }
    }
}