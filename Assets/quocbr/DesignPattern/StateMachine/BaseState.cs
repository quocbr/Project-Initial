using UnityEngine;

namespace quocbr.DesignPattern
{
    /// <summary>
    /// Base class cho State, cung cấp reference tới context object
    /// Context: MonoBehaviour chứa state machine (Enemy, Player, UI, v.v.)
    /// </summary>
    public abstract class BaseState<TContext> : IState where TContext : MonoBehaviour
    {
        protected TContext Context { get; private set; }

        public BaseState(TContext context)
        {
            Context = context;
        }

        public virtual void OnEnter() { }
        public virtual void OnUpdate() { }
        public virtual void OnFixedUpdate() { }
        public virtual void OnExit() { }
    }
}
