using UnityEngine;

public class PickupBase : MonoBehaviour
{
    //Declare variables for the obstacle
    PlayerMovement playerMovement;

    //Pickup variables
    private int healthIncreaseAmount = 20;
    private int healthDecreaseAmount = 10;
    public int currentHealth;
    public int maxHealth = 100;
    public int artifactAmount;

    //Time variable for boost
    private float timer;
    private bool jumpBoostActive = false;
    private bool speedBoostActive = false;



    //This class is for pickups
    private void Start()
    {
        //Find the player movement script
        playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();

        //Assign currnt health to max health
        currentHealth = maxHealth;
    }
    private void Update()
    {
        //Caalculate the time
        if (jumpBoostActive)
        {
            timer += Time.deltaTime;
            //Reset jump height after 5 seconds
            if (timer >= 5f)
            {
                playerMovement.jumpHeight = 3f;
                //Reset timer
                timer = 0f;
                //Deactivate jump boost
                jumpBoostActive = false;
                Debug.Log("Jump boost has worn off!");
            }
        }
        if (speedBoostActive)
        {
            timer += Time.deltaTime;
            //Reset after 5 seconds
            if (timer >= 5f)
            {
                playerMovement.forwardSpeed = 10f;
                playerMovement.sidewaySpeed = 10f;
                //Reset timer
                timer = 0f;
                //Deactivate boost
                speedBoostActive = false;
                Debug.Log("Speed boost has worn off!");
            }
        }

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("HealthINC"))
        {
            Debug.Log("Player hit a health pickup and is healed!");
            HealthIncrease();
            Destroy(other.gameObject);
        }
        if (other.CompareTag("HealthDEC"))
        {
            Debug.Log("Player hit a health pickup and is damaged!");
            HealthDecrease();
            Destroy(other.gameObject);
        }
        if (other.CompareTag("Artifact"))
        {
            Debug.Log("Player hit an artifac and has increased the amount!");
            Artifact();
            Destroy(other.gameObject);
        }
        if (other.CompareTag("JumpBoost"))
        {
            Debug.Log("Player hit a jump boost and increased their jump height!");
            JumpBoost();
            Destroy(other.gameObject);
        }
        if(other.CompareTag("Sneak"))
        {
            Debug.Log("Player hit a sneak pickup and is now invisible to obstacles for 5 seconds!");
            Destroy(other.gameObject);
        }
        if (other.CompareTag("Speed"))
        {
            Debug.Log("Player hit a speed boost and is now faster for 5 seconds!");
            SpeedBoost();
            Destroy(other.gameObject);
        }
    }

    void HealthIncrease()
    {
        //Change the players speed to be slower
        currentHealth += healthIncreaseAmount;
    }
    void HealthDecrease()
    {
        //Decrease player health
        currentHealth -= healthDecreaseAmount;
    }
    void Artifact()
    {
        //Increase artifact amount
        artifactAmount += 1;
    }
    void JumpBoost()
    {
        //Increase jump height
        playerMovement.jumpHeight = 6f;
        //Activate timer
        jumpBoostActive = true;
        Debug.Log("Pickup jump timer started: " + timer);
        //Reset timer
        timer = 0f;

    }
    void SpeedBoost()
    {
        //Increase player speed
        playerMovement.forwardSpeed = 20f;
        playerMovement.sidewaySpeed = 10f;
        //Activate timer
        speedBoostActive = true;
        Debug.Log("Pickup speed timer started: " + timer);
        //Reset timer
        timer = 0f;
    }
}
