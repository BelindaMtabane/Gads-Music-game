using UnityEngine;
using UnityEngine.SceneManagement;

public class ArtifactGoal : MonoBehaviour
{
    [Tooltip("Rotate the artifact for visual flair")]
    public float rotationSpeed = 60f;

    private void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PickupBase pickup = other.GetComponent<PickupBase>();
        if (pickup != null)
            pickup.artifactAmount++;

        SceneManager.LoadScene("VictoryScene");
    }
}
