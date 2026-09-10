using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent (typeof(CinemachineCamera), typeof(CinemachineFollow))]
public class CameraMove : MonoBehaviour
{
    [Header("---- Components -----")]
    [SerializeField] PlayerInput playerInput;

    [Header("----- Zoom -----")]
    [SerializeField] private Vector3 followZoom;
    [SerializeField] private Vector3 followInit;
    [SerializeField, Min(0.01f)] private float zoomSmoothTime;

    [SerializeField] private float deadZone;

    private GameObject player;
    private CinemachineFollow follow;
    private InputAction zoomAction;

    private Vector2 scroll;
    private Vector3 targetOffset;
    private Vector3 velocity;

#if UNITY_EDITOR
    private void Reset()
    {
        followZoom = new Vector3(0, 3, -3);
        followInit = new Vector3(0, 5.5f, -4.1f);
        zoomSmoothTime = 0.2f;

        deadZone = 0.3f;

        scroll = Vector2.zero;
        targetOffset = followInit;
        velocity = Vector3.zero;
    }
#endif

    private void Awake()
    {
        player = GameObject.FindWithTag("Player");
        playerInput = player.GetComponent<PlayerInput>();

        InputActionMap actionMap = playerInput.actions.FindActionMap("Player");

        CinemachineCamera cam = GetComponent<CinemachineCamera>();
        follow = GetComponent<CinemachineFollow>();

        cam.Target.TrackingTarget = player.transform.FindChildByName("CamPos");

        targetOffset = followInit;

        zoomAction = actionMap.FindAction("Zoom", true);
    }

    private void OnEnable()
    {
        zoomAction.performed += Zoom;
    }

    private void OnDisable()
    {
        zoomAction.performed -= Zoom;
    }

    private void Update()
    {
        follow.FollowOffset = Vector3.SmoothDamp(follow.FollowOffset, targetOffset, ref velocity, zoomSmoothTime);
    }

    private void Zoom(InputAction.CallbackContext context)
    {
        scroll = context.ReadValue<Vector2>();

        if (scroll.y > deadZone)
            targetOffset = followZoom;
        else if (scroll.y < deadZone)
            targetOffset = followInit;

    }

    // ÀÎº¥Åä¸® ¿ÀÇÂ ½Ã Ä«¸Þ¶ó ÀÌµ¿ ¸ØÃã

    private void OnGUI()
    {

    }
}
