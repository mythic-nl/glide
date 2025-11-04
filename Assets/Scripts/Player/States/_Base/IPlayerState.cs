using System.Collections.Generic;
using Stateforge.Runtime.Interfaces;

namespace Player.States._Base
{
    public interface IPlayerState<TContext> where TContext : IContext
    {
        public bool IsRootState { get; }
        public IPlayerState<TContext> ParentState { get; set; }
        public IPlayerState<TContext> ChildState { get; set; }
        
        public HashSet<ITransition<TContext>> Transitions { get; }
        
        public void Create(IStateMachine<TContext> stateMachine, TContext context, bool isRoot = true);
        public void Setup();
        public void Enter();
        public void Exit();
        public void Update();
    }    
}