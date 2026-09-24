using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    //Declare variables
    public Transform player;
    private float speed = 7f;
    private float baseSpeed = 7f;
    private float boostedSpeed = 12f;   // overridden by LevelBootstrap; 12 = manageable surge
    [Tooltip("When the player is slowed, guard speed = player speed + this margin.")]
    public float catchUpMargin = 1f;
    private PickupBase pickupBase;
    private PlayerMovement playerMovement;
    public float followDistance = 1f;
    public float groundOffset = 0.05f;
    public float groundRayHeight = 6f;
    public LayerMask groundMask;

    private bool  _surging      = false;
    private float _surgeTimer   = 0f;
    private float _surgeDuration = 0f;
    private bool  _caughtPlayer = false;
    private Collider _catchCollider;
    private float _pivotToFeet;

    void Start()
    {
        //Assign scripts to be used in the enemy base script
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO == null)
        {
            Debug.LogError("Player not found!");
            return;
        }
        player = playerGO.transform;
        playerMovement = player.GetComponent<PlayerMovement>();
        pickupBase = player.GetComponent<PickupBase>();

        if (pickupBase == null)
            Debug.LogError("PickupBase script not found on Player!");
        // currentHealth is owned by LevelBootstrap â€” do NOT overwrite it here

        if (playerMovement == null)
            Debug.LogError("PlayerMovement script not found on Player!");

        _catchCollider = GetComponent<Collider>();
        if (_catchCollider == null)
            _catchCollider = GetComponentInChildren<Collider>();
        if (_catchCollider != null)
            _catchCollider.isTrigger = true;

        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (groundMask.value == 0)
            groundMask = LayerMask.GetMask("Ground");

        _pivotToFeet = ComputePivotToFeet();
        SnapToGround(true);
    }

    void Update()
    {
        if (!GameManager.GameStarted)
            return;

        // Surge timer â€” returns to base speed when surge expires
        if (_surging)
        {
            _surgeTimer += Time.deltaTime;
            if (_surgeTimer >= _surgeDuration)
            {
                speed    = baseSpeed + _pianoBonus;
                _surging = false;
            }
        }

        //Calculate the target position to follow the player
        Vector3 targetPosition = new Vector3(player.position.x, transform.position.y, player.position.z - followDistance);

        float chaseSpeed = speed;

        //Move toward player
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, chaseSpeed * Time.deltaTime);
        SnapToGround(false);
    }

    void SnapToGround(bool instant)
    {
        LayerMask mask = groundMask.value != 0 ? groundMask : LayerMask.GetMask("Ground");

        Vector3 origin = transform.position + Vector3.up * groundRayHeight;
        float maxDist = groundRayHeight + _pivotToFeet + 6f;
        if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, maxDist, mask, QueryTriggerInteraction.Ignore))
            return;

        float targetY = hit.point.y + _pivotToFeet;
        if (instant || Mathf.Abs(transform.position.y - targetY) > 0.5f)
            transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
        else if (Mathf.Abs(transform.position.y - targetY) > 0.02f)
        {
            float smoothY = Mathf.Lerp(transform.position.y, targetY, Mathf.Min(1f, 15f * Time.deltaTime));
            transform.position = new Vector3(transform.position.x, smoothY, transform.position.z);
        }
    }

    float ComputePivotToFeet()
    {
        return GroundPlacementUtility.GetPivotToFeet(transform, groundOffset);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!PlayerColliderUtility.IsPlayer(other)) return;
        CatchPlayer();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!PlayerColliderUtility.IsPlayer(collision.collider)) return;
        CatchPlayer();
    }

    private void CatchPlayer()
    {
        if (_caughtPlayer || pickupBase == null) return;
        if (!GameManager.GameStarted) return;
        if (pickupBase.isSneaking) return;

        _caughtPlayer = true;
        pickupBase.KillPlayer(DeathCause.Guard);
    }

    public void ApplyLevelTuning(float normalSpeed, float boostSpeed)
    {
        baseSpeed    = normalSpeed;
        speed        = normalSpeed;
        boostedSpeed = boostSpeed;
        _pianoBonus  = 0f;
        _chaseStarted = false;
    }

    float _pianoBonus;
    bool _chaseStarted;

    // Wrong tiles and slows nudge the guard. The cap keeps that nudge small.
    const float PressureStep = 1f;
    const float PressureCap = 1.5f;

    public void AddPianoPressure()
    {
        _pianoBonus = Mathf.Min(PressureCap, _pianoBonus + PressureStep);
        if (!_surging)
            speed = baseSpeed + _pianoBonus;
    }

    public void RelievePianoPressure()
    {
        if (ChaseHasStarted())
            return;

        _pianoBonus = Mathf.Max(0f, _pianoBonus - PressureStep);
        if (!_surging)
            speed = baseSpeed + _pianoBonus;
    }

    bool ChaseHasStarted()
    {
        if (_chaseStarted)
            return true;
        if (player == null)
            return false;

        float behind = player.position.z - transform.position.z;
        if (behind <= 8f)
            _chaseStarted = true;
        return _chaseStarted;
    }

    public bool IsSurging     => _surging;
    public float SurgeTimeLeft => _surging ? Mathf.Max(0f, _surgeDuration - _surgeTimer) : 0f;

    /// <summary>
    /// Called when the player collects their first artifact.
    /// Guard surges to boostedSpeed for <duration> seconds to add tension.
    /// </summary>
    public void Surge(float duration)
    {
        if (_surging) return;   // already surging
        speed         = boostedSpeed;
        _surgeDuration = duration;
        _surgeTimer   = 0f;
        _surging      = true;
    }
}
