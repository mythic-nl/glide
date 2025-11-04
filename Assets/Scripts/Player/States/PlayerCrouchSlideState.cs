using Player.States._Base;
using UnityEngine;

namespace Player.States
{
    public class PlayerCrouchSlideState : PlayerState<PlayerContext>
    {
        protected override void OnEnterVelocity(ref Vector3 velocity, float deltaTime)
        {
            var slideSpeed = Mathf.Max(Context.slideStartSpeed.Value, velocity.magnitude);
            velocity = Context.GetDirectionTangentToSurface(
                velocity,
                Context.CollisionInfo.Normal
            ) * slideSpeed;   
        }
        
        protected override void OnUpdateVelocity(ref Vector3 velocity, float deltaTime)
        {
            velocity = Context.GetVelocityAfterFriction(velocity, Context.slideFriction.Value, deltaTime);
            
            Vector3 force = Vector3.ProjectOnPlane(-Context.CharacterInfo.Up, Context.CollisionInfo.Normal) * Context.downwardForce.Value;
            Vector3 planarVelocity = Context.GetDirectionTangentToSurface(Context.InputRequest.MovementDirection, Context.CollisionInfo.Normal);
            Vector3 groundedMovement = planarVelocity * Context.InputRequest.MovementDirection.magnitude;
            
            velocity += groundedMovement + force * deltaTime;
        }

        protected override void SetTransitions()
        {
            AddTransition<PlayerIdleState>(ToIdleCondition);
            AddTransition<PlayerMovementState>(ToMovementCondition);
            AddTransition<PlayerSlideState>(ToSlideCondition);
            AddTransition<PlayerJumpState>(() => Context.InputRequest.IsJumping);
            AddTransition<PlayerAirborneState>(() => Context.CollisionInfo.Grounded == false);
        }
        
        private bool ToMovementCondition()
        {
            return Context.CollisionInfo.Grounded && 
                   Context.CollisionInfo.Angle < 45f && 
                   Context.InputRequest.MovementDirection.magnitude > 0.1f &&
                   Context.CharacterInfo.Velocity.magnitude <= 0.1f;
        }
        
        private bool ToIdleCondition()
        {
            return Context.CollisionInfo.Grounded &&
                   Context.CollisionInfo.Angle < 45f &&
                   Context.InputRequest.MovementDirection.magnitude <= 0.1f &&
                   Context.CharacterInfo.Velocity.magnitude < Context.slideEndSpeed.Value;
        }
        
        private bool ToSlideCondition()
        {
            return Context.CollisionInfo.Grounded && 
                   Context.CollisionInfo.Angle >= 45f &&
                   Context.CharacterInfo.Velocity.magnitude >= Context.slideEndSpeed.Value;
        }
    }
}