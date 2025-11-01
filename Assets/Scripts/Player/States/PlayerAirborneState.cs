using Player.States._Base;
using UnityEngine;
using Utils;

namespace Player.States
{
    public class PlayerAirborneState : PlayerState<PlayerContext>
    {
        protected override void OnEnter()
        {
            SetChild<PlayerAirStrafeState>();
        }
        
        protected override void OnUpdateRotation(ref Quaternion rotation, float deltaTime)
        {
            if (Context.CharacterInfo.Velocity.magnitude >= 0.1f) {
                Vector3 planarVelocity = Vector3.ProjectOnPlane(Context.CharacterInfo.Velocity, Context.CharacterInfo.Up);

                if (planarVelocity.sqrMagnitude < 0.001f) {
                    return;
                }
                
                Quaternion targetRotation = Quaternion.LookRotation(planarVelocity, Context.CharacterInfo.Up);
                rotation = Quaternion.RotateTowards(
                    from: rotation,
                    to: targetRotation,
                    maxDegreesDelta: Context.rotationSpeed.Value * deltaTime
                );
            }
        }
        
        protected override void OnUpdateVelocity(ref Vector3 velocity, float deltaTime)
        {
            HandleUpdateGravity(ref velocity, deltaTime);
        }
        
        protected override void SetTransitions()
        {
            AddTransition<PlayerGroundedState>(() => Context.CollisionInfo.Grounded);
        }

        private void HandleUpdateGravity(ref Vector3 velocity, float deltaTime)
        {
            Vector3 planarVelocity = Vector3.ProjectOnPlane(velocity, Context.CharacterInfo.Up);
            float verticalSpeed = Vector3.Dot(velocity, Context.CharacterInfo.Up);

            if (Context.CollisionInfo.Grounded == false) {
                float targetVerticalSpeed = -Context.gravity.Value;
                verticalSpeed = Mathf.Lerp(
                    a: verticalSpeed,
                    b: targetVerticalSpeed,
                    t: Common.GetInterpolationTime(Context.gravityResponse.Value, deltaTime)
                );
            }

            velocity = planarVelocity + (Context.CharacterInfo.Up * verticalSpeed);
        }
    }
}