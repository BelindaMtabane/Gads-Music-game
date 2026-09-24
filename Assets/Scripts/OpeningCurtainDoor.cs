using UnityEngine;

/// <summary>
/// Opens the opera-hall curtain pair so the track beyond it is visible.
/// </summary>
public class OpeningCurtainDoor : MonoBehaviour
{
    public Transform left;
    public Transform right;
    public GameObject backing;
    public float openSeconds = 1.1f;

    Vector3 _leftStart;
    Vector3 _rightStart;
    bool _opening;
    float _t;

    public void Setup(Transform leftCurtain, Transform rightCurtain, GameObject wall)
    {
        left = leftCurtain;
        right = rightCurtain;
        backing = wall;
        if (left != null) _leftStart = left.position;
        if (right != null) _rightStart = right.position;
    }

    void Update()
    {
        if (_opening && _t >= 1f)
            return;
        if (!_opening)
        {
            if (!GameManager.GameStarted)
                return;
            _opening = true;
            if (left != null) _leftStart = left.position;
            if (right != null) _rightStart = right.position;
        }

        _t = Mathf.MoveTowards(_t, 1f, Time.deltaTime / openSeconds);
        float eased = Mathf.SmoothStep(0f, 1f, _t);
        if (left != null)
            left.position = _leftStart + Vector3.left * (7.5f * eased);
        if (right != null)
            right.position = _rightStart + Vector3.right * (7.5f * eased);
        if (backing != null && eased > 0.35f)
        {
            var wallRenderer = backing.GetComponent<Renderer>();
            if (wallRenderer != null)
                wallRenderer.enabled = false;
        }
    }
}
