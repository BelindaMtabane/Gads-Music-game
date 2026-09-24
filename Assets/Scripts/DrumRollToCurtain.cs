using UnityEngine;

/// <summary>
/// Rolls a drum forward along the track, in the same direction the player runs.
/// </summary>
public class DrumRollToCurtain : MonoBehaviour
{
    public float speed = 3.2f;
    public float stopDistance = 2.2f;

    float _targetZ;
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
        _targetZ = transform.position.z + 90f;
    }

    void Start()
    {
        float ahead = FindFinishZ();
        if (ahead > transform.position.z + 6f)
            _targetZ = ahead - stopDistance;
    }

    void Update()
    {
        if (!GameManager.GameStarted)
            return;

        float remaining = _targetZ - transform.position.z;
        if (remaining <= stopDistance)
            return;

        float step = Mathf.Min(speed * Time.deltaTime, remaining);
        Vector3 pos = transform.position;
        pos.z += step;
        transform.position = pos;

        _rolled += step / Radius * Mathf.Rad2Deg;
        transform.rotation = _baseRotation * Quaternion.AngleAxis(-_rolled, Vector3.right);
    }

    static float FindFinishZ()
    {
        float best = float.NegativeInfinity;
        string[] names = { "MuseumEndWall", "ClubEndWall", "OperaExitWall" };
        for (int i = 0; i < names.Length; i++)
        {
            var marker = GameObject.Find(names[i]);
            if (marker != null)
                best = Mathf.Max(best, marker.transform.position.z);
        }
        return best;
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
