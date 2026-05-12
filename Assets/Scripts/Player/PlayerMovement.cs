using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //Declare variables
    private CharacterController controller;
    public float sidewaySpeed = 10f;
    public float forwardSpeed = 10f;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;

    //Ground variables
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    private bool isGrounded;

    //Vector variable
    private float velocity;
    public LayerMask Ground;

    private void Start()
    {
        //Initialize character controller
        controller = GetComponent<CharacterController>();
    }
    void Update()
    {
        //Lock the cursor to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;

        //Check if player is grounded

        groundMask = Ground;
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity < 0)
        {
            velocity = -2f;
        }
        //Debug.Log(controller.isGrounded);
        //Initialize horixontal movement
        float horizontal = Input.GetAxis("Horizontal");

        //Create movement vector
        Vector3 move = new Vector3(horizontal * sidewaySpeed, 0, forwardSpeed);
        
        //Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        //Apply Gravity
        velocity += gravity * Time.deltaTime;
        //Initialize gravity to movement
        move.y = velocity;
        //Move the player
        controller.Move(move * Time.deltaTime);
    }
}