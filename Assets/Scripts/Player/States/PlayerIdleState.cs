using Player.States._Base;
using UnityEngine;

namespace Player.States
{
    public class PlayerIdleState : PlayerState<PlayerContext>
    {
        protected override void OnUpdateVelocity(ref Vector3 velocity, float deltaTime)
        {
            velocity = Context.GetVelocityAfterFriction(velocity, Context.baseFriction, deltaTime);
        }
        
        protected override void SetTransitions()
        {
            AddTransition<PlayerSlideState>(() => Context.CollisionInfo.Angle >= 45f);
            AddTransition<PlayerMovementState>(() => Context.InputRequest.MovementDirection.magnitude > 0.1f && Context.CollisionInfo.Angle < 45f);
            AddTransition<PlayerJumpState>(() => Context.InputRequest.IsJumping);
        }
    }
}