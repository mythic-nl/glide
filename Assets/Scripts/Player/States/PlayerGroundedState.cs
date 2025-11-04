using System;
using Player.States._Base;
using Player.Structs;
using UnityEngine;
using Utils;

namespace Player.States
{
    public class PlayerGroundedState : PlayerState<PlayerContext>
    {
        protected override void OnEnter()
        {
            SetChild<PlayerIdleState>();
        }
        
        protected override void OnUpdateRotation(ref Quaternion rotation, float deltaTime)
        {
            if (Context.CharacterInfo.Velocity.magnitude >= 0.1f) {
                Vector3 planarVelocity = Vector3.ProjectOnPlane(Context.CharacterInfo.Velocity, Context.CharacterInfo.Up);
                
                if (planarVelocity.sqrMagnitude < 0.001f) {
                    return;
                }
                
                Quaternion turnRotation = Quaternion.LookRotation(planarVelocity, Context.CharacterInfo.Up);

                // NOTE: Applying pitch/roll to the root transform will tilt the collider (CharacterController/Capsule),
                // which breaks collisions. Only apply yaw (rotation around the up axis) to the root. If you want a
                // visual forward lean based on velocity, apply it to a visual child (mesh/graphics) instead.

                // Only use the yaw from the computed turnRotation for the root/collider.
                Quaternion targetRotation = Quaternion.Euler(0f, turnRotation.eulerAngles.y, 0f);

                rotation = Quaternion.RotateTowards(
                    from: rotation,
                    to: targetRotation,
                    maxDegreesDelta: Context.rotationSpeed.Value * deltaTime
                );

                // If you still want to visually tilt the model forward based on velocity (without affecting the collider),
                // consider applying a pitch to a visual child transform, e.g.:
                // visualModel.localRotation = Quaternion.Euler(visualPitch, 0f, 0f);
            }
        }

        protected override void OnUpdateVelocity(ref Vector3 velocity, float deltaTime)
        {
            HandleUpdateGravity(ref velocity, deltaTime);
        }
        
        protected override void SetTransitions()
        {
            AddTransition<PlayerAirborneState>(() => Context.CollisionInfo.Grounded == false);
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