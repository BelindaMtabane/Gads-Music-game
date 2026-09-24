using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class PickupBase : MonoBehaviour
{
    //Declare variables for the pickups
    PlayerMovement playerMovement;

    //Pickup variables
    private int healthIncreaseAmount = 20;
    private int healthDecreaseAmount = 10;
    public int currentHealth = 100;
    public int maxHealth = 100;
    public const int ArtifactsToWin = 2;

    public static int RequiredToWin
    {
        get
        {
            int level = LevelProgress.CurrentLevel;
            if (level <= 0)
                level = 1;
            return Mathf.Max(1, LevelCatalog.Get(level).artifactsToWin);
        }
    }

    public int artifactAmount;
    public int artifactMoneyTotal;
    public int moneyPerArtifact = 500;
    public bool isSneaking = false;
    public int hitCounter = 10;
    public int maxHeadphones = 10;

    // Vibe is how far this level's run has gone. It resets every level.
    // Good pickups add score points, and they are not added into vibe.
    const float VibePerMeter = 4f;
    float _vibeValue;
    int _bonusPoints;
    bool _runTracked;
    float _trackedZ;

    public int Vibe => Mathf.FloorToInt(_vibeValue);
    public int BonusPoints => _bonusPoints;
    public int RunScore => Vibe * 10 + _bonusPoints;

    private bool _deathHandled;
    float _drumBounceReady;
    readonly Dictionary<int, float> _solidHitTimes = new Dictionary<int, float>();
    const float SolidHitCooldown = 0.45f;

    //Independent timers for each boost (sharing one timer causes them to expire early when two are active)
    private float jumpBoostTimer = 0f;
    private float speedBoostTimer = 0f;
    private float sneakTimer = 0f;
    private bool jumpBoostActive = false;
    private bool speedBoostActive = false;

    // â”€â”€ Public boost state for HUD â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private const float BoostDuration = 5f;
    public bool  IsSpeedBoostActive  => speedBoostActive;
    public bool  IsJumpBoostActive   => jumpBoostActive;
    public float SpeedBoostTimeLeft  => speedBoostActive ? Mathf.Max(0f, BoostDuration - speedBoostTimer) : 0f;
    public float JumpBoostTimeLeft   => jumpBoostActive  ? Mathf.Max(0f, BoostDuration - jumpBoostTimer)  : 0f;
    public float SneakTimeLeft       => isSneaking       ? Mathf.Max(0f, BoostDuration - sneakTimer)      : 0f;
    //This class is for pickups
    private void Start()
    {
        //Find the player movement script
        playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        //Assign currnt health to max health
        currentHealth = maxHealth;
    }
    private void Update()
    {
        
        //Each boost has its own independent timer so two active boosts don't interfere
        if (jumpBoostActive)
        {
            jumpBoostTimer += Time.deltaTime;
            if (jumpBoostTimer >= 5f)
            {
                playerMovement.jumpHeight = 3f;
                jumpBoostTimer = 0f;
                jumpBoostActive = false;
            }
        }
        if (speedBoostActive)
        {
            speedBoostTimer += Time.deltaTime;
            if (speedBoostTimer >= 5f)
            {
                // A slow that started after the boost keeps its own speed.
                var slow = playerMovement.GetComponent<PianoMissSlow>();
                if (slow == null || !slow.IsActive)
                    playerMovement.forwardSpeed = playerMovement.baseForwardSpeed;
                playerMovement.sidewaySpeed = playerMovement.baseSidewaySpeed;
                speedBoostTimer = 0f;
                speedBoostActive = false;
            }
        }
        if (isSneaking)
        {
            sneakTimer += Time.deltaTime;
            if (sneakTimer >= 5f)   // 5 s â€” timing the sneak now matters
            {
                isSneaking = false;
                sneakTimer = 0f;
            }
        }

        TrackRunVibe();
    }

    void TrackRunVibe()
    {
        if (!GameManager.GameStarted || _deathHandled) return;

        float z = transform.position.z;
        if (!_runTracked)
        {
            _runTracked = true;
            _trackedZ = z;
            return;
        }

        float delta = z - _trackedZ;
        if (delta <= 0f) return;
        _trackedZ = z;
        _vibeValue += delta * VibePerMeter;
    }

    void GrantCorrectPickup(int points)
    {
        _bonusPoints += points;
    }
    // Called both by Unity (if the CapsuleCollider trigger fires directly) and by
    // PickupTriggerProxy on the child "PickupTrigger" object.  The CharacterController
    // suppresses trigger events on the same GameObject, so the proxy child is the
    // reliable path for all pickup / obstacle interactions.
    public void HandleTrigger(Collider other)
    {
        OnTriggerEnter(other);
    }

    public void HandleSolidContact(Collider other)
    {
        if (!GameManager.GameStarted || other == null) return;
        if (PickupCollectUtility.IsPickupConsumed(other)) return;

        int rootId = other.transform.root.GetInstanceID();
        if (_solidHitTimes.TryGetValue(rootId, out float lastHit)
            && Time.time - lastHit < SolidHitCooldown)
            return;

        var crowd = other.GetComponentInParent<DanceCrowd>();
        if (crowd != null)
        {
            _solidHitTimes[rootId] = Time.time;
            crowd.TryTakeArtifact(this);
            return;
        }

        if (HasTag(other, GameplayCollisionUtility.TagHealthDec))
        {
            _solidHitTimes[rootId] = Time.time;
            AudioManager.Instance?.PlayObstacleHitSfx();
            var obstacle = other.GetComponentInParent<HealthDecreaseObstacle>();
            if (obstacle != null)
                ApplyObstacleDamage(obstacle);
            else
                HealthDecrease();
            DestroyPickupRoot(other);
            return;
        }

        if (HasTag(other, GameplayCollisionUtility.TagSlowDown))
        {
            _solidHitTimes[rootId] = Time.time;
            var slow = other.GetComponentInParent<SlowDownObstacle>();
            if (slow != null)
                slow.ApplyToPlayer(playerMovement);
            else
                DestroyPickupRoot(other);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!GameManager.GameStarted) return;
        if (PickupCollectUtility.IsPickupConsumed(other)) return;

        if (HasTag(other, "HealthINC"))
        {
            AudioManager.Instance?.PlayHealthPickupSfx();
            HealthIncrease();
            GrantCorrectPickup(100);
            DestroyPickupRoot(other);
        }
        if (HasTag(other, "HealthDEC"))
        {
            if (IsRollingDrum(other))
            {
                BounceOffDrum();
                return;
            }

            AudioManager.Instance?.PlayObstacleHitSfx();
            var obstacle = other.GetComponentInParent<HealthDecreaseObstacle>();
            if (obstacle != null)
                ApplyObstacleDamage(obstacle);
            else
                HealthDecrease();
            DestroyPickupRoot(other);
        }
        // Guard catch is handled exclusively by EnemyBase.CatchPlayer() to avoid double-trigger.
        if (HasTag(other, "Artifact"))
        {
            if (other.GetComponentInParent<MusicInstrument>() != null)
                return;

            Artifact();
            DestroyPickupRoot(other);
        }
        if (HasTag(other, "JumpBoost"))
        {
            AudioManager.Instance?.PlayJumpPickupSfx();
            JumpBoost();
            GrantCorrectPickup(100);
            DestroyPickupRoot(other);
        }
        if (HasTag(other, "Sneak"))
        {
            AudioManager.Instance?.PlaySneakPickupSfx();
            AddHeadphone();
            DestroyPickupRoot(other);
        }
        if (HasTag(other, "Speed"))
        {
            AudioManager.Instance?.PlaySpeedPickupSfx();
            SpeedBoost();
            GrantCorrectPickup(150);
            DestroyPickupRoot(other);
        }
        if (HasTag(other, "SlowDown"))
        {
            var slow = other.GetComponentInParent<SlowDownObstacle>();
            if (slow != null)
                slow.ApplyToPlayer(playerMovement);
            else
                DestroyPickupRoot(other);
        }
    }

    static bool HasTag(Collider col, string tag)
    {
        var t = col.transform;
        while (t != null)
        {
            if (t.CompareTag(tag)) return true;
            t = t.parent;
        }
        return false;
    }

    static void DestroyPickupRoot(Collider col)
    {
        PickupCollectUtility.TryConsumeFromCollider(col);
    }

    void HealthIncrease()
    {
        // Always clamp to maxHealth so display never shows e.g. 105/100
        currentHealth = Mathf.Min(currentHealth + healthIncreaseAmount, maxHealth);
    }
    public void HealthDecrease()
    {
        ApplyDamage(healthDecreaseAmount);
    }

    public void ApplyObstacleDamage(HealthDecreaseObstacle obstacle)
    {
        if (obstacle == null) return;
        if (obstacle.instantKill)
        {
            KillPlayer(DeathCause.Obstacle);
            return;
        }

        if (obstacle.vibeDamage > 0)
        {
            currentHealth = Mathf.Max(0, currentHealth - obstacle.vibeDamage);
            if (currentHealth <= 0)
            {
                KillPlayer(DeathCause.Obstacle);
                return;
            }
        }

        ApplyDamage(obstacle.damage);
    }

    /// <summary>
    /// Damage absorption: Shield (hitCounter) absorbs hits first.
    /// Only when Shield reaches 0 does Vibe (currentHealth) take damage.
    /// </summary>
    void ApplyDamage(int amount)
    {
        if (hitCounter > 0)
        {
            hitCounter = Mathf.Max(0, hitCounter - 1);
            // Shield fully depleted this hit â€” spill leftover into Vibe
            if (hitCounter == 0)
            {
                int spillover = amount - 1;   // 1 point consumed the last shield
                if (spillover > 0)
                {
                    currentHealth = Mathf.Max(0, currentHealth - spillover);
                    if (currentHealth <= 0) KillPlayer(DeathCause.Obstacle);
                }
            }
        }
        else
        {
            // No shield left â€” Vibe takes full damage
            currentHealth = Mathf.Max(0, currentHealth - amount);
            if (currentHealth <= 0) KillPlayer(DeathCause.Obstacle);
        }
    }

    public void ResetForNewRun()
    {
        currentHealth      = maxHealth;   // maxHealth is already set by LevelBootstrap
        // hitCounter intentionally NOT reset here â€” LevelBootstrap.Awake() owns that value
        artifactAmount     = 0;
        artifactMoneyTotal = 0;
        _vibeValue         = 0f;
        _bonusPoints       = 0;
        _runTracked        = false;
        _deathHandled      = false;
    }

    /// <summary>Instant game over â€” defeat animation then DeathScene.</summary>
    public void KillPlayer(DeathCause cause = DeathCause.Unknown)
    {
        if (_deathHandled) return;
        _deathHandled = true;
        currentHealth = 0;
        hitCounter = 0;
        RunStats.SetDeathCause(cause);
        Death();
    }
    void Artifact()
    {
        CollectArtifact(1);
    }

    public void CollectArtifact(int count = 1, int money = -1)
    {
        if (money < 0)
            money = moneyPerArtifact * count;

        artifactAmount += count;
        artifactMoneyTotal += money;
        GrantCorrectPickup(money);
        AudioManager.Instance?.PlayArtifactPickupSfx();

        TriggerArtifactDialogue();

        TryTriggerVictory();
    }

    public bool LoseArtifact()
    {
        if (artifactAmount <= 0)
            return false;

        artifactAmount--;
        AudioManager.Instance?.PlayObstacleHitSfx();
        return true;
    }

    static bool IsRollingDrum(Collider other)
    {
        if (other == null)
            return false;
        if (other.GetComponentInParent<DrumRollToCurtain>() != null)
            return true;

        var t = other.transform;
        while (t != null)
        {
            if (t.name.IndexOf("Drum", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            t = t.parent;
        }
        return false;
    }

    void BounceOffDrum()
    {
        if (Time.time < _drumBounceReady)
            return;
        _drumBounceReady = Time.time + 0.8f;

        playerMovement?.BounceUp();

        if (hitCounter > 0)
        {
            hitCounter--;
            AudioManager.Instance?.PlayObstacleHitSfx();
        }
        else
            LoseArtifact();
    }

    void TriggerArtifactDialogue()
    {
        if (AIDialogueService.Instance == null) return;
        var def = LevelCatalog.Get(LevelProgress.CurrentLevel);
        AIDialogueService.Instance.Speak(new DialogueContext
        {
            speakerName = "Pulse",
            levelName = def.displayName,
            eventType = DialogueEvent.ArtifactCollected,
            detail = $"artifact {artifactAmount}",
            artifactsCollected = artifactAmount,
            artifactsRequired = def.artifactsToWin
        });
    }

    /// <summary>Returns true and loads victory if the player has collected enough artifacts.</summary>
    public bool TryTriggerVictory()
    {
        if (artifactAmount < RequiredToWin || currentHealth < 1)
            return false;

        Victory();
        return true;
    }
    void JumpBoost()
    {
        playerMovement.jumpHeight = 6f;
        jumpBoostTimer = 0f;
        jumpBoostActive = true;
    }
    void SpeedBoost()
    {
        // Fast wins. Cancel so the slow timer doesn't write over this speed.
        playerMovement.GetComponent<PianoMissSlow>()?.Cancel();
        playerMovement.forwardSpeed = playerMovement.baseForwardSpeed + 4f;
        playerMovement.sidewaySpeed = playerMovement.baseSidewaySpeed + 1.5f;
        speedBoostTimer = 0f;
        speedBoostActive = true;
    }
    void AddHeadphone()
    {
        if (hitCounter < maxHeadphones)
            hitCounter++;
        GrantCorrectPickup(80);
        Sneak();
    }

    void Sneak()
    {
        isSneaking = true;
        sneakTimer = 0f;
    }
    public void Victory()
    {
        // Use GameManager so the victory sequence (fade, etc.) runs cleanly
        if (GameManager.Instance != null)
            GameManager.Instance.TriggerVictory();
        else
            SceneFader.LoadScene("VictoryScene");
    }
    public void Death()
    {
        RunStats.SaveFrom(this);

        if (GameManager.Instance != null)
            GameManager.Instance.TriggerGameOver();
        else
        {
            RunStats.SaveFrom(this);
            GameOverOverlay.Show("GAME OVER\n\n" + RunStats.GetCauseHeadline());
            SceneFader.LoadScene("DeathScene");
        }
    }
}
