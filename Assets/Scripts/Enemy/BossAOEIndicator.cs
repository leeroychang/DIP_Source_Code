using System;
using System.Collections;
using UnityEngine;

public class BossAOEIndicator : MonoBehaviour
{
    [Header("Colors")]
    [SerializeField] Color warningColor = new Color(1f, 0.6f, 0f, 0.4f); //semi-transparent orange
    [SerializeField] Color dangerColor  = new Color(1f, 0f,   0f, 0.8f); // solid red

    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void Activate(float castTime, Action onComplete)
    {
        StartCoroutine(CastRoutine(castTime, onComplete));
    }

    IEnumerator CastRoutine(float castTime, Action onComplete)
    {
        if (sr != null) sr.color = warningColor;

        float elapsed = 0f;
        float quickTime = 0.8f;
        float dangerTime = castTime-quickTime;
        Color midColor = Color.Lerp(warningColor, dangerColor, 0.3f);
        while (elapsed < castTime)
        {
            elapsed += Time.deltaTime;
            if (sr != null)
            if (elapsed<dangerTime) sr.color = Color.Lerp(warningColor,midColor,elapsed/dangerTime);
            sr.color = Color.Lerp(midColor, dangerColor, elapsed-dangerTime / quickTime);
            yield return null;
        }
        sr.enabled = false;
        yield return new WaitForSeconds(0.3f);

        if (sr != null) sr.color = dangerColor;
        onComplete?.Invoke();
    }
}
