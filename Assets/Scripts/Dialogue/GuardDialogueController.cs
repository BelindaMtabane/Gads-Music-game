using UnityEngine;

/// <summary>
/// Triggers guard dialogue based on distance to Pulse.
/// </summary>
public class GuardDialogueController : MonoBehaviour
{
    public Transform player;
    public string speakerName = "Security Guard";

    [Header("Distance thresholds (m)")]
    public float suspiciousDistance = 18f;
    public float chaseDistance = 8f;

    float _nextPatrolTime;
    bool _chaseLinePlayed;
    bool _suspiciousPlayed;

    void Start()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        _nextPatrolTime = Time.time + 12f;
    }

    void Update()
    {
        if (!GameManager.GameStarted || player == null || AIDialogueService.Instance == null)
            return;

        float dist = Vector3.Distance(transform.position, player.position);
        var def = LevelCatalog.Get(LevelProgress.CurrentLevel);

        var ctx = new DialogueContext
        {
            speakerName = speakerName,
            levelName = def.displayName,
            artifactsCollected = GetArtifactCount(),
            artifactsRequired = def.artifactsToWin,
            guardDistance = dist
        };

        if (dist <= chaseDistance)
        {
            if (!_chaseLinePlayed)
            {
                ctx.eventType = DialogueEvent.GuardChase;
                ctx.detail = "close pursuit";
                AIDialogueService.Instance.Speak(ctx);
                _chaseLinePlayed = true;
                _suspiciousPlayed = true;
            }
            return;
        }

        if (dist <= suspiciousDistance && !_suspiciousPlayed)
        {
            ctx.eventType = DialogueEvent.GuardSuspicious;
            ctx.detail = "player spotted";
            AIDialogueService.Instance.Speak(ctx);
            _suspiciousPlayed = true;
            return;
        }

        if (dist > suspiciousDistance + 4f)
        {
            _suspiciousPlayed = false;
            _chaseLinePlayed = false;
        }

        if (Time.time >= _nextPatrolTime)
        {
            ctx.eventType = DialogueEvent.GuardPatrol;
            ctx.detail = "routine patrol";
            AIDialogueService.Instance.Speak(ctx);
            _nextPatrolTime = Time.time + Random.Range(14f, 22f);
        }
    }

    static int GetArtifactCount()
    {
        var pickup = Object.FindAnyObjectByType<PickupBase>();
        return pickup != null ? pickup.artifactAmount : 0;
    }
}
