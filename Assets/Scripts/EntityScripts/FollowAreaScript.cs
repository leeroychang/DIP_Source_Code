using UnityEngine;

public class FollowAreaScript : SpawnedAreaScript
{
    public Entity followTarget;

    void FixedUpdate()
    {
        TickUpdate();
        if (followTarget!=null) transform.position = followTarget.transform.position;
    }
}
