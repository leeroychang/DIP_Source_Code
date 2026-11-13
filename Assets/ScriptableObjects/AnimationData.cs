using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class AnimationData
{
    [SerializeField] List<Sprite> idle;
    [SerializeField] List<Sprite> move;
    [SerializeField] List<Sprite> attack1;
    [SerializeField] List<Sprite> attack2;
    [SerializeField] List<Sprite> hurt;
    [SerializeField] List<Sprite> down;
    [SerializeField] public int framesPerSec=4;
}
