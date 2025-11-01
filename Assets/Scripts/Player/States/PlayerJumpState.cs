using Player.States._Base;
using UnityEngine;

namespace Player.States
{
    public class PlayerJumpState : PlayerState<PlayerContext>
    {
        protected override void OnEnterVelocity(ref Vector3 velocity, float deltaTime)
        {
            float currentVerticalSpeed = Vector3.Dot(velocity, Context.CharacterInfo.Up);
            float targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, Context.upwardForce.Value);
                
            velocity += Context.CharacterInfo.Up * (targetVerticalSpeed - currentVerticalSpeed);
        }
        
        protected override void SetTransitions()
        {
        }
    }
}