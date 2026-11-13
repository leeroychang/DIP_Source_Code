using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugCamera : MonoBehaviour
{
    [Header("Control Settings")]
    [SerializeField] float moveSpeed = 1f;
    [SerializeField] float zoomSpeed = 5f;

    [SerializeField] float minZoom = 5f;
    [SerializeField] float maxZoom = 30f;

    [Header("References")]
    [SerializeField] Camera cam;
    [SerializeField] InputActionAsset actionAsset;
    InputAction moveAction;
    InputAction zoomAction;

    void Awake()
    {
        moveAction = actionAsset.FindAction("Movement");
        zoomAction = actionAsset.FindAction("Zoom");
    }

    // void OnEnable()
    // {
    //     moveAction.Enable();
    //     zoomAction.Enable();
    // }

    // void OnDisable()
    // {
    //     moveAction.Disable();
    //     zoomAction.Disable();
    // }

    void FixedUpdate()
    {
        Vector2 moveDir = moveAction.ReadValue<Vector2>();
        if (moveDir!=Vector2.zero) transform.Translate(moveDir*Time.fixedDeltaTime*moveSpeed*cam.orthographicSize);
    }

    void Update()
    {
        float zoomVal = zoomAction.ReadValue<float>();
        if (zoomVal==0) return;
        
        float currentZoom = cam.orthographicSize;
        cam.orthographicSize = math.clamp(currentZoom-zoomVal*zoomSpeed,minZoom,maxZoom);
        
    }
}
