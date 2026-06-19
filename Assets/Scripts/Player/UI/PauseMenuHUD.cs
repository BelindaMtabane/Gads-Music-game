using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Full pause menu with resume, settings, and main menu actions.
/// Used in MainGameL1 (Escape) and StartScene (menu PAUSE button).
/// </summary>
public class PauseMenuHUD : MonoBehaviour
{
    [Header("Pause panel")]
    public GameObject pausePanel;
    public Button     resumeButton;
    public Button     settingsButton;
    public Button     mainMenuButton;

    [Header("Settings overlay")]
    public GameObject settingsPanel;
    public Button     settingsCloseButton;
    public Slider     musicVolumeSlider;
    public Slider     sfxVolumeSlider;

    [Header("Behaviour")]
    public string mainMenuSceneName = "StartScene";
    public bool pauseTimeOnOpen     = true;
    public bool listenForEscape     = true;
    public bool requireGameStarted  = false;
    public bool showMainMenuButton  = true;

    private bool _isOpen;

    public bool IsOpen => _isOpen;

    private void Start()
    {
        if (pausePanel != null)    pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        WireButton(resumeButton,        OnResume);
        WireButton(settingsButton,      OnOpenSettings);
        WireButton(mainMenuButton,      OnMainMenu);
        WireButton(settingsCloseButton, OnCloseSettings);

        if (mainMenuButton != null)
            mainMenuButton.gameObject.SetActive(showMainMenuButton);

        BindVolumeSliders();
        UIInputFix.EnsureEventSystem();
    }

    private void OnDisable()
    {
        if (_isOpen && pauseTimeOnOpen)
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }
    }

    private void Update()
    {
        if (!listenForEscape || !Input.GetKeyDown(KeyCode.Escape))
            return;

        if (requireGameStarted && !GameManager.GameStarted)
            return;

        if (settingsPanel != null && settingsPanel.activeSelf)
        {
            OnCloseSettings();
            return;
        }

        if (_isOpen)
            Hide();
        else if (!requireGameStarted || GameManager.GameStarted)
            Show();
    }

    public void Show()
    {
        if (pausePanel == null) return;

        _isOpen = true;
        UICursor.UnlockForMenu();
        UIInputFix.EnsureEventSystem();

        if (pausePanel != null)
            GameplayCanvasGuard.EnsureCanvasScale(pausePanel);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (pauseTimeOnOpen)
        {
            Time.timeScale = 0f;
            AudioListener.pause = true;
        }

        pausePanel.SetActive(true);
        pausePanel.transform.SetAsLastSibling();
    }

    public void Hide()
    {
        _isOpen = false;

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (pauseTimeOnOpen)
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }
    }

    public void Toggle()
    {
        if (_isOpen) Hide();
        else         Show();
    }

    public void OnResume()
    {
        Hide();
    }

    public void OnOpenSettings()
    {
        if (settingsPanel == null) return;

        UIInputFix.EnsureEventSystem();
        settingsPanel.SetActive(true);
        settingsPanel.transform.SetAsLastSibling();
    }

    public void OnCloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void OnMainMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        StartSceneUI.OpenDirectlyToMenu = true;
        SceneFader.LoadScene(mainMenuSceneName);
    }

    private void BindVolumeSliders()
    {
        if (AudioManager.Instance == null) return;

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.musicVolume);
            musicVolumeSlider.onValueChanged.AddListener(v => AudioManager.Instance.SetMusicVolume(v));
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.sfxVolume);
            sfxVolumeSlider.onValueChanged.AddListener(v => AudioManager.Instance.SetSfxVolume(v));
        }
    }

    private static void WireButton(Button btn, UnityEngine.Events.UnityAction action)
    {
        if (btn == null || action == null) return;
        UIButtonRaycastFix.Apply(btn);
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(action);
    }
}
