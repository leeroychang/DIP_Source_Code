using UnityEngine;

public class TargetTransition : MonoBehaviour
{
    [Header("Target Location")]
    public Transform targetSpawnPoint; // Drag 'Room_B_SpawnPoint' here in the inspector

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Teleport the player directly to the spawn position and rotation
            other.transform.position = targetSpawnPoint.position;
        }
    }
}
