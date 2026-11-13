using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newEntityData", menuName = "Data/Entity", order = 1)]
public class EntityData : ScriptableObject
{
    
    [SerializeField] int baseHP;
    [SerializeField] int baseATK;
    [SerializeField] int baseDEF;
    [SerializeField] int baseSPD;
    [SerializeField] int _xp;
    [SerializeField] List<SkillData> baseSkills;
    [SerializeField] float _size = 1;
    public float size { get { return _size; } }
    [SerializeField] Vector2 _offset;
    public Vector2 offset { get { return new Vector2(_offset.x,_offset.y); } }

    [Header("Sprite Animations")]
    [SerializeField] List<Sprite> idle;
    [SerializeField] List<Sprite> move;
    [SerializeField] List<Sprite> attack1;
    [SerializeField] List<Sprite> attack2;
    [SerializeField] List<Sprite> hurt;
    [SerializeField] List<Sprite> down;
    [SerializeField] public int framesPerSec=4;
    [SerializeField] public bool aniDir = false;
    [SerializeField] public AnimatorOverrideController animator;
    

    public int xp { get { return _xp;} }

    public Dictionary<Stat,int> GetStats()
    {
        return new Dictionary<Stat, int>()
        {
            {Stat.MaxHP, baseHP},
            {Stat.HP, baseHP},
            {Stat.ATK, baseATK},
            {Stat.DEF, baseDEF},
            {Stat.SPD, baseSPD}
        };
    }

    public List<SkillData> GetSkills()
    {
        return new List<SkillData>(baseSkills);
    }

    public Dictionary<StateAnimator.State,List<Sprite>> Sprites()
    {
        //Add directions
        Dictionary<StateAnimator.State,List<Sprite>> sprites = new();
        sprites[StateAnimator.State.Idle] = idle;
        sprites[StateAnimator.State.Move] = move;
        sprites[StateAnimator.State.Attack1] = attack1;
        sprites[StateAnimator.State.Attack2] = attack2;
        sprites[StateAnimator.State.Hurt] = hurt;
        sprites[StateAnimator.State.Down] = down;

        return sprites;
    }

    public enum Stat
    {
        None,
        HP,
        ATK,
        DEF,
        SPD,
        ATKBONUS,
        DMG,
        MaxHP
    }
}
