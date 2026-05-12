using UnityEngine;

public class ObstacleDown : MonoBehaviour
{
    //Declare variables for the obstacle
    PlayerMovement playerMovement;

    //Time variable
    public float timer;

    //Slow down variable
    private bool slowDownActive = false;

    //This class is for obstacles
    private void Start()
    {
        //Find the player movement script
        playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
    }
    private void Update()
    {
        //Caalculate the time
        if (slowDownActive)
        {
            timer += Time.deltaTime;
            //Reset after 5 seconds
            if (timer >= 5f)
            {
                playerMovement.forwardSpeed = 10f;
                playerMovement.sidewaySpeed = 10f;
                //Reset timer
                timer = 0f;
                //Deactivate slow down
                slowDownActive = false;
                Debug.Log("Slow motion has worn off!");
            }
        }

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SlowDown"))
        {
            Slowdown();
            Debug.Log("Player hit an obstacle and is slowed down!");
            Destroy(other.gameObject);
        }
    }

    void Slowdown()
    {
        //Change the players speed to be slower
        playerMovement.forwardSpeed = 5f;
        playerMovement.sidewaySpeed = 2f;
        //Activate slow down
        slowDownActive = true;
        Debug.Log("obstacle slow timer started: " + timer);
        //Reset the timer
        timer = 0f;
    }
}
