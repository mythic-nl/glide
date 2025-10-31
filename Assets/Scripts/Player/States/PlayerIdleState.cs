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
            AddTransition<PlayerMovementState>(() => Context.InputRequest.MovementDirection.magnitude > 0.1f);
            AddTransition<PlayerJumpState>(() => Context.InputRequest.IsJumping);
        }
    }
}