using Unity.Mathematics;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    Transform player;

    [SerializeField] float followSpeed = 1f;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void FixedUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position,player.position,math.clamp(followSpeed*Time.fixedDeltaTime*Vector3.Distance(transform.position,player.position),Time.fixedDeltaTime,followSpeed*10*Time.fixedDeltaTime));
    }
}
