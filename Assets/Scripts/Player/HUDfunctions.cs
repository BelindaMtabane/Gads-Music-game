using UnityEngine;

public class HUDfunctions : MonoBehaviour
{
    //Declare variables - text variables
    public TMPro.TextMeshProUGUI healthText;
    public TMPro.TextMeshProUGUI artifactText;
    public TMPro.TextMeshProUGUI sneakText;
    public TMPro.TextMeshProUGUI hitText;

    //Declare variables - pickup base
    PickupBase pickupBase;

    private void Start()
    {
        //Find the pickup base script
        pickupBase = GameObject.FindGameObjectWithTag("Player").GetComponent<PickupBase>();
    }
    private void Update()
    {
        //Update the text on the HUD
        healthText.text = "Health: " + pickupBase.currentHealth;
        artifactText.text = "Artifacts: " + pickupBase.artifactAmount;
        hitText.text = "Hits Left: " + pickupBase.hitCounter;

        if (pickupBase.isSneaking == true)
        {
            sneakText.text = "Sneaking ON";
        }
        else if (pickupBase.isSneaking == false)
        {
            sneakText.text = "Sneaking OFF";
        }
    }

}
