using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PauseController : MonoBehaviour
{
    [SerializeField] private GameObject pausePopup;
    [SerializeField] private GameObject firstButton;
    [SerializeField, Range(0f, 1f)] private float pausedVolume = 0.3f;

    public static bool IsPaused { get; private set; }

    private PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls();
        pausePopup.SetActive(false);
        IsPaused = false;
    }

    private void OnEnable()
    {
        controls.Player.Pause.performed += OnPausePressed;
        controls.Player.Pause.Enable();
    }

    private void OnDisable()
    {
        controls.Player.Pause.performed -= OnPausePressed;
        controls.Player.Pause.Disable();
        Time.timeScale = 1f;
        AudioListener.volume = 1f;
        IsPaused = false;
        SetPlayersEnabled(true);
    }

    private void OnDestroy()
    {
        controls?.Dispose();
    }

    private void OnPausePressed(InputAction.CallbackContext _) => SetPaused(!IsPaused);

    public void SetPaused(bool paused)
    {
        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
        pausePopup.SetActive(paused);
        AudioListener.volume = paused ? pausedVolume : 1f;
        SetPlayersEnabled(!paused);
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(paused ? firstButton : null);
    }

    void SetPlayersEnabled(bool enabled)
    {
        var players = FindObjectsByType<PlayerController>(FindObjectsInactive.Include);
        for (var i = 0; i < players.Length; i++)
            players[i].enabled = enabled;
    }

    public void Resume() => SetPaused(false);
    public void Restart() => Flow.Restart();
    public void ToMenu() => Flow.Go(Flow.MainMenuScene);
}