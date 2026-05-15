using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Drives the Start / Intro scene.
/// Flow:
///   1. Scene fades in → narration plays automatically (typewriter)
///   2. Player can click "Next" to advance each line
///   3. After all narration lines → PLAY button appears
///   4. PLAY button → fades to MainGameL1
///
/// Assign all references in the Inspector, or run
/// Tools → Setup Narration Scenes to auto-create the UI.
/// </summary>
public class StartSceneUI : MonoBehaviour
{
    [Header("Scene to load")]
    public string gameSceneName = "MainGameL1";

    [Header("Buttons")]
    public Button playButton;
    public Button nextButton;

    [Header("Narration lines (edit text here)")]
    [TextArea(2,5)]
    public string[] narrationLines = new string[]
    {
        "Welcome to Gads Music Game!",
        "You are a musician on the run. Collect all the instruments before the security guard catches you!",
        "Use the arrow keys or WASD to move. Press Space to jump.",
        "Avoid obstacles, collect power-ups, and reach the goal. Good luck!"
    };

    public string speakerName = "Narrator";

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    private void Start()
    {
        // Hide play button until narration finishes
        if (playButton != null) playButton.gameObject.SetActive(false);
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNext);
            nextButton.gameObject.SetActive(false);
        }

        // Build NarrationLine array from the plain-text array
        var lines = BuildLines();

        // Start narration; show play button when done
        if (NarrationManager.Instance != null)
        {
            NarrationManager.Instance.Play(lines, OnNarrationComplete);
            if (nextButton != null) nextButton.gameObject.SetActive(true);
        }
        else
        {
            // No NarrationManager in scene — skip straight to play button
            OnNarrationComplete();
        }
    }

    // ── Button callbacks ──────────────────────────────────────────────────────

    public void OnNext()
    {
        NarrationManager.Instance?.Advance();
    }

    public void OnPlay()
    {
        SceneFader.LoadScene(gameSceneName);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void OnNarrationComplete()
    {
        if (nextButton != null) nextButton.gameObject.SetActive(false);
        if (playButton != null)
        {
            playButton.gameObject.SetActive(true);
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(OnPlay);
        }
    }

    private NarrationLine[] BuildLines()
    {
        var result = new NarrationLine[narrationLines.Length];
        for (int i = 0; i < narrationLines.Length; i++)
            result[i] = new NarrationLine(narrationLines[i], speakerName);
        return result;
    }
}
