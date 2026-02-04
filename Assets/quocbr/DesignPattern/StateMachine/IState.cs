namespace quocbr.DesignPattern
{
    /// <summary>
    /// Interface cho tất cả các State trong State Machine
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// Gọi khi vào state này
        /// </summary>
        void OnEnter();

        /// <summary>
        /// Gọi mỗi frame khi đang ở state này
        /// </summary>
        void OnUpdate();

        /// <summary>
        /// Gọi mỗi fixed frame (dùng cho physics)
        /// </summary>
        void OnFixedUpdate();

        /// <summary>
        /// Gọi khi rời khỏi state này
        /// </summary>
        void OnExit();
    }
}
