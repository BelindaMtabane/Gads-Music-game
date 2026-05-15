using UnityEngine;

public class HealthDecreaseObstacle : MonoBehaviour
{
    [Tooltip("How much health to take away")]
    public int damage = 15;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PickupBase pickup = other.GetComponent<PickupBase>();
        if (pickup == null) return;

        pickup.currentHealth = Mathf.Max(0, pickup.currentHealth - damage);

        if (pickup.hitCounter > 0)
            pickup.hitCounter--;

        if (pickup.currentHealth <= 0 || pickup.hitCounter <= 0)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("DeathScene");
            return;
        }

        Destroy(gameObject);
    }
}
