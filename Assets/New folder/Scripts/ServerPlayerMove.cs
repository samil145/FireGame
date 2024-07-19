using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

/// <summary>
/// Server side script to do some movements that can only be done server side with Netcode. In charge of spawning (which happens server side in Netcode)
/// and picking up objects
/// </summary>
[DefaultExecutionOrder(0)] // before client component
public class ServerPlayerMove : NetworkBehaviour
{
    CharacterController m_characterController;
    Vector3 move = Vector3.zero;

    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer && !IsOwner)
        {
            enabled = false;
            return;
        }
        m_characterController = GetComponent<CharacterController>();
    }

    [ServerRpc]
    public void MovePlayerServerRpc(Vector3 move)
    {
        if (move != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(move), 6f * Time.fixedDeltaTime);
        m_characterController.Move(move * 7f * Time.fixedDeltaTime);
    }
}