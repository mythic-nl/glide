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
        }

        private void GroundedState()
        {
            AddRootState<PlayerGroundedState>();
            
            AddChildState<PlayerGroundedState, PlayerIdleState>();
            AddChildState<PlayerGroundedState, PlayerMovementState>();
            AddChildState<PlayerGroundedState, PlayerJumpState>();
        }
        
        private void AirborneState()
        {
            AddRootState<PlayerAirborneState>();
            
            AddChildState<PlayerAirborneState, PlayerAirStrafeState>();
        }
    }
}