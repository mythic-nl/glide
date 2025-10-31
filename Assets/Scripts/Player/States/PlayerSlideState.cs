using Player.States._Base;
using UnityEngine;

namespace Player.States
{
    public class PlayerSlideState : PlayerState<PlayerContext>
    {
        protected override void OnEnterVelocity(ref Vector3 velocity, float deltaTime)
        {
            Vector3 planarVelocity = Vector3.ProjectOnPlane(velocity, Context.CollisionInfo.Normal);
            
            velocity = planarVelocity;
        }
        
        protected override void OnUpdateVelocity(ref Vector3 velocity, float deltaTime)
        {
            velocity = Context.GetVelocityAfterFriction(velocity, Context.slideFriction, deltaTime);
            
            Vector3 force = Vector3.ProjectOnPlane(-Context.CharacterInfo.Up, Context.CollisionInfo.Normal) * Context.downwardForce;
            
            velocity += force * deltaTime;
        }
        
        protected override void SetTransitions()
        {
            AddTransition<PlayerIdleState>(ToIdleCondition);
            AddTransition<PlayerMovementState>(ToMovementCondition);
            AddTransition<PlayerJumpState>(() => Context.InputRequest.IsJumping);
            AddTransition<PlayerAirborneState>(() => Context.CollisionInfo.Grounded == false);
        }

        private bool ToMovementCondition()
        {
            return Context.CollisionInfo.Grounded && 
                   Context.CollisionInfo.Angle < 45f && 
                   Context.InputRequest.MovementDirection.magnitude > 0.1f;
        }
        
        private bool ToIdleCondition()
        {
            return Context.CollisionInfo.Grounded && 
                   Context.CollisionInfo.Angle < 45f && 
                   Context.CharacterInfo.Velocity.magnitude < Context.slideEndSpeed &&
                   Context.InputRequest.MovementDirection.magnitude <= 0.1f;
        }
    }
}