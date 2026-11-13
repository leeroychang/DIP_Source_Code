using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineRenderAnimation : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] Texture2D texture;
    [SerializeField] int spriteCount;
    [SerializeField] int framesPerSec = 12;
    [SerializeField] bool loop = true;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] bool destroyOnEnd = false;
    [SerializeField] float duration;

    public GameObject obj1;
    public GameObject obj2;
    public Vector2 pos1;
    public Vector2 pos2;

    Coroutine animationCoroutine;

    float offset;
    float frameTime;

    void Awake()
    {
        if (texture==null || spriteCount==0 || framesPerSec==0) return;
        animationCoroutine = StartCoroutine(StartAnimation());
    }

    void OnDisable()
    {
        if (animationCoroutine!=null) StopCoroutine(animationCoroutine);
        if (destroyOnEnd) Destroy(gameObject);
    }

    public IEnumerator StartAnimation()
    {
        lineRenderer.material.mainTexture = texture;
        offset = 1f/spriteCount;
        frameTime = 1f/framesPerSec;
        lineRenderer.textureScale = new Vector2(offset,1);
        do
        {
            for (int i=0; i<spriteCount; i++)
            {
                if (obj1!=null) pos1 = obj1.transform.position;
                if (obj2!=null) pos2 = obj2.transform.position;
                lineRenderer.SetPositions(new Vector3[] {pos1,pos2});
                lineRenderer.material.SetTextureOffset("_BaseMap",new Vector2(i*offset,0));
                yield return new WaitForSeconds(frameTime);
                if (duration!=0) duration -= frameTime;
            }
        } while (loop || duration>0);
        if (destroyOnEnd) OnDisable();
    }



}
