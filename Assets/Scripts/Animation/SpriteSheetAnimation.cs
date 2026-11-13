using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpriteSheetAnimation : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] List<Sprite> spritesheet = new();
    [SerializeField] int framesPerSec = 12;
    [SerializeField] bool loop = true;
    [SerializeField] public SpriteRenderer spriteRenderer;
    [SerializeField] bool destroyOnEnd = false;

    Coroutine animationCoroutine;

    void OnEnable()
    {
        if (spritesheet==null || spritesheet.Count==0 || framesPerSec==0) return;
        animationCoroutine = StartCoroutine(StartAnimation());
    }

    void OnDisable()
    {
        if (animationCoroutine!=null) StopCoroutine(animationCoroutine);
        if (destroyOnEnd) Destroy(gameObject);
    }

    public IEnumerator StartAnimation()
    {
        do
        {
            for (int i=0; i<spritesheet.Count; i++)
            {
                spriteRenderer.sprite = spritesheet[i];
                yield return new WaitForSeconds(1f/framesPerSec);
            }
        } while (loop);
        if (destroyOnEnd) OnDisable();
    }

    public void SetAnimation(List<Sprite> sprites, bool loop=true, int framesPerSec=4, bool destroyOnEnd=false)
    {
        if (animationCoroutine!=null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        if (sprites==null || sprites.Count==0) return;

        spritesheet = sprites;
        this.loop = loop;
        this.destroyOnEnd = destroyOnEnd;
        this.framesPerSec = framesPerSec;

        animationCoroutine = StartCoroutine(StartAnimation());



    }



}
