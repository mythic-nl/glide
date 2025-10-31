using Player.States;
using Stateforge.Runtime;

namespace Player
{
    public class PlayerStateFactory : StateFactory<PlayerContext>
    {
        protected override void SetStates()
        {
            GroundedState();
            AirborneState();
            SlideState();
        }

        private void GroundedState()
        {
            AddRootState<PlayerGroundedState>();
            
            AddChildState<PlayerGroundedState, PlayerIdleState>();
            AddChildState<PlayerGroundedState, PlayerMovementState>();
            AddChildState<PlayerGroundedState, PlayerJumpState>();
            AddChildState<PlayerGroundedState, PlayerSlideState>();
        }
        
        private void AirborneState()
        {
            AddRootState<PlayerAirborneState>();
            
            AddChildState<PlayerAirborneState, PlayerAirStrafeState>();
        }

        private void SlideState()
        {
            //AddRootState<PlayerSlideState>();
        }
    }
}