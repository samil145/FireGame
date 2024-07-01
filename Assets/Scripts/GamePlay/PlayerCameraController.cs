using Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerCameraController : NetworkBehaviour
{
    [Range(0f, 1f)]
    [SerializeField]
    private float rotationSpeed;
    [SerializeField] private CinemachineVirtualCamera thirdViewCamera;
    [SerializeField] private CinemachineVirtualCamera firstViewCamera;
    [SerializeField] private Transform viewPoint;
    [SerializeField]
    [Range(0f, 90f)]
    private float minimunRotationAngle;
    [SerializeField]
    [Range(0f, 90f)]
    private float maximunRotationAngle;
    private float viewRadius;
    private Vector3 firstViewCameraShift;
    private KeyCode swapCamera;

    private void Start()
    {
        viewRadius = viewPoint.localPosition.z;
        firstViewCameraShift = viewPoint.localPosition;
        swapCamera = KeyCode.F5;
        if (!IsLocalPlayer)
        {
            firstViewCamera.Priority = thirdViewCamera.Priority - 1;
            thirdViewCamera.enabled = false;
            firstViewCamera.enabled = false;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(swapCamera))
        {
            firstViewCamera.Priority += thirdViewCamera.Priority > firstViewCamera.Priority ? 2 : -2;
        }
    }

    public void RotateThirdPerson(Transform transform, Vector3 mouseInput)
    {
        Quaternion rotation = Quaternion.Euler(0, mouseInput.y, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotationSpeed);
        RotateFirstPerson(mouseInput.x);
    }

    public void RotateFirstPerson(float value)
    {
        value = Mathf.Clamp(value, -minimunRotationAngle, maximunRotationAngle);
        viewPoint.localRotation = Quaternion.Euler(-value, 0f, 0f);
        value *= Mathf.Deg2Rad;
        viewPoint.localPosition = new Vector3(0f, firstViewCameraShift.y + viewRadius * Mathf.Sin(value), viewRadius * Mathf.Cos(value));
    }

    public KeyCode SwapCamera
    {
        get => swapCamera;
        set => swapCamera = value;
    }
}
