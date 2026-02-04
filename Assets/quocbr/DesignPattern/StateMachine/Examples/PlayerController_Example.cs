using UnityEngine;

namespace quocbr.DesignPattern.Examples
{
    /// <summary>
    /// Enum định nghĩa các state của Player
    /// </summary>
    public enum PlayerState
    {
        Idle,
        Walking,
        Running,
        Jumping,
        Attacking,
        Dead
    }

    /// <summary>
    /// Example: Player Controller sử dụng State Machine
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float walkSpeed = 3f;
        [SerializeField] private float runSpeed = 6f;
        [SerializeField] private float jumpForce = 5f;

        private StateMachine<PlayerState> _stateMachine;
        private CharacterController _controller;
        private Vector3 _velocity;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();

            // Khởi tạo State Machine
            _stateMachine = new StateMachine<PlayerState>();
            _stateMachine.ChangeState(PlayerState.Idle, new PlayerIdleState(this));

            // Subscribe to state change event
            _stateMachine.OnStateChanged += OnStateChanged;
        }

        private void Update()
        {
            _stateMachine.Update();
            HandleInput();
        }

        private void HandleInput()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            bool isMoving = horizontal != 0 || vertical != 0;
            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            bool jumpPressed = Input.GetKeyDown(KeyCode.Space);
            bool attackPressed = Input.GetMouseButtonDown(0);

            // State transitions
            switch (_stateMachine.CurrentStateKey)
            {
                case PlayerState.Idle:
                    if (attackPressed)
                        _stateMachine.ChangeState(PlayerState.Attacking, new PlayerAttackState(this));
                    else if (jumpPressed && _controller.isGrounded)
                        _stateMachine.ChangeState(PlayerState.Jumping, new PlayerJumpState(this));
                    else if (isMoving && isRunning)
                        _stateMachine.ChangeState(PlayerState.Running, new PlayerRunState(this));
                    else if (isMoving)
                        _stateMachine.ChangeState(PlayerState.Walking, new PlayerWalkState(this));
                    break;

                case PlayerState.Walking:
                    if (attackPressed)
                        _stateMachine.ChangeState(PlayerState.Attacking, new PlayerAttackState(this));
                    else if (jumpPressed && _controller.isGrounded)
                        _stateMachine.ChangeState(PlayerState.Jumping, new PlayerJumpState(this));
                    else if (!isMoving)
                        _stateMachine.ChangeState(PlayerState.Idle, new PlayerIdleState(this));
                    else if (isRunning)
                        _stateMachine.ChangeState(PlayerState.Running, new PlayerRunState(this));
                    break;

                case PlayerState.Running:
                    if (attackPressed)
                        _stateMachine.ChangeState(PlayerState.Attacking, new PlayerAttackState(this));
                    else if (jumpPressed && _controller.isGrounded)
                        _stateMachine.ChangeState(PlayerState.Jumping, new PlayerJumpState(this));
                    else if (!isMoving)
                        _stateMachine.ChangeState(PlayerState.Idle, new PlayerIdleState(this));
                    else if (!isRunning)
                        _stateMachine.ChangeState(PlayerState.Walking, new PlayerWalkState(this));
                    break;

                case PlayerState.Jumping:
                    if (_controller.isGrounded && _velocity.y <= 0)
                    {
                        if (isMoving)
                            _stateMachine.ChangeState(PlayerState.Walking, new PlayerWalkState(this));
                        else
                            _stateMachine.ChangeState(PlayerState.Idle, new PlayerIdleState(this));
                    }
                    break;

                case PlayerState.Attacking:
                    // Attack state will handle its own transition back to idle
                    break;
            }
        }

        private void OnStateChanged(PlayerState oldState, PlayerState newState)
        {
            Debug.Log($"[Player] State: {oldState} → {newState}");
        }

        // Public accessors
        public CharacterController Controller => _controller;
        public float WalkSpeed => walkSpeed;
        public float RunSpeed => runSpeed;
        public float JumpForce => jumpForce;
        public Vector3 Velocity { get => _velocity; set => _velocity = value; }
    }

    // --- Player States ---

    public class PlayerIdleState : BaseState<PlayerController>
    {
        public PlayerIdleState(PlayerController context) : base(context) { }

        public override void OnEnter()
        {
            Debug.Log("[Player] → Idle");
        }

        public override void OnUpdate()
        {
            // Apply gravity
            ApplyGravity();
        }

        private void ApplyGravity()
        {
            if (!Context.Controller.isGrounded)
            {
                Vector3 velocity = Context.Velocity;
                velocity.y += Physics.gravity.y * Time.deltaTime;
                Context.Velocity = velocity;
                Context.Controller.Move(velocity * Time.deltaTime);
            }
        }
    }

    public class PlayerWalkState : BaseState<PlayerController>
    {
        public PlayerWalkState(PlayerController context) : base(context) { }

        public override void OnEnter()
        {
            Debug.Log("[Player] → Walking");
        }

        public override void OnUpdate()
        {
            Move(Context.WalkSpeed);
        }

        private void Move(float speed)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 movement = new Vector3(horizontal, 0, vertical).normalized;
            Context.Controller.Move(movement * speed * Time.deltaTime);

            // Apply gravity
            if (!Context.Controller.isGrounded)
            {
                Vector3 velocity = Context.Velocity;
                velocity.y += Physics.gravity.y * Time.deltaTime;
                Context.Velocity = velocity;
                Context.Controller.Move(velocity * Time.deltaTime);
            }

            // Rotate player
            if (movement != Vector3.zero)
            {
                Context.transform.forward = movement;
            }
        }
    }

    public class PlayerRunState : BaseState<PlayerController>
    {
        public PlayerRunState(PlayerController context) : base(context) { }

        public override void OnEnter()
        {
            Debug.Log("[Player] → Running");
        }

        public override void OnUpdate()
        {
            Move(Context.RunSpeed);
        }

        private void Move(float speed)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 movement = new Vector3(horizontal, 0, vertical).normalized;
            Context.Controller.Move(movement * speed * Time.deltaTime);

            // Apply gravity
            if (!Context.Controller.isGrounded)
            {
                Vector3 velocity = Context.Velocity;
                velocity.y += Physics.gravity.y * Time.deltaTime;
                Context.Velocity = velocity;
                Context.Controller.Move(velocity * Time.deltaTime);
            }

            // Rotate player
            if (movement != Vector3.zero)
            {
                Context.transform.forward = movement;
            }
        }
    }

    public class PlayerJumpState : BaseState<PlayerController>
    {
        public PlayerJumpState(PlayerController context) : base(context) { }

        public override void OnEnter()
        {
            Debug.Log("[Player] → Jumping");
            Vector3 velocity = Context.Velocity;
            velocity.y = Context.JumpForce;
            Context.Velocity = velocity;
        }

        public override void OnUpdate()
        {
            // Apply gravity
            Vector3 velocity = Context.Velocity;
            velocity.y += Physics.gravity.y * Time.deltaTime;
            Context.Velocity = velocity;

            // Move horizontally while jumping
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Vector3 movement = new Vector3(horizontal, 0, vertical).normalized;
            
            Vector3 finalMovement = movement * Context.WalkSpeed * Time.deltaTime;
            finalMovement.y = velocity.y * Time.deltaTime;
            
            Context.Controller.Move(finalMovement);
        }
    }

    public class PlayerAttackState : BaseState<PlayerController>
    {
        private float _attackDuration = 0.5f;
        private float _attackTimer;

        public PlayerAttackState(PlayerController context) : base(context) { }

        public override void OnEnter()
        {
            Debug.Log("[Player] → Attacking!");
            _attackTimer = _attackDuration;
            // TODO: Play attack animation
        }

        public override void OnUpdate()
        {
            _attackTimer -= Time.deltaTime;

            // Return to idle after attack
            if (_attackTimer <= 0)
            {
                // This is a hack - normally you'd use a callback or event
                var stateMachine = new StateMachine<PlayerState>();
                // We need access to the parent's state machine here
                // In real implementation, you'd pass it as parameter or use event
            }
        }
    }
}
