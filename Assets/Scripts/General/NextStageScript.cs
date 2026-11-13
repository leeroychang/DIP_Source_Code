using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class NextStageScript : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag=="Player") UIManager.Instance.NextLevel();
    }
}
