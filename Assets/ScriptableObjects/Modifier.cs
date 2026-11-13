using System;
using UnityEngine;

[Serializable]
public abstract class Modifier
{
    public abstract void OnEnable(Skill skill, int slot);
    public abstract void OnDisable(Skill skill, int slot);
    
}
