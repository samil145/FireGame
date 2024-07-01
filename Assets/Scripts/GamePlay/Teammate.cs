using Connection;
using Gameplay.Creatures;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay
{
    public class Teammate : NetworkBehaviour, ILivingCreature
    {
        private NetworkVariable<Belonging> side;
        [SerializeField] private int maxHp;
        private NetworkVariable<int> hp;
        [SerializeField]
        [CustomAttributes.ReadOnlyField]
        private Belonging sideDebug;

        private void Awake()
        {
            side = new NetworkVariable<Belonging>(Belonging.Blue, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
            side.OnValueChanged += OnSideChanged;
            hp = new NetworkVariable<int>(maxHp, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
            hp.OnValueChanged += InspectHP;
            sideDebug = side.Value;
        }

        private void InspectHP(int prev, int current)
        {
            Debug.Log($"Previous: {prev}\tCurrent: {current}");
            if (current <= 0)
            {
                gameObject.SetActive(false);
            }
        }

        private void OnSideChanged(Belonging prev, Belonging cur)
        {
            Debug.Log($"Client with ID: {NetworkManager.LocalClientId} is {cur}!");
            sideDebug = cur;
        }

        public override void OnNetworkSpawn()
        {
            if (IsLocalPlayer && IsHost == false)
            {
                GetClientDataServerRpc(NetworkManager.LocalClientId);
                sideDebug = side.Value;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void GetClientDataServerRpc(ulong id)
        {
            if (NetworkManager.ConnectedClients.ContainsKey(id))
            {
                UserData userData = ServerDataDB.Instance.ClientsData[id];
                NetworkClient client = NetworkManager.ConnectedClients[id];
                Teammate teammate = client.PlayerObject.GetComponent<Teammate>();
                teammate.side.Value = userData.side;
            }
        }

        public Belonging Side { get => side.Value; set => side.Value = value; }

        public int HP
        {
            get => hp.Value;
            set
            {
                if (IsServer)
                {
                    hp.Value = value <= 0 ? 0 : value;
                }
            }
        }

        public int MaxHP { get => maxHp; set => maxHp = value; }
    }
}

