using Player.States._Base;
using UnityEngine;

namespace Player.States
{
    public class PlayerSlideState : PlayerState<PlayerContext>
    {
        protected override void OnUpdateVelocity(ref Vector3 velocity, float deltaTime)
        {
            velocity = Context.GetVelocityAfterFriction(velocity, Context.slideFriction, deltaTime);
            
            Vector3 force = Vector3.ProjectOnPlane(-Context.CharacterInfo.Up, Context.CollisionInfo.Normal) * Context.downwardForce;
            
            velocity += force * deltaTime;
        }
        
        protected override void SetTransitions()
        {
            AddTransition<PlayerGroundedState>(() => Context.CollisionInfo.Grounded && Context.CollisionInfo.Angle < 45f);
            AddTransition<PlayerAirborneState>(() => Context.CollisionInfo.Grounded == false);
        }
    }
}