using UnityEngine;

/// <summary>
/// Parts a track curtain so the lane ahead is visible and the player does not run through the cloth.
/// A curtain across the hall splits left and right. A side curtain draws offstage, away from the lanes.
/// </summary>
public class OpeningCurtain : MonoBehaviour
{
    public float openAheadDistance = 36f;
    public float openSeconds = 0.85f;

    Transform _left;
    Transform _right;
    Vector3 _leftClosed;
    Vector3 _rightClosed;
    Vector3 _leftOpen;
    Vector3 _rightOpen;
    bool _opening;
    bool _sealed;
    float _t;

    void Awake()
    {
        Seal();
    }

    public void Seal()
    {
        foreach (var col in GetComponents<Collider>())
            col.enabled = false;

        var slow = GetComponent<CurtainObstacle>();
        if (slow != null)
            slow.enabled = false;

        var source = GetComponent<MeshRenderer>();
        Material mat = source != null ? source.sharedMaterial : null;
        if (source != null)
            source.enabled = false;

        _left = transform.Find("CurtainLeft");
        _right = transform.Find("CurtainRight");
        if (_left == null || _right == null)
            CreateHalves(mat);

        if (_sealed)
            return;

        _leftClosed = _left.localPosition;
        _rightClosed = _right.localPosition;

        bool centered = Mathf.Abs(transform.position.x) < 2.5f;
        if (centered)
        {
            _leftOpen = _leftClosed + Vector3.left * 0.9f;
            _rightOpen = _rightClosed + Vector3.right * 0.9f;
        }
        else
        {
            float side = Mathf.Sign(transform.position.x);
            Vector3 offstage = Vector3.right * side * 1.05f;
            _leftOpen = _leftClosed + offstage;
            _rightOpen = _rightClosed + offstage;
        }

        _sealed = true;
    }

    void CreateHalves(Material mat)
    {
        if (mat == null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            mat = new Material(shader);
            mat.color = new Color(0.55f, 0.08f, 0.1f);
        }

        _left = CreateHalf("CurtainLeft", mat, -1);
        _right = CreateHalf("CurtainRight", mat, 1);
    }

    Transform CreateHalf(string halfName, Material mat, int side)
    {
        var half = GameObject.CreatePrimitive(PrimitiveType.Cube);
        half.name = halfName;
        half.transform.SetParent(transform, false);
        half.transform.localPosition = new Vector3(side * 0.25f, 0f, 0f);
        half.transform.localRotation = Quaternion.identity;
        half.transform.localScale = new Vector3(0.5f, 1f, 1f);
        var renderer = half.GetComponent<Renderer>();
        if (renderer != null)
            renderer.sharedMaterial = mat;
        var col = half.GetComponent<Collider>();
        if (col != null)
            Destroy(col);
        return half.transform;
    }

    void Update()
    {
        if (_left == null || _right == null || (_opening && _t >= 1f))
            return;

        if (!_opening && GameManager.GameStarted && PlayerIsClose())
            _opening = true;

        if (!_opening)
            return;

        _t = Mathf.MoveTowards(_t, 1f, Time.deltaTime / openSeconds);
        float eased = Mathf.SmoothStep(0f, 1f, _t);
        _left.localPosition = Vector3.Lerp(_leftClosed, _leftOpen, eased);
        _right.localPosition = Vector3.Lerp(_rightClosed, _rightOpen, eased);
    }

    bool PlayerIsClose()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return true;

        float ahead = transform.position.z - player.transform.position.z;
        return ahead < openAheadDistance && ahead > -4f;
    }

    public static void Ensure(GameObject curtain)
    {
        if (curtain == null || curtain.GetComponent<CurtainObstacle>() == null)
            return;

        var opener = curtain.GetComponent<OpeningCurtain>();
        if (opener == null)
            opener = curtain.AddComponent<OpeningCurtain>();
        opener.Seal();
    }

    public static void BindAll()
    {
        var curtains = Object.FindObjectsByType<CurtainObstacle>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < curtains.Length; i++)
            Ensure(curtains[i].gameObject);
    }
}
