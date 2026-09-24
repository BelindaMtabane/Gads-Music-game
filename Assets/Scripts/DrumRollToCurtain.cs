using UnityEngine;

/// <summary>
/// Rolls a drum obstacle down its lane toward the curtain that closes the hall.
/// </summary>
public class DrumRollToCurtain : MonoBehaviour
{
    public float speed = 3.2f;
    public float stopDistance = 2.2f;

    float _targetZ;
    bool _hasTarget;
    float _rolled;
    Quaternion _baseRotation;
    const float Radius = 0.55f;

    void Awake()
    {
        var animator = GetComponent<Animator>();
        if (animator != null)
            animator.enabled = false;

        var fix = GetComponent<DrumGroundFix>();
        if (fix != null)
            fix.enabled = false;

        foreach (var childAnimator in GetComponentsInChildren<Animator>(true))
            childAnimator.enabled = false;

        _baseRotation = transform.rotation;
    }

    void Start()
    {
        _hasTarget = TryFindCurtainZ(out _targetZ);
    }

    void Update()
    {
        if (!_hasTarget || !GameManager.GameStarted)
            return;

        Vector3 pos = transform.position;
        float remaining = _targetZ - pos.z;
        if (Mathf.Abs(remaining) <= stopDistance)
            return;

        float step = Mathf.Sign(remaining) * speed * Time.deltaTime;
        if (Mathf.Abs(step) > Mathf.Abs(remaining))
            step = remaining;

        pos.z += step;
        transform.position = pos;

        float degrees = Mathf.Abs(step) / Radius * Mathf.Rad2Deg;
        _rolled += Mathf.Sign(step) * degrees;
        transform.rotation = _baseRotation * Quaternion.AngleAxis(_rolled, Vector3.right);
    }

    static bool TryFindCurtainZ(out float targetZ)
    {
        var exit = GameObject.Find("OperaExitWall");
        if (exit != null)
        {
            targetZ = exit.transform.position.z;
            return true;
        }

        var club = GameObject.Find("ClubEndWall");
        if (club != null)
        {
            targetZ = club.transform.position.z;
            return true;
        }

        float best = float.PositiveInfinity;
        bool found = false;
        var curtains = Object.FindObjectsByType<CurtainObstacle>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < curtains.Length; i++)
        {
            float z = curtains[i].transform.position.z;
            if (z < best)
            {
                best = z;
                found = true;
            }
        }

        targetZ = found ? best : 24f;
        return true;
    }

    public static void Attach(GameObject go)
    {
        if (go == null || !ContainsDrum(go))
            return;

        Transform host = go.transform;
        while (host.parent != null && !IsContainer(host.parent))
            host = host.parent;

        if (host.GetComponent<PlayerMovement>() != null)
            return;
        if (host.GetComponent<DrumRollToCurtain>() == null)
            host.gameObject.AddComponent<DrumRollToCurtain>();
    }

    public static void BindLoaded()
    {
        var transforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i].name.IndexOf("Drum", System.StringComparison.OrdinalIgnoreCase) < 0)
                continue;
            Attach(transforms[i].gameObject);
        }
    }

    static bool ContainsDrum(GameObject go)
    {
        var transforms = go.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i].name.IndexOf("Drum", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
        }
        return false;
    }

    static bool IsContainer(Transform t)
    {
        if (t.GetComponent<Spawner>() != null) return true;
        if (t.GetComponent<GroundSegmentLifetime>() != null) return true;
        if (t.name == "spawnObjects") return true;
        if (t.name.IndexOf("Ground", System.StringComparison.OrdinalIgnoreCase) >= 0) return true;
        return false;
    }
}
