using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Ensures menu scenes use InputSystemUIInputModule so UI buttons receive clicks.
/// </summary>
public static class UIInputFix
{
    public static void EnsureEventSystem()
    {
        var es = EventSystem.current;
        if (es == null)
        {
            var go = new GameObject("EventSystem");
            es = go.AddComponent<EventSystem>();
        }

#if ENABLE_INPUT_SYSTEM
        var legacy = es.GetComponent<StandaloneInputModule>();
        if (legacy != null)
            Object.Destroy(legacy);

        if (es.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() == null)
            es.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
        if (es.GetComponent<StandaloneInputModule>() == null)
            es.gameObject.AddComponent<StandaloneInputModule>();
#endif
    }
}
