using System.Collections.Generic;
using UnityEngine;

public class StateAnimator : MonoBehaviour
{
    [SerializeField] SpriteSheetAnimation animator;

    Dictionary<State,List<Sprite>> spritesheet = new();
    State current;
    
    public void Init(Dictionary<State,List<Sprite>> sprites) { spritesheet = sprites; }

    public void SetState(State state, int framesPerSec=4, bool ignoreCurrent=false)
    {
        if (animator==null || (!ignoreCurrent && current==state)) return;
        if (state==State.Down) animator.spriteRenderer.color = Color.gray;
        if (state==State.Idle || state==State.Move) animator.SetAnimation(spritesheet[state],true,framesPerSec);
        else animator.SetAnimation(spritesheet[state],true,framesPerSec);
        current = state;
        Debug.Log("State set");
    }

    public enum State
    {
        Idle,
        Move,
        Attack1,
        Attack2,
        Hurt,
        Down
    }

}
