using Gameplay;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

namespace Connection
{
    public class ConnectionHandler : NetworkBehaviour
    {
        private static ConnectionHandler instance;
        [SerializeField] private int maxNumberOfPlayers;
        [SerializeField] private List<uint> playerCharacters;
        private string address;
        private ApprovalData approvalData;
        private bool isGameStarted;
        private Dictionary<ulong, UserData> clientsData;
        [SerializeField] private List<Transform> spawnPoints;
        [SerializeField] private List<GameObject> invisableWalls;
        [SerializeField] GameObject firePoint;
        [SerializeField] GameObject topBar;


        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                clientsData = new Dictionary<ulong, UserData>();
                approvalData = new ApprovalData()
                {
                    prefabId = -1
                };
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            if (isGameStarted == false && IsServer && NetworkManager.ConnectedClients.Count == MaxNumberOfPlayers)
            {
                IsGameStarted = true;
            }
        }

        public void StartClient()
        {
            NetworkManager.NetworkConfig.ConnectionData = approvalData.Serialize();
            if (NetworkManager.StartClient())
            {
                //Time.timeScale = 0f;
            }
            else
            {
                Debug.LogWarning("Failled to connect");
            }
        }

        public void StartServer()
        {
            if (NetworkManager.Singleton.StartServer())
            {
                invisableWalls.ForEach(g => g.SetActive(true));
                ConfigurateServer();
            }
            else
            {
                Debug.LogWarning("Failed to start server");
            }
        }

        private void ConfigurateServer()
        {
            clientsData.Clear();
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        private void OnClientConnected(ulong id)
        {
            Debug.Log($"Client {id} connected");
        }

        private void OnClientDisconnected(ulong id)
        {
            clientsData.Remove(id);
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
            }
        }

        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            new ApprovalHandler(request, response).Handle();
        }

        public static ConnectionHandler Instance => instance;

        public string Address
        {
            get => address;
            set
            {
                if ((value ?? string.Empty) != string.Empty)
                {
                    address = value;
                    UnityTransport transport = NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();
                    transport.SetConnectionData(address, 7777);
                }
            }
        }

        public int MaxNumberOfPlayers { get => maxNumberOfPlayers; set => maxNumberOfPlayers = value > 0 ? value : maxNumberOfPlayers; }

        public bool IsGameStarted
        {
            get => isGameStarted;
            private set
            {
                isGameStarted = value;
                if (isGameStarted)
                {
                    StartGame();
                    StartGameClientRpc();
                }
            }
        }

        [ClientRpc]
        private void StartGameClientRpc()
        {
            StartGame();
        }

        private void StartGame()
        {
            topBar.SetActive(true);
            firePoint.SetActive(true);
            invisableWalls.ForEach(wall => wall.SetActive(false));
            isGameStarted = true;
            string message = (IsServer ? "[Server]" : "[Client]") + " Game started";
            Debug.Log(message);
        }

        public List<uint> PlayerCharactersHashes => playerCharacters;

        public int UserPlayerPrefabId
        {
            get => approvalData.prefabId;
            set
            {
                if (value > playerCharacters.Count)
                {
                    Debug.LogError($"Trying to assign player prefab index of {value} when there are onlky {playerCharacters.Count} entries!");
                    return;
                }
                if (NetworkManager.IsListening || IsSpawned)
                {
                    Debug.LogError("This needs to be set this prior to connecting!");
                    return;
                }
                approvalData.prefabId = value;
            }
        }

        public void GameOver(Belonging winSide)
        {
            if (isGameStarted)
            {
                GameOverClientRpc(winSide);
            }
        }

        [ClientRpc]
        private void GameOverClientRpc(Belonging winSide)
        {
            UiController.Instance.GameOver(winSide);
            //NetworkManager.LocalClient.PlayerObject.GetComponent<MovementRigidbody>().enabled = false;
        }

        public void CheckIfTeamIsDead(ulong deadClientId)
        {
            UserData userData = clientsData[deadClientId];
            userData.isDead = true;
            clientsData[deadClientId] = userData;
            int blueAlive = 0;
            int redAlive = 0;
            foreach (var item in clientsData)
            {
                if (item.Value.isDead == false)
                {
                    if (item.Value.side == Gameplay.Belonging.Red)
                    {
                        redAlive++;
                    }
                    else
                    {
                        blueAlive++;
                    }
                }
            }
            if (redAlive == 0 || blueAlive == 0)
            {
                GameOverClientRpc(redAlive == 0 ? Belonging.Blue : Belonging.Red);
            }
        }

        public Dictionary<ulong, UserData> ClientsData => clientsData;

        public List<Transform> PlayerSpawnPositions => spawnPoints;
    }
}
