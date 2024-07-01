using Unity.Netcode;
using UnityEngine;

namespace Gameplay
{
    [RequireComponent(typeof(CharacterController))]
    public class MovementController : NetworkBehaviour
    {
        [SerializeField] private float movementSpeed;
        [SerializeField] private float sprintSpeed;
        private Vector3 keyboardInput;
        private Vector3 mouseInput;
        private bool isSprinting;
        private KeyCode sprintKey;
        private PlayerCameraController cameraController;
        private CharacterController characterController;

        private void Start()
        {
            characterController = GetComponent<CharacterController>();
            TryGetComponent(out cameraController);
            sprintKey = KeyCode.LeftShift;
            keyboardInput = Vector3.zero;
            mouseInput = Vector3.zero;
        }

        private void Update()
        {
            keyboardInput.x = Input.GetAxis("Horizontal");
            keyboardInput.z = Input.GetAxis("Vertical");
            mouseInput.x += Input.GetAxis("Mouse Y");
            mouseInput.y += Input.GetAxis("Mouse X");
            isSprinting = Input.GetKey(sprintKey);
        }

        private void FixedUpdate()
        {
            new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            if (IsClient)
            {
                ServerRpcParams serverRpcParams = new ServerRpcParams();
                serverRpcParams.Receive.SenderClientId = NetworkManager.LocalClientId;
                MoveServerRpc(mouseInput, keyboardInput, serverRpcParams);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void MoveServerRpc(Vector3 mouseInput, Vector3 keyboardInput, ServerRpcParams @params)
        {
            ulong cliendId = @params.Receive.SenderClientId;
            if (NetworkManager.ConnectedClients.ContainsKey(cliendId))
            {
                NetworkClient client = NetworkManager.ConnectedClients[cliendId];
                MovementController moveController = client.PlayerObject.GetComponent<MovementController>();
                Transform transform = client.PlayerObject.transform;
                if (keyboardInput.magnitude > 0.001f)
                {
                    keyboardInput = transform.forward * keyboardInput.z + transform.right * keyboardInput.x;
                    moveController.characterController.Move(keyboardInput * Time.fixedDeltaTime * (moveController.isSprinting ? moveController.sprintSpeed : moveController.movementSpeed));
                }
                if (moveController.cameraController)
                {
                    moveController.cameraController.RotateThirdPerson(transform, mouseInput);
                }
            }
        }

        public bool IsSprinting => isSprinting;

        public KeyCode SprintKey { get => sprintKey; set => sprintKey = value; }

        public Vector3 InputMovement => keyboardInput;
    }
}

