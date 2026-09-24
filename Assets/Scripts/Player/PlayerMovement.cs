using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    PickupBase _pickup;

    public float baseForwardSpeed = 8f;
    public float baseSidewaySpeed = 10f;
    public float sidewaySpeed = 10f;
    public float forwardSpeed = 8f;
    public float gravity = -20f;     // stronger gravity = snappier, less floaty jumps
    public float jumpHeight = 3f;
    public float footGroundOffset = 0.08f;

    //Ground variables
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    private bool isGrounded;

    //Vector variable
    private float velocity;
    public LayerMask Ground;

    private bool canJump = true;
    public bool DidJumpThisFrame { get; private set; }
    public bool IsDodging { get; private set; }

    public float dodgeDuration = 0.6f;
    public float dodgeCooldown = 0.85f;

    float _dodgeUntil;
    float _dodgeReadyAt;
    float _standHeight;
    Vector3 _standCenter;
    Transform _visual;
    Vector3 _visualScale;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        _pickup = GetComponent<PickupBase>();
        forwardSpeed = baseForwardSpeed;
        sidewaySpeed = baseSidewaySpeed;
        EnsureFootGroundCheck();
        if (groundMask.value == 0)
            groundMask = GroundPlacementUtility.DefaultGroundMask;

        GroundPlacementUtility.SnapTransformFeetToGround(
            transform, groundMask, footGroundOffset);

        _standHeight = controller.height;
        _standCenter = controller.center;
        var animator = GetComponentInChildren<Animator>();
        if (animator != null && animator.transform != transform)
        {
            _visual = animator.transform;
            _visualScale = _visual.localScale;
        }
    }

    /// <summary>
    /// Scene had groundCheck wired to the Ground platform — that made isGrounded always true.
    /// </summary>
    private void EnsureFootGroundCheck()
    {
        if (groundCheck != null && groundCheck.IsChildOf(transform))
            return;

        var existing = transform.Find("GroundCheck");
        if (existing != null)
        {
            groundCheck = existing;
            return;
        }

        var feet = new GameObject("GroundCheck");
        feet.transform.SetParent(transform, false);
        float footY = controller != null
            ? controller.center.y - controller.height * 0.5f + 0.05f
            : -0.95f;
        feet.transform.localPosition = new Vector3(0f, footY, 0f);
        groundCheck = feet.transform;
    }

    private void UpdateGrounded()
    {
        isGrounded = controller.isGrounded;

        if (groundCheck != null && groundCheck.IsChildOf(transform))
        {
            isGrounded |= Physics.CheckSphere(
                groundCheck.position,
                groundDistance,
                groundMask,
                QueryTriggerInteraction.Ignore);
        }
    }

    void Update()
    {
        DidJumpThisFrame = false;

        if (!enabled || !GameManager.GameStarted)
            return;

        // Skip cursor lock while paused — pause menu needs the cursor visible and free.
        if (Time.timeScale > 0f)
            UICursor.LockForGameplay();

        groundMask = Ground;
        UpdateGrounded();

        if (controller.isGrounded && velocity < 0f)
        {
            velocity = -2f;
            canJump = true;
        }

        float horizontal = Input.GetAxis("Horizontal");
        Vector3 move = new Vector3(horizontal * sidewaySpeed, 0, forwardSpeed);

        if (Input.GetKeyDown(KeyCode.G))
            TryDodge();

        if (IsDodging && Time.time >= _dodgeUntil)
            EndDodge();

        if (Input.GetButtonDown("Jump") && canJump && isGrounded && !IsDodging)
        {
            velocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            canJump = false;
            DidJumpThisFrame = true;
        }

        velocity += gravity * Time.deltaTime;
        move.y = velocity;
        controller.Move(move * Time.deltaTime);

        RecoverFromFall();
    }

    public void TryDodge()
    {
        if (!enabled || !GameManager.GameStarted || IsDodging)
            return;
        if (controller == null || Time.time < _dodgeReadyAt)
            return;

        UpdateGrounded();
        if (!isGrounded)
            return;

        IsDodging = true;
        _dodgeUntil = Time.time + dodgeDuration;
        _dodgeReadyAt = Time.time + dodgeCooldown;

        // Keep the feet planted. The ducked top stays high enough to hit lasers,
        // and low enough to pass under guitar strings.
        float bottom = _standCenter.y - _standHeight * 0.5f;
        const float duckedHeight = 1.28f;
        controller.height = duckedHeight;
        controller.center = new Vector3(_standCenter.x, bottom + duckedHeight * 0.5f, _standCenter.z);

        if (_visual != null)
            _visual.localScale = new Vector3(_visualScale.x, _visualScale.y * 0.62f, _visualScale.z);
    }

    void EndDodge()
    {
        IsDodging = false;
        if (controller != null)
        {
            controller.height = _standHeight;
            controller.center = _standCenter;
        }
        if (_visual != null)
            _visual.localScale = _visualScale;
    }

    void OnDisable()
    {
        if (IsDodging)
            EndDodge();
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!enabled || !GameManager.GameStarted || hit.collider == null) return;
        _pickup?.HandleSolidContact(hit.collider);
    }

    void RecoverFromFall()
    {
        UpdateGrounded();
        if (isGrounded) return;

        Vector3 origin = transform.position + Vector3.up * 2f;
        if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 40f, groundMask, QueryTriggerInteraction.Ignore))
            return;

        float footY = hit.point.y + (controller != null
            ? controller.height * 0.5f + controller.center.y + footGroundOffset
            : 1f);

        if (transform.position.y > footY + 0.35f)
            return;

        controller.enabled = false;
        transform.position = new Vector3(transform.position.x, footY, transform.position.z);
        controller.enabled = true;
        velocity = -2f;
    }
    // AlignFeetToGround removed — it fought CharacterController's native ground
    // detection each frame, causing the player to stutter and stop mid-run.
    // The CharacterController isGrounded + gravity=-20 handles ground contact cleanly.
}