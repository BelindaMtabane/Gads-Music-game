using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Drives the Death / Game Over scene.
/// Flow:
///   1. Scene fades in → "GAME OVER" title shown
///   2. Narration plays automatically
///   3. After narration → Retry and Main Menu buttons appear
/// </summary>
public class DeathHUD : MonoBehaviour
{
    [Header("Scene names")]
    public string gameSceneName     = "MainGameL1";
    public string mainMenuSceneName = "StartScene";

    [Header("UI References")]
    public TextMeshProUGUI titleText;
    public Button          retryButton;
    public Button          mainMenuButton;
    public Button          nextButton;

    [Header("Narration lines")]
    [TextArea(2, 5)]
    public string[] narrationLines = new string[]
    {
        "You were caught by the security guard!",
        "Don't give up — the instruments are still out there.",
        "Try again and make it to the goal!"
    };

    public string speakerName = "Narrator";

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    private void Start()
    {
        if (titleText    != null) titleText.text = "GAME OVER";
        if (retryButton  != null) retryButton.gameObject.SetActive(false);
        if (mainMenuButton != null) mainMenuButton.gameObject.SetActive(false);

        if (retryButton    != null) retryButton.onClick.AddListener(OnRetry);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenu);

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNext);
            nextButton.gameObject.SetActive(false);
        }

        var lines = BuildLines();
        if (NarrationManager.Instance != null)
        {
            NarrationManager.Instance.Play(lines, OnNarrationComplete);
            if (nextButton != null) nextButton.gameObject.SetActive(true);
        }
        else
        {
            OnNarrationComplete();
        }
    }

    // ── Callbacks ─────────────────────────────────────────────────────────────

    public void OnNext()      => NarrationManager.Instance?.Advance();
    public void OnRetry()     => SceneFader.LoadScene(gameSceneName);
    public void OnMainMenu()  => SceneFader.LoadScene(mainMenuSceneName);

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void OnNarrationComplete()
    {
        if (nextButton     != null) nextButton.gameObject.SetActive(false);
        if (retryButton    != null) retryButton.gameObject.SetActive(true);
        if (mainMenuButton != null) mainMenuButton.gameObject.SetActive(true);
    }

    private NarrationLine[] BuildLines()
    {
        var result = new NarrationLine[narrationLines.Length];
        for (int i = 0; i < narrationLines.Length; i++)
            result[i] = new NarrationLine(narrationLines[i], speakerName);
        return result;
    }
}
