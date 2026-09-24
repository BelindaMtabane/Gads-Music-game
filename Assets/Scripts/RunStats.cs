/// <summary>
/// Persists the last run's stats and death cause for DeathScene / VictoryScene.
/// </summary>
public enum DeathCause
{
    Unknown,
    Guard,
    Obstacle,
    Fall
}

public static class RunStats
{
    public static int        ArtifactsCollected { get; private set; }
    public static int        ArtifactMoney      { get; private set; }
    public static int        Vibe               { get; private set; }
    public static int        Score              { get; private set; }
    public static float      RunTime            { get; private set; }  // seconds
    public static DeathCause LastDeathCause     { get; private set; } = DeathCause.Unknown;

    private static float _runStartTime = -1f;

    /// <summary>Call when gameplay actually begins (after countdown / cutscene).</summary>
    public static void MarkRunStart()
    {
        _runStartTime = UnityEngine.Time.time;
    }

    public static void SetDeathCause(DeathCause cause)
    {
        LastDeathCause = cause;
    }

    public static void SaveFrom(PickupBase pickup)
    {
        // Capture elapsed run time
        RunTime = _runStartTime >= 0f
            ? UnityEngine.Mathf.Max(0f, UnityEngine.Time.time - _runStartTime)
            : 0f;
        _runStartTime = -1f;   // reset so replays start fresh

        if (pickup == null)
        {
            ArtifactsCollected = 0;
            ArtifactMoney      = 0;
            Vibe               = 0;
            Score              = 0;
            return;
        }

        ArtifactsCollected = pickup.artifactAmount;
        ArtifactMoney      = pickup.artifactMoneyTotal;
        Vibe               = pickup.Vibe;
        Score              = pickup.RunScore;
    }

    public static string GetCauseHeadline()
    {
        return LastDeathCause switch
        {
            DeathCause.Guard    => "You were caught by the guard!",
            DeathCause.Obstacle => "You were stopped by an obstacle!",
            DeathCause.Fall     => "You fell off the track!",
            _                   => "Your run has ended."
        };
    }

    public static string[] GetCauseNarration()
    {
        return LastDeathCause switch
        {
            DeathCause.Guard => new[]
            {
                "You were caught by the security guard!",
                "Don't give up — the instruments are still out there.",
                "Try again and stay ahead of the guard!"
            },
            DeathCause.Obstacle => new[]
            {
                "A dangerous obstacle ended your run!",
                "Dodge hazards and grab health pickups when you can.",
                "Try again — watch the path ahead!"
            },
            DeathCause.Fall => new[]
            {
                "You fell off the track!",
                "Keep to the path and don't miss the ground.",
                "Try again — stay on your feet!"
            },
            _ => new[]
            {
                "Your run has ended.",
                "Don't give up — the instruments are still out there.",
                "Try again and make it to the goal!"
            }
        };
    }

    static string CollectibleName()
    {
        var def = LevelCatalog.Get(LevelProgress.CurrentLevel);
        return string.IsNullOrEmpty(def.collectibleName) ? "Artifacts" : def.collectibleName;
    }

    static string FormatTime(float seconds)
    {
        int m = (int)(seconds / 60);
        int s = (int)(seconds % 60);
        return m > 0 ? $"{m}m {s:D2}s" : $"{s}s";
    }

    public static string FormatScoreSummary()
    {
        string time = RunTime > 0f ? $"\nTime: {FormatTime(RunTime)}" : "";
        return $"Score: {Score:N0}\n" +
               $"Vibe: {Vibe:N0}\n" +
               $"{CollectibleName()}: {ArtifactsCollected} / {PickupBase.RequiredToWin}{time}";
    }

    public static string FormatRunSummaryLine()
    {
        string time = RunTime > 0f ? $"   |   {FormatTime(RunTime)}" : "";
        return $"Score: {Score:N0}   |   Vibe: {Vibe:N0}   |   {CollectibleName()}: {ArtifactsCollected} / {PickupBase.RequiredToWin}{time}";
    }
}
