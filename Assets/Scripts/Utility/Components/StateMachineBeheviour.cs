namespace Cozy.Builder.Utility.Components
{
    using UnityEngine;

    /// <summary>
    /// IMonoState represents a state in a MonoBehaviour-based state machine. It provides Enter and Exit methods for 
    /// handling state transitions, as well as a reference to the context MonoBehaviour that the state operates on.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public abstract class IMonoState<TContext> : MonoBehaviour where TContext : MonoBehaviour
    {
        protected TContext Context { get; private set; }

        public virtual void Enter(TContext context) { Context = context; }
        public virtual void Exit(TContext context) { Context = null; }
    }
    
    /// <summary>
    /// MonoStateMachine is a base class for creating MonoBehaviour-based state machines. It manages the current state 
    /// and provides a method for transitioning between states.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public abstract class MonoStateMachine<TContext> : MonoBehaviour where TContext : MonoBehaviour
    {
        private IMonoState<TContext> currentState;

        /// <summary>
        /// EnterState handles transitioning to a new state by first exiting the current state (if one exists) and then creating and entering the new state.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="context"></param>
        public void EnterState<TState>(TContext context) where TState : IMonoState<TContext>
        {
            if (currentState != null)
            {
                if (currentState is TState)
                    return; // Already in the desired state, no transition needed.

                ExitState(context);
            }

            currentState = gameObject.AddComponent<TState>();
            currentState.Enter(context);
        }

        /// <summary>
        /// ExitState handles exiting the current state by calling its Exit method and then destroying the state component.
        /// After this method is called, there will be no active state until EnterState is called again to create a new state instance.
        /// </summary>
        /// <param name="context"></param>
        public void ExitState(TContext context)
        {
            if (currentState != null)
            {
                currentState.Exit(context);
                Destroy(currentState);
                currentState = null;
            }
        }
    }
}