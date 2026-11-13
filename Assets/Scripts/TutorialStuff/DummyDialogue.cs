using UnityEngine;
using UnityEngine.Video; // Required for video components

public class DummyDialogue : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public EntityData dummyData;
    public GameObject gameUI;
    public GameObject proximityPanel;
    public GameObject deathPanel;


    [Header("Settings")]
    public float triggerDistance = 3f;

    private Entity entityScript;
    private bool hasTriggeredProximity = false;
    private bool hasTriggeredDeath = false;

    void Start()
    {
        entityScript = GetComponent<Entity>();
    }

    void Update()
    {
        if (player == null) return;

        // Proximity Trigger
        if (!hasTriggeredProximity)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance <= triggerDistance)
            {
                hasTriggeredProximity = true;
                ShowDialogue(proximityPanel);
            }
        }

        // Death Trigger
        if (!hasTriggeredDeath && entityScript != null && entityScript.IsDown())
        {
            hasTriggeredDeath = true;
            ShowDialogue(deathPanel);
        }
    }

    private void ShowDialogue(GameObject panel)
    {
        if (panel != null)
        {
            if (gameUI != null) gameUI.SetActive(false);
            panel.SetActive(true);
            Time.timeScale = 0f;

            // Automatically find and play any VideoPlayer inside this specific panel
            VideoPlayer video = panel.GetComponentInChildren<VideoPlayer>();
            if (video != null)
            {
                video.Play();
            }

            if (player != null)
            {
                PlayerController pc = player.GetComponent<PlayerController>();
                if (pc != null) pc.allowInput = false;
            }
        }
    }

    public void CloseDialogue(GameObject panel)
    {
        if (panel != null)
        {
            // Automatically find and stop the video before closing
            VideoPlayer video = panel.GetComponentInChildren<VideoPlayer>();
            if (video != null)
            {
                video.Stop();
            }

            panel.SetActive(false);
        }

        if (gameUI != null) gameUI.SetActive(true);
        Time.timeScale = 1f;

        if (player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null) pc.allowInput = true;
        }
    }
}