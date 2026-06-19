using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// LLM-backed dialogue for Rhythm Raiders (Beat Horizon).
/// Default: rich scripted lines (no install). Optional: Groq or any OpenAI-compatible API
/// (same wire format as Ollama /v1/chat/completions) — lighter than running Ollama locally.
/// </summary>
public class AIDialogueService : MonoBehaviour
{
    public enum ProviderMode
    {
        Scripted,
        Groq,
        OllamaCompatible
    }

    public static AIDialogueService Instance { get; private set; }

    [Header("Provider")]
    [Tooltip("OllamaCompatible = LM Studio at localhost:1234 (OpenAI-compatible). Groq = cloud free tier. Scripted = offline fallback.")]
    public ProviderMode provider = ProviderMode.Scripted;
    [Tooltip("Groq API key (console.groq.com) — only needed when Provider = Groq.")]
    public string apiKey = "";
    [Tooltip("Model name served by LM Studio. Must match the model you loaded in LM Studio (e.g. 'phi-3-mini').")]
    public string model = "phi-3-mini";
    public string groqUrl = "https://api.groq.com/openai/v1/chat/completions";
    [Tooltip("LM Studio default: http://localhost:1234/v1/chat/completions")]
    public string ollamaUrl = "http://localhost:1234/v1/chat/completions";
    public float requestTimeout = 8f;

    [Header("Behaviour")]
    public float minSecondsBetweenLines = 4f;

    float _lastLineTime = -99f;
    Coroutine _activeRequest;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Speak(DialogueContext context, Action<string> onLine = null)
    {
        if (Time.unscaledTime - _lastLineTime < minSecondsBetweenLines)
            return;

        if (_activeRequest != null)
            StopCoroutine(_activeRequest);

        _activeRequest = StartCoroutine(RequestLine(context, line =>
        {
            _lastLineTime = Time.unscaledTime;
            _activeRequest = null;
            if (string.IsNullOrWhiteSpace(line)) return;
            ShowSubtitle(line, context.speakerName);
            onLine?.Invoke(line);
        }));
    }

    IEnumerator RequestLine(DialogueContext context, Action<string> onDone)
    {
        if (provider == ProviderMode.Scripted || string.IsNullOrWhiteSpace(apiKey))
        {
            onDone(ScriptedDialogueBank.Pick(context));
            yield break;
        }

        string url = provider == ProviderMode.Groq ? groqUrl : ollamaUrl;
        string system = BuildSystemPrompt(context);
        string user = BuildUserPrompt(context);

        string json = BuildChatJson(model, system, user);
        using var req = new UnityWebRequest(url, "POST");
        byte[] body = Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        if (!string.IsNullOrEmpty(apiKey))
            req.SetRequestHeader("Authorization", "Bearer " + apiKey);
        req.timeout = Mathf.CeilToInt(requestTimeout);

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"[AIDialogue] LLM request failed: {req.error}. Using scripted line.");
            onDone(ScriptedDialogueBank.Pick(context));
            yield break;
        }

        string responseText = req.downloadHandler.text;
        string line = ParseChatResponse(responseText);
        onDone(string.IsNullOrWhiteSpace(line) ? ScriptedDialogueBank.Pick(context) : line);
    }

    static string BuildSystemPrompt(DialogueContext ctx)
    {
        return "You write ONE short in-game dialogue line (max 12 words) for Rhythm Raiders: Beat Horizon. " +
               "Teen rhythm thief Pulse steals musical artifacts from guards. Tone: tense, musical, arcade. " +
               "No quotes. No emojis. Speaker: " + ctx.speakerName + ".";
    }

    static string BuildUserPrompt(DialogueContext ctx)
    {
        return $"Level: {ctx.levelName}. Event: {ctx.eventType}. Detail: {ctx.detail}. " +
               $"Artifacts: {ctx.artifactsCollected}/{ctx.artifactsRequired}. Guard distance: {ctx.guardDistance:0}m.";
    }

    static string BuildChatJson(string modelName, string system, string user)
    {
        string esc(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", " ");
        return "{\"model\":\"" + esc(modelName) + "\"," +
               "\"messages\":[" +
               "{\"role\":\"system\",\"content\":\"" + esc(system) + "\"}," +
               "{\"role\":\"user\",\"content\":\"" + esc(user) + "\"}" +
               "],\"max_tokens\":60,\"temperature\":0.85}";
    }

    static string ParseChatResponse(string json)
    {
        try
        {
            var wrapped = JsonUtility.FromJson<ChatResponseWrapper>(json);
            if (wrapped?.choices != null && wrapped.choices.Length > 0)
                return wrapped.choices[0].message?.content?.Trim();
        }
        catch { /* fall through */ }
        return null;
    }

    static void ShowSubtitle(string line, string speaker)
    {
        if (NarrationManager.Instance == null) return;

        if (GameManager.GameStarted && !GameManager.IsGameOver)
        {
            NarrationManager.Instance.ShowGameplaySubtitle(line, speaker, 2.2f);
            return;
        }

        var nm = NarrationManager.Instance;
        nm.autoAdvance = true;
        nm.autoDelay = 2.2f;
        nm.Play(new[] { new NarrationLine(line, speaker) });
    }

    [Serializable]
    class ChatResponseWrapper
    {
        public ChatChoice[] choices;
    }

    [Serializable]
    class ChatChoice
    {
        public ChatMessageContent message;
    }

    [Serializable]
    class ChatMessageContent
    {
        public string content;
    }
}

public struct DialogueContext
{
    public string speakerName;
    public string levelName;
    public DialogueEvent eventType;
    public string detail;
    public int artifactsCollected;
    public int artifactsRequired;
    public float guardDistance;
}

public enum DialogueEvent
{
    LevelStart,
    ArtifactCollected,
    GuardPatrol,
    GuardSuspicious,
    GuardChase,
    GuardAlerted,
    PlayerJump,
    Victory,
    Defeat,
    MidRun
}
