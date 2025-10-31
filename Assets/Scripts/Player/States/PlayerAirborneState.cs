using Player.States._Base;
using UnityEngine;

namespace Player.States
{
    public class PlayerAirborneState : PlayerState<PlayerContext>
    {
        protected override void OnEnter()
        {
            SetChild<PlayerAirStrafeState>();
        }
        
        protected override void OnUpdateVelocity(ref Vector3 velocity, float deltaTime)
        {
            HandleUpdateGravity(ref velocity, deltaTime);
        }
        
        protected override void SetTransitions()
        {
            AddTransition<PlayerSlideState>(() => Context.CollisionInfo.Grounded && Context.CollisionInfo.Angle >= 45f);
            AddTransition<PlayerGroundedState>(() => Context.CollisionInfo.Grounded && Context.CollisionInfo.Angle < 45f);
        }

        private void HandleUpdateGravity(ref Vector3 velocity, float deltaTime)
        {
            Vector3 planarVelocity = Vector3.ProjectOnPlane(velocity, Context.CharacterInfo.Up);
            float verticalSpeed = Vector3.Dot(velocity, Context.CharacterInfo.Up);

            if (Context.CollisionInfo.Grounded == false) {
                float targetVerticalSpeed = -Context.gravity;
                verticalSpeed = Mathf.Lerp(
                    verticalSpeed,
                    targetVerticalSpeed,
                    Context.GetInterpolationTime(Context.gravityResponse, deltaTime)
                    );
            }

            velocity = planarVelocity + (Context.CharacterInfo.Up * verticalSpeed);
        }
    }
}