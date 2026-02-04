# State Machine Pattern - Hướng dẫn sử dụng

## 📖 Tổng quan

State Machine Pattern giúp quản lý các trạng thái (states) của object và chuyển đổi giữa chúng một cách có tổ chức, dễ maintain và mở rộng.

## 🏗️ Kiến trúc

### Core Components

1. **IState** - Interface định nghĩa các method cơ bản:
   - `OnEnter()` - Gọi khi vào state
   - `OnUpdate()` - Gọi mỗi frame
   - `OnFixedUpdate()` - Gọi mỗi physics frame
   - `OnExit()` - Gọi khi rời state

2. **StateMachine<TState>** - Generic state machine:
   - `TState`: Enum định nghĩa các trạng thái
   - Quản lý state hiện tại
   - Xử lý chuyển đổi state
   - Event `OnStateChanged`

3. **BaseState<TContext>** - Base class cho states:
   - `TContext`: MonoBehaviour chứa state machine
   - Cung cấp reference tới context object

## 🚀 Cách sử dụng

### Bước 1: Định nghĩa Enum cho states
**Xem các file Example để biết cách implement chi tiết!**

---

```
}
    private StateMachine<CombatSubState> _subStateMachine;
{
public class CombatState : BaseState<Player>
// Parent state có thể chứa sub-states
```csharp

### Hierarchical State Machine

```
}
    public System.Func<bool> Condition;
    public TState ToState;
    public TState FromState;
{
public class StateTransition<TState> where TState : Enum
```csharp

### Thêm Transition Conditions

## 🔜 Mở rộng

| **Best for** | Logic, AI | Animation |
| **Debug** | Dễ debug | Khó debug |
| **Visual** | Code-based | Visual graph |
| **Flexibility** | Rất linh hoạt | Giới hạn |
| **Performance** | Nhanh hơn | Slower |
|---------|--------------|----------|
| Feature | State Machine | Animator |

## 🎯 So sánh với Animator

- Dùng `IsInState()` để check state hiện tại
- State nên **stateless** khi có thể, data nên ở Context
- Không gọi `ChangeState()` trong `OnEnter()` hoặc `OnExit()`
- State Machine là **single-threaded**, gọi trong main thread

## ⚠️ Lưu ý

   - Subscribe `OnStateChanged` để log, analytics, sounds
5. ✅ **Dùng Events**

   - Để logic transition ở Context (MonoBehaviour)
   - State không nên biết về state khác
4. ✅ **Tránh State phụ thuộc nhau**

   - `OnExit()`: Cleanup, stop animations, reset
   - `OnEnter()`: Initialize, play animation, set flags
3. ✅ **OnEnter/OnExit cho Setup/Cleanup**

   - Không hardcode transitions trong state class
   - Kiểm tra điều kiện chuyển state trong `Update()`
2. ✅ **State Transitions trong Update**

   - `PlayerState` cho `PlayerController`
   - `EnemyState` cho `EnemyAI`
   - Mỗi MonoBehaviour nên có enum riêng
1. ✅ **Một Enum cho một Context**

## 💡 Best Practices

- ✅ Tutorial Steps
- ✅ Cutscene Management
- ✅ Animation States
### 5. Animation

- ✅ Quest System (NotStarted, InProgress, Completed, Failed)
- ✅ Elevator (Idle, MovingUp, MovingDown, Arrived)
- ✅ Door (Closed, Opening, Open, Closing)
### 4. Game Systems

- ✅ Dialog System (Hidden, Showing, Typing, WaitInput)
- ✅ Game States (Menu, Gameplay, Pause, GameOver)
- ✅ Menu Flow (MainMenu, Settings, Credits, Loading)
### 3. UI/Menu System

- ✅ Vehicle (Idle, Accelerate, Brake, Drift, Crash)
- ✅ Combat System (Idle, Attack, Block, Dodge, Parry)
- ✅ Character Controller (Idle, Walk, Run, Jump, Crouch, Slide)
### 2. Player Control

- ✅ Boss AI (Phase 1, Phase 2, Phase 3, Enraged)
- ✅ NPC AI (Idle, Talk, Walk, Work, Sleep)
- ✅ Enemy AI (Idle, Patrol, Chase, Attack, Flee)
### 1. AI Behavior

## 🔧 Use Cases

```
}
    // Enemy is dead
{
if (_stateMachine.CurrentStateKey == EnemyState.Dead)
// Hoặc

}
    // Do something when attacking
{
if (_stateMachine.IsInState(EnemyState.Attack))
```csharp

### Kiểm tra State hiện tại

```
}
    }
        _animator.SetBool("IsWalking", false);
    {
    public override void OnExit()

    }
        _animator.SetBool("IsWalking", true);
    {
    public override void OnEnter()

    }
        _animator = context.GetComponent<Animator>();
    {
    public WalkState(Player context) : base(context) 

    private Animator _animator;
{
public class WalkState : BaseState<Player>
```csharp

### State với Animation

```
}
    }
        }
            Context.ChangeToIdleState();
            // Transition back to idle
        {
        if (_timer <= 0)
        
        _timer -= Time.deltaTime;
    {
    public override void OnUpdate()

    }
        Context.PlayAttackAnimation();
        _timer = _attackDuration;
    {
    public override void OnEnter()

    public AttackState(Enemy context) : base(context) { }

    private float _timer;
    private float _attackDuration = 1f;
{
public class AttackState : BaseState<Enemy>
```csharp

### State với Timer

## ✨ Ví dụ nâng cao

```
File: UIManager_Example.cs
States: MainMenu → GameplayUI → PauseMenu → GameOver
```
### 3. UI Manager (Menu, Pause, Gameplay)

```
File: PlayerController_Example.cs
States: Idle → Walking → Running → Jumping → Attacking
```
### 2. Player Controller (Di chuyển, nhảy, tấn công)

```
File: EnemyAI_Example.cs
States: Idle → Patrol → Chase → Attack
```
### 1. Enemy AI (AI với Patrol, Chase, Attack)

## 📚 Examples

```
}
    }
        Debug.Log($"State: {oldState} → {newState}");
    {
    private void OnStateChanged(EnemyState oldState, EnemyState newState)

    }
        _stateMachine.FixedUpdate();
    {
    private void FixedUpdate()

    }
        }
            _stateMachine.ChangeState(EnemyState.Chase, new ChaseState(this));
        {
        if (PlayerInRange())
        // Check transitions
        
        _stateMachine.Update();
    {
    private void Update()

    }
        _stateMachine.OnStateChanged += OnStateChanged;
        // Subscribe to state change event (optional)
        
        _stateMachine.ChangeState(EnemyState.Idle, new IdleState(this));
        // Set initial state
        
        _stateMachine = new StateMachine<EnemyState>();
        // Khởi tạo state machine
    {
    private void Awake()

    private StateMachine<EnemyState> _stateMachine;
{
public class EnemyAI : MonoBehaviour
```csharp

### Bước 3: Setup State Machine trong MonoBehaviour

```
}
    }
        Debug.Log("Exit Idle State");
    {
    public override void OnExit()

    }
        // Logic cho idle state
    {
    public override void OnUpdate()

    }
        Debug.Log("Enter Idle State");
    {
    public override void OnEnter()

    public IdleState(EnemyAI context) : base(context) { }
{
public class IdleState : BaseState<EnemyAI>
```csharp

### Bước 2: Tạo State classes

```
}
    Dead
    Attack,
    Chase,
    Patrol,
    Idle,
{
public enum EnemyState
```csharp

