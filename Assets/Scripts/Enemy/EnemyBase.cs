using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    //Declare variables
    public Transform player;
    private float speed = 15f;
    private float boostedSpeed = 40f;
    private float speedBoostTime = 10f;
    public float boostDuration = 10f;
    public float timer;
    private PickupBase pickupBase;
    private PlayerMovement playerMovement;
    public float followDistance = 1f;
    private bool hasBoosted = false;

    void Start()
    {
        //Assign scripts to be used in the enemy base script
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerMovement = player.GetComponent<PlayerMovement>();
        pickupBase = player.GetComponent<PickupBase>();
        pickupBase.currentHealth = 100;
        //Check if the scripts are attached 
        if (pickupBase == null)
        {
            Debug.LogError("PickupBase script not found on Player!");
        }

        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement script not found on Player!");
        }

        if (player == null)
        {
            Debug.LogError("Player not found!");
        }
    }

    void Update()
    {
        //Increment the timer
        timer += Time.deltaTime;

        if (pickupBase != null && playerMovement != null)
        {
            //Check if the timer reached the boost time and if the boost has not been applied yet
            if (pickupBase.isSneaking || playerMovement.forwardSpeed == 20f || pickupBase.artifactAmount == 10f)
            {
                if (timer >= speedBoostTime && !hasBoosted)
                {
                    speed = boostedSpeed;

                    Debug.Log("Security speed boosted to: " + speed);

                    hasBoosted = true;
                }
            }
        }
        //Return back to normal speed
        if (timer >= boostDuration && hasBoosted)
        {
            speed = 10f;

            hasBoosted = false;

            timer = 0f;

            Debug.Log("Security is in Normal Speed!");
        }
        //Calculate the target position to follow the player 
        Vector3 targetPosition = new Vector3(player.position.x, transform.position.y, player.position.z - followDistance);

        //Move toward player
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (pickupBase.hitCounter <= 0)
            {
                Debug.Log("Player has been hit 3 times and is now dead!");
                //Reset the counter
                pickupBase.hitCounter = 0;
                pickupBase.Death();
            }
            else
            {
                Debug.Log("Security hit the player! Hit count: " + (pickupBase.hitCounter + 1));
                //Decrease counter
                pickupBase.hitCounter -= 1;
                
                //Decrease the player's health by 20
                pickupBase.currentHealth -= 20;
                //Go to normal speed after hitting the player
                timer = 10f;
                hasBoosted = true;
            }
        }
    }
}