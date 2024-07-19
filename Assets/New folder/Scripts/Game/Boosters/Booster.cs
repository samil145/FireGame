using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Boosters
{
    public abstract class Booster : NetworkBehaviour
    {
        private NetworkVariable<bool> isActive;
        [SerializeField] private bool toPerformOnClient;
        [SerializeField] private float coolDown;

        private void OnCollisionEnter(Collision collision)
        {
            Debug.Log("[Both] Booster Hitted");
            if (collision.gameObject.CompareTag("Gamer"))
            {
                if (toPerformOnClient)
                {
                    NetworkObject networkObject = collision.gameObject.GetComponent<NetworkObject>();
                    ApplyModifier(networkObject);
                    SetActiveServerRpc(false);
                }
                else
                {
                    ApplyModifierServerRpc(NetworkManager.LocalClientId);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetActiveServerRpc(bool val)
        {
            isActive.Value = false;
        }

        private void Awake()
        {
            isActive = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
            isActive.OnValueChanged += SetActive;
        }

        private void SetActive(bool prev, bool cur)
        {
            gameObject.SetActive(cur);
            if(IsServer)
            {
                NetworkManager.StartCoroutine(CoolDown());
            }
        }
        
        private IEnumerator CoolDown()
        {
            yield return new WaitForSeconds(coolDown);
            isActive.Value = true;
        }

        public override void OnNetworkSpawn()
        {
            gameObject.SetActive(isActive.Value);
        }

        [ServerRpc(RequireOwnership = false)]
        private void ApplyModifierServerRpc(ulong clientId)
        {
            if (NetworkManager.ConnectedClients.ContainsKey(clientId))
            {
                Debug.Log("[Server] Booster Hitted");
                NetworkObject networkObject = NetworkManager.ConnectedClients[clientId].PlayerObject;
                ApplyModifier(networkObject);
                isActive.Value = false;
            }
        }

        protected abstract void ApplyModifier(NetworkObject player);

        public override void OnDestroy()
        {
            isActive.OnValueChanged -= SetActive;
            base.OnDestroy();
        }
    }
}