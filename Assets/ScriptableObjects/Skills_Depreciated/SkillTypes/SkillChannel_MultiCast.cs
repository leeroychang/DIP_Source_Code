using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class SkillChannel_MultiCast : SkillChannel_Block
{
    [SerializeField] float castInterval = 0.1f;

    public override void Cast()
    {
        int count = (int) multi + 1;
        caster.StartCoroutine(CastLoop(count));
        
    }

    public IEnumerator CastLoop(int count)
    {
        for (int i=0; i<count; i++)
        {
            cast.OnUse(caster,targetPos,true);
            yield return new WaitForSeconds(castInterval);
        }
    }
}
