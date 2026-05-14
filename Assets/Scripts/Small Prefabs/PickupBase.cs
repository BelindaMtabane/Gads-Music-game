using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class PickupBase : MonoBehaviour
{
    //Declare variables for the pickups
    PlayerMovement playerMovement;

    //Pickup variables
    private int healthIncreaseAmount = 20;
    private int healthDecreaseAmount = 10;
    public int currentHealth;
    public int maxHealth = 100;
    public int artifactAmount;
    public bool isSneaking = false;

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
        if(isSneaking == true)
        {
            timer += Time.deltaTime;
            //Reset after 5 seconds
            if (timer >= 7f)
            {
                isSneaking = false;
                //Reset timer
                timer = 0f;
                Debug.Log("Sneaking has worn off!");
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
            Sneak();
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
        //Check if the player health is already at max
        if (currentHealth >= maxHealth)
        {
            //Set the players health to max
            currentHealth = maxHealth;
        }
        else
        {
            //Increase the players health
            currentHealth += healthIncreaseAmount;
        }
    }
    public void HealthDecrease()
    {
        //Check if the player's health is already at 0
        if (currentHealth <= 0f)
        {
            Debug.Log("Player has died!");
            //Load the death scene
            Death();
            //Restart the level
            SceneManager.LoadScene("MainGameL1");
        }
        else
        {
            //Decrease the players health
            currentHealth -= healthDecreaseAmount;
        }
    }
    void Artifact()
    {
        //Increase artifact amount
        artifactAmount += 1;
        if (artifactAmount >= 10 && currentHealth >= 1)
        {
            Debug.Log("Player has collected all artifacts and wins!");
            //Load victory scene
            Victory();
        }
        else
        {
            //Increase artifact amount
            artifactAmount += 1;
        }
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
    void Sneak()
    {
        //Activate sneak
        isSneaking = true;
        //Activate timer
        Debug.Log("Pickup sneak timer started: " + timer);
        //Reset timer
        timer = 0f;
    }
    void Victory()
    {
        Debug.Log("Victory, player won!");
        //UnityEngine.SceneManagement.SceneManager.LoadScene("VictoryScene");
    }
    void Death()
    {
        Debug.Log("Defeat, player lost!");
        //UnityEngine.SceneManagement.SceneManager.LoadScene("DeathScene");
    }
}
