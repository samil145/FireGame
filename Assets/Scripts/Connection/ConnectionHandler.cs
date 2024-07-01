using Gameplay;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using System.Linq;

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
        private ApprovalHandler approvalHandler;

        private void Start()
        {
            if (instance == null)
            {
                instance = this;
                approvalData = new ApprovalData()
                {
                    prefabId = -1,
                    playerName = "Player"
                };
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            if (IsServer && NetworkManager.ConnectedClients.Count == MaxNumberOfPlayers)
            {
                IsGameStarted = true;
            }
        }

        public void StartClient()
        {
            NetworkManager.NetworkConfig.ConnectionData = approvalData.Serialize();
            if (NetworkManager.StartClient() == false)
            {
                Time.timeScale = 0f;
                Debug.LogWarning("Failled to connect");
            }
        }

        public void StartHost()
        {
            if (NetworkManager.Singleton.StartHost())
            {
                //Time.timeScale = 0f;
                ConfigurateServer();
                ServerDataDB.Instance.ClientsData.Add(0ul, new UserData()
                {
                    clientId = 0ul,
                    playerName = approvalData.playerName,
                    side = Belonging.Blue
                });
            }
            else
            {
                Debug.LogWarning("Failed to start host");
            }
        }

        public void StartServer()
        {
            if (NetworkManager.Singleton.StartServer())
            {
                Time.timeScale = 0f;
                ConfigurateServer();
            }
            else
            {
                Debug.LogWarning("Failed to start server");
            }
        }

        private void ConfigurateServer()
        {
            ServerDataDB.Instance.CleanDB();
            Application.targetFrameRate = 60;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        private void OnClientConnected(ulong id)
        {
            Debug.Log(IsServer);
            Debug.Log($"Client {id} connected");
        }

        private void OnClientDisconnected(ulong id)
        {
            ServerDataDB.SpawnPoint spawnPoint = ServerDataDB.Instance.PlayerSpawnPositions.Where((x) => x.ownerId == id).FirstOrDefault();
            spawnPoint.ownerId = null;
            spawnPoint.isOccupied = false;
            ServerDataDB.Instance.RemoveClient(id);
            StartGameClientRpc(id);
        }

        [ClientRpc]
        private void StartGameClientRpc(ulong clientId)
        {
            if (clientId == NetworkManager.LocalClientId)
            {
                StartGame();
                isGameStarted = false;
            }
        }

        public void DisconectLocalClient()
        {
            NetworkManager.DisconnectClient(NetworkManager.LocalClientId);
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
            if (approvalHandler == null)
            {
                approvalHandler = new ApprovalHandler(request, response);
            }
            else
            {
                approvalHandler.InjectDependencies(request, response);
            }
            approvalHandler.Handle();
        }

        public static ConnectionHandler Instance => instance;

        public string Address
        {
            get => address;
            set
            {
                if ((value ?? string.Empty) != string.Empty)
                {
                    UnityTransport transport = NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();
                    transport.SetConnectionData(address, 7777);
                    address = value;
                }
            }
        }

        public int MaxNumberOfPlayers => maxNumberOfPlayers;

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
            isGameStarted = true;
        }

        private void StartGame()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
        }

        public List<uint> PlayerCharactersHashes => playerCharacters;

        public string PlayerName { get => approvalData.playerName; set => approvalData.playerName = value; }

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
    }
}

