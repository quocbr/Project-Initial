using System;

namespace quocbr.DesignPattern
{
    /// <summary>
    /// State Machine quản lý các state và chuyển đổi giữa chúng
    /// Generic TState: Enum định nghĩa các trạng thái (EnemyState, PlayerState, UIState, v.v.)
    /// </summary>
    public class StateMachine<TState> where TState : Enum
    {
        private IState _currentState;
        private TState _currentStateKey;

        /// <summary>
        /// State hiện tại (enum key)
        /// </summary>
        public TState CurrentStateKey => _currentStateKey;

        /// <summary>
        /// State hiện tại (IState object)
        /// </summary>
        public IState CurrentState => _currentState;

        /// <summary>
        /// Event được gọi khi chuyển state
        /// </summary>
        public event Action<TState, TState> OnStateChanged;

        /// <summary>
        /// Thay đổi sang state mới
        /// </summary>
        public void ChangeState(TState newStateKey, IState newState)
        {
            // Exit state cũ
            _currentState?.OnExit();

            TState oldStateKey = _currentStateKey;
            _currentStateKey = newStateKey;
            _currentState = newState;

            // Enter state mới
            _currentState?.OnEnter();

            // Trigger event
            OnStateChanged?.Invoke(oldStateKey, newStateKey);
        }

        /// <summary>
        /// Update state hiện tại (gọi trong MonoBehaviour.Update)
        /// </summary>
        public void Update()
        {
            _currentState?.OnUpdate();
        }

        /// <summary>
        /// FixedUpdate state hiện tại (gọi trong MonoBehaviour.FixedUpdate)
        /// </summary>
        public void FixedUpdate()
        {
            _currentState?.OnFixedUpdate();
        }

        /// <summary>
        /// Kiểm tra có đang ở state nào đó không
        /// </summary>
        public bool IsInState(TState stateKey)
        {
            return _currentStateKey.Equals(stateKey);
        }
    }
}
