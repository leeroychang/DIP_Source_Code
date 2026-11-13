using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialExitScript : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag=="Player") UIManager.Instance.NextLevel();
    }
}
