using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioPlayer : MonoBehaviour
{
    [SerializeField] List<AudioClip> clips;
    AudioSource source;

    Coroutine audioCoroutine;

    void Start()
    {
        source = GetComponent<AudioSource>();
        audioCoroutine = StartCoroutine(Play());
    }

    void OnDisable()
    {
        if (audioCoroutine==null) return;
        StopCoroutine(audioCoroutine);
        audioCoroutine = null;
    }

    public IEnumerator Play()
    {
        while (true)
        {
            AudioClip clip = clips[UnityEngine.Random.Range(0,clips.Count)];
            source.clip = clip;
            source.Play();
            yield return new WaitForSecondsRealtime(clip.length);
        }
    }
}
