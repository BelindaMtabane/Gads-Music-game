using UnityEngine;

/// <summary>
/// Ensures AIDialogueService exists for the whole game session.
/// Add to SplashScene or StartScene Canvas.
/// </summary>
public class AIDialogueBootstrap : MonoBehaviour
{
    [SerializeField] AIDialogueService prefab;

    void Awake()
    {
        if (AIDialogueService.Instance != null) return;

        if (prefab != null)
        {
            Instantiate(prefab);
            return;
        }

        var go = new GameObject("AIDialogueService");
        go.AddComponent<AIDialogueService>();
    }
}
