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

        if (Input.GetButtonDown("Jump") && canJump && isGrounded)
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