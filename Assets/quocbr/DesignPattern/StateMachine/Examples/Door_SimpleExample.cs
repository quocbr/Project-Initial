using UnityEngine;

namespace quocbr.DesignPattern.Examples
{
    /// <summary>
    /// Example đơn giản: Door có 4 states (Closed, Opening, Open, Closing)
    /// </summary>
    public enum DoorState
    {
        Closed,
        Opening,
        Open,
        Closing
    }

    public class Door : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float openHeight = 3f;
        [SerializeField] private float openSpeed = 2f;

        private StateMachine<DoorState> _stateMachine;
        private Vector3 _closedPosition;
        private Vector3 _openPosition;

        private void Awake()
        {
            _closedPosition = transform.position;
            _openPosition = _closedPosition + Vector3.up * openHeight;

            _stateMachine = new StateMachine<DoorState>();
            _stateMachine.ChangeState(DoorState.Closed, new DoorClosedState(this));
            _stateMachine.OnStateChanged += OnDoorStateChanged;
        }

        private void Update()
        {
            _stateMachine.Update();
        }

        private void OnDoorStateChanged(DoorState oldState, DoorState newState)
        {
            Debug.Log($"[Door] {oldState} → {newState}");
        }

        // Public methods
        public void Open()
        {
            if (_stateMachine.IsInState(DoorState.Closed))
            {
                _stateMachine.ChangeState(DoorState.Opening, new DoorOpeningState(this));
            }
        }

        public void Close()
        {
            if (_stateMachine.IsInState(DoorState.Open))
            {
                _stateMachine.ChangeState(DoorState.Closing, new DoorClosingState(this));
            }
        }

        // Accessors
        public Vector3 ClosedPosition => _closedPosition;
        public Vector3 OpenPosition => _openPosition;
        public float OpenSpeed => openSpeed;
        public StateMachine<DoorState> StateMachine => _stateMachine;

        // Trigger để test
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Open();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Close();
            }
        }
    }

    // --- Door States ---

    public class DoorClosedState : BaseState<Door>
    {
        public DoorClosedState(Door context) : base(context) { }

        public override void OnEnter()
        {
            Debug.Log("[Door] Cửa đóng");
        }
    }

    public class DoorOpeningState : BaseState<Door>
    {
        public DoorOpeningState(Door context) : base(context) { }

        public override void OnEnter()
        {
            Debug.Log("[Door] Đang mở cửa...");
        }

        public override void OnUpdate()
        {
            // Di chuyển cửa lên
            Context.transform.position = Vector3.MoveTowards(
                Context.transform.position,
                Context.OpenPosition,
                Context.OpenSpeed * Time.deltaTime
            );

            // Đã mở xong
            if (Vector3.Distance(Context.transform.position, Context.OpenPosition) < 0.01f)
            {
                Context.StateMachine.ChangeState(DoorState.Open, new DoorOpenState(Context));
            }
        }
    }

    public class DoorOpenState : BaseState<Door>
    {
        public DoorOpenState(Door context) : base(context) { }

        public override void OnEnter()
        {
            Debug.Log("[Door] Cửa đã mở");
            Context.transform.position = Context.OpenPosition; // Snap to position
        }
    }

    public class DoorClosingState : BaseState<Door>
    {
        public DoorClosingState(Door context) : base(context) { }

        public override void OnEnter()
        {
            Debug.Log("[Door] Đang đóng cửa...");
        }

        public override void OnUpdate()
        {
            // Di chuyển cửa xuống
            Context.transform.position = Vector3.MoveTowards(
                Context.transform.position,
                Context.ClosedPosition,
                Context.OpenSpeed * Time.deltaTime
            );

            // Đã đóng xong
            if (Vector3.Distance(Context.transform.position, Context.ClosedPosition) < 0.01f)
            {
                Context.StateMachine.ChangeState(DoorState.Closed, new DoorClosedState(Context));
            }
        }
    }
}
