namespace PopupPlugin
{
    /// <summary>
    /// Singleton pattern.
    /// </summary>
    public class SingletonPopup<T> : BasePopup where T : BasePopup
    {
        protected static T _instance;

        /// <summary>
        /// Singleton using for Popup Group only!
        /// </summary>
        /// <value>The instance.</value>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();
                    if (_instance == null) _instance = PopupManagerAbi.Ins.CheckInstancePopupPrebab<T>();
                }

                return _instance;
            }
        }

        public override void Awake()
        {
            base.Awake();
        }
    }
}