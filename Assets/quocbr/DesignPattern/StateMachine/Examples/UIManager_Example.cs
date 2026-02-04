using UnityEngine;
using UnityEngine.UI;

namespace quocbr.DesignPattern.Examples
{
    /// <summary>
    /// Enum định nghĩa các state của UI Menu
    /// </summary>
    public enum MenuState
    {
        MainMenu,
        SettingsMenu,
        PauseMenu,
        GameplayUI,
        GameOver
    }

    /// <summary>
    /// Example: UI Manager sử dụng State Machine
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject gameplayPanel;
        [SerializeField] private GameObject gameOverPanel;

        private StateMachine<MenuState> _stateMachine;

        private void Awake()
        {
            _stateMachine = new StateMachine<MenuState>();
            _stateMachine.OnStateChanged += OnMenuStateChanged;

            // Start at main menu
            _stateMachine.ChangeState(MenuState.MainMenu, new MainMenuState(this));
        }

        private void Update()
        {
            _stateMachine.Update();

            // ESC to pause during gameplay
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_stateMachine.IsInState(MenuState.GameplayUI))
                {
                    ShowPauseMenu();
                }
                else if (_stateMachine.IsInState(MenuState.PauseMenu))
                {
                    ResumeGame();
                }
            }
        }

        private void OnMenuStateChanged(MenuState oldState, MenuState newState)
        {
            Debug.Log($"[UI] Menu: {oldState} → {newState}");
        }

        // Public methods to change states
        public void ShowMainMenu() => _stateMachine.ChangeState(MenuState.MainMenu, new MainMenuState(this));
        public void ShowSettings() => _stateMachine.ChangeState(MenuState.SettingsMenu, new SettingsMenuState(this));
        public void ShowPauseMenu() => _stateMachine.ChangeState(MenuState.PauseMenu, new PauseMenuState(this));
        public void ShowGameplay() => _stateMachine.ChangeState(MenuState.GameplayUI, new GameplayUIState(this));
        public void ShowGameOver() => _stateMachine.ChangeState(MenuState.GameOver, new GameOverState(this));
        
        public void ResumeGame() => ShowGameplay();
        public void QuitGame() => Application.Quit();

        // Public accessors for panels
        public GameObject MainMenuPanel => mainMenuPanel;
        public GameObject SettingsPanel => settingsPanel;
        public GameObject PausePanel => pausePanel;
        public GameObject GameplayPanel => gameplayPanel;
        public GameObject GameOverPanel => gameOverPanel;
    }

    // --- UI States ---

    public class MainMenuState : BaseState<UIManager>
    {
        public MainMenuState(UIManager context) : base(context) { }

        public override void OnEnter()
        {
            Debug.Log("[UI] → Main Menu");
            ShowPanel(Context.MainMenuPanel);
            Time.timeScale = 1f;
        }

        public override void OnExit()
        {
            HidePanel(Context.MainMenuPanel);
        }

        private void ShowPanel(GameObject panel)
        {
            if (panel != null) panel.SetActive(true);
        }

        private void HidePanel(GameObject panel)
        {
            if (panel != null) panel.SetActive(false);
        }
    }

    public class SettingsMenuState : BaseState<UIManager>
    {
        public SettingsMenuState(UIManager context) : base(context) { }

        public override void OnEnter()
        {
            Debug.Log("[UI] → Settings Menu");
            if (Context.SettingsPanel != null)
                Context.SettingsPanel.SetActive(true);
        }

        public override void OnExit()
        {
            if (Context.SettingsPanel != null)
                Context.SettingsPanel.SetActive(false);
        }
    }

    public class PauseMenuState : BaseState<UIManager>
    {
        public PauseMenuState(UIManager context) : base(context) { }

        public override void OnEnter()
        {
            Debug.Log("[UI] → Pause Menu");
            if (Context.PausePanel != null)
                Context.PausePanel.SetActive(true);
            
            // Pause game
            Time.timeScale = 0f;
        }

        public override void OnExit()
        {
            if (Context.PausePanel != null)
                Context.PausePanel.SetActive(false);
            
            // Resume game
            Time.timeScale = 1f;
        }
    }

    public class GameplayUIState : BaseState<UIManager>
    {
        public GameplayUIState(UIManager context) : base(context) { }

        public override void OnEnter()
        {
            Debug.Log("[UI] → Gameplay UI");
            if (Context.GameplayPanel != null)
                Context.GameplayPanel.SetActive(true);
            
            Time.timeScale = 1f;
        }

        public override void OnExit()
        {
            if (Context.GameplayPanel != null)
                Context.GameplayPanel.SetActive(false);
        }

        public override void OnUpdate()
        {
            // Update gameplay UI (health, score, etc.)
        }
    }

    public class GameOverState : BaseState<UIManager>
    {
        public GameOverState(UIManager context) : base(context) { }

        public override void OnEnter()
        {
            Debug.Log("[UI] → Game Over");
            if (Context.GameOverPanel != null)
                Context.GameOverPanel.SetActive(true);
            
            Time.timeScale = 0f;
        }

        public override void OnExit()
        {
            if (Context.GameOverPanel != null)
                Context.GameOverPanel.SetActive(false);
        }
    }
}
