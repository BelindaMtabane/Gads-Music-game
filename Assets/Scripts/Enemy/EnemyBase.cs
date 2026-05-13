using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    //Declare variables
    /*
    public Transform player;
    private float speed = 2f;
    private float maxSpeed = 10f;
    private float speedIncreaseRate = 0.1f;
    public float chaseDistance = 20f;*/
    //Player reference
    public Transform player;

    //Starting speed
    public float moveSpeed = 10f;

    //Boosted speed
    public float boostedSpeed = 30f;

    //Time before speed boost
    public float speedBoostTime = 10f;


    //How long boost lasts
    public float boostDuration = 10f;

    //Timer
    private float timer;

    //Check if boosted
    private bool hasBoosted = false;
    //Pickup base reference
    private PickupBase pickupBase;
    public float followDistance = 1f;

    void Start()
    {
        //Find the player automatically
        player = GameObject.FindGameObjectWithTag("Player").transform;
        //Find the pickup base script
        pickupBase = GameObject.FindGameObjectWithTag("Player").GetComponent<PickupBase>();

    }

    void Update()
    {
        //Increase timer
        timer += Time.deltaTime;
        //Boost ONLY ONCE
        if (timer >= speedBoostTime && !hasBoosted)
        {
            moveSpeed = boostedSpeed;

            Debug.Log("Enemy speed boosted to: " + moveSpeed);

            hasBoosted = true;
        }
        //Return to normal speed
        if (timer >= boostDuration && hasBoosted)
        {
            moveSpeed = 10f;

            hasBoosted = false;

            timer = 0f;

            Debug.Log("Enemy Back To Normal Speed!");
        }
        Vector3 targetPosition = new Vector3(
    player.position.x,
    transform.position.y,
    player.position.z - followDistance
);

        //Move toward player
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Handle player collision (e.g., reduce health, trigger game over, etc.)
            Debug.Log("Enemy collided with player!");
            //Reload scene again
        }
    }
}