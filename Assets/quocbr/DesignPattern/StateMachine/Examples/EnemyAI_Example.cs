using UnityEngine;

namespace quocbr.DesignPattern.Examples
{
    /// <summary>
    /// Enum định nghĩa các state của Enemy
    /// </summary>
    public enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Dead
    }

    /// <summary>
    /// Example: Enemy AI sử dụng State Machine
    /// </summary>
    public class EnemyAI : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float detectionRange = 10f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float moveSpeed = 3f;

        private StateMachine<EnemyState> _stateMachine;
        private Transform _player;

        private void Awake()
        {
            // Tìm player
            _player = GameObject.FindGameObjectWithTag("Player")?.transform;

            // Khởi tạo State Machine
            _stateMachine = new StateMachine<EnemyState>();

            // Set initial state
            _stateMachine.ChangeState(EnemyState.Idle, new IdleState(this));

            // Subscribe to state change event
            _stateMachine.OnStateChanged += OnStateChanged;
        }

        private void Update()
        {
            _stateMachine.Update();
            CheckTransitions();
        }

        private void FixedUpdate()
        {
            _stateMachine.FixedUpdate();
        }

        private void CheckTransitions()
        {
            if (_player == null) return;

            float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

            // Transition logic
            switch (_stateMachine.CurrentStateKey)
            {
                case EnemyState.Idle:
                case EnemyState.Patrol:
                    if (distanceToPlayer <= detectionRange)
                    {
                        _stateMachine.ChangeState(EnemyState.Chase, new ChaseState(this));
                    }
                    break;

                case EnemyState.Chase:
                    if (distanceToPlayer <= attackRange)
                    {
                        _stateMachine.ChangeState(EnemyState.Attack, new AttackState(this));
                    }
                    else if (distanceToPlayer > detectionRange * 1.5f)
                    {
                        _stateMachine.ChangeState(EnemyState.Patrol, new PatrolState(this));
                    }
                    break;

                case EnemyState.Attack:
                    if (distanceToPlayer > attackRange)
                    {
                        _stateMachine.ChangeState(EnemyState.Chase, new ChaseState(this));
                    }
                    break;
            }
        }

        private void OnStateChanged(EnemyState oldState, EnemyState newState)
        {
            Debug.Log($"[EnemyAI] State changed: {oldState} → {newState}");
        }

        // Public accessors for states
        public Transform Player => _player;
        public float MoveSpeed => moveSpeed;
        public float AttackRange => attackRange;

        private void OnDrawGizmosSelected()
        {
            // Draw detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }

    // --- Enemy States ---

    public class IdleState : BaseState<EnemyAI>
    {
        public IdleState(EnemyAI context) : base(context) { }

        public override void OnEnter()
        {
            Debug.Log("[Enemy] Entering Idle State");
        }

        public override void OnUpdate()
        {
            // Enemy đứng im, có thể thêm animation idle
        }
    }

    public class PatrolState : BaseState<EnemyAI>
    {
        private Vector3 _targetPosition;
        private float _waitTime;

        public PatrolState(EnemyAI context) : base(context) { }

        public override void OnEnter()
        {
            Debug.Log("[Enemy] Entering Patrol State");
            SetRandomTarget();
        }

        public override void OnUpdate()
        {
            // Di chuyển đến target
            Context.transform.position = Vector3.MoveTowards(
                Context.transform.position,
                _targetPosition,
                Context.MoveSpeed * Time.deltaTime
            );

            // Đến target rồi thì chọn target mới
            if (Vector3.Distance(Context.transform.position, _targetPosition) < 0.5f)
            {
                _waitTime -= Time.deltaTime;
                if (_waitTime <= 0)
                {
                    SetRandomTarget();
                }
            }
        }

        private void SetRandomTarget()
        {
            _targetPosition = Context.transform.position + Random.insideUnitSphere * 5f;
            _targetPosition.y = Context.transform.position.y;
            _waitTime = Random.Range(1f, 3f);
        }
    }

    public class ChaseState : BaseState<EnemyAI>
    {
        public ChaseState(EnemyAI context) : base(context) { }

        public override void OnEnter()
        {
            Debug.Log("[Enemy] Entering Chase State");
        }

        public override void OnUpdate()
        {
            if (Context.Player == null) return;

            // Chase player
            Vector3 direction = (Context.Player.position - Context.transform.position).normalized;
            Context.transform.position += direction * Context.MoveSpeed * Time.deltaTime;

            // Look at player
            Context.transform.LookAt(Context.Player);
        }
    }

    public class AttackState : BaseState<EnemyAI>
    {
        private float _attackCooldown = 1f;
        private float _nextAttackTime;

        public AttackState(EnemyAI context) : base(context) { }

        public override void OnEnter()
        {
            Debug.Log("[Enemy] Entering Attack State");
            _nextAttackTime = Time.time;
        }

        public override void OnUpdate()
        {
            if (Context.Player == null) return;

            // Look at player
            Context.transform.LookAt(Context.Player);

            // Attack
            if (Time.time >= _nextAttackTime)
            {
                Attack();
                _nextAttackTime = Time.time + _attackCooldown;
            }
        }

        private void Attack()
        {
            Debug.Log("[Enemy] ATTACK!");
            // TODO: Deal damage, play animation, etc.
        }
    }
}
