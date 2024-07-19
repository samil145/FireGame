using Gameplay;
using Utils;
using Unity.Netcode;
using UnityEngine;

namespace Connection
{
    public class ApprovalHandler
    {
        private NetworkManager.ConnectionApprovalRequest request;
        private NetworkManager.ConnectionApprovalResponse response;
        private ConnectionHandler connection;

        public ApprovalHandler()
        {
            connection = ConnectionHandler.Instance;
        }

        public ApprovalHandler(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) : this()
        {
            this.request = request;
            this.response = response;
        }

        public void Handle()
        {
            ApprovalData connectionData = ICustomSerialiser<ApprovalData>.Fabricate(request.Payload);
            UserData userData = new UserData();
            response.Pending = true;
            response.Approved = false;
            response.PlayerPrefabHash = null;
            if (connection.IsGameStarted)
                return;
            int numberOfClientsOnServer = NetworkManager.Singleton.ConnectedClients.Count;
            if (numberOfClientsOnServer >= connection.MaxNumberOfPlayers)
            {
                Debug.LogError("Server is full");
                return;
            }
            userData.side = numberOfClientsOnServer % 2 == 0 ? Belonging.Red : Belonging.Blue;
            SetSpawn(userData.side);
            if (TrySetPlayerPrefab(connectionData.prefabId) == false)
            {
                return;
            }
            userData.clientId = request.ClientNetworkId;
            connection.ClientsData.Add(request.ClientNetworkId, userData);
            response.Approved = true;
            response.CreatePlayerObject = true;
            response.Pending = false;
        }

        private void SetSpawn(in Belonging side)
        {
            var spawnPoint = connection.PlayerSpawnPositions[side == Belonging.Blue ? 0 : 1];
            response.Position = spawnPoint.position;
            response.Rotation = spawnPoint.rotation;
        }

        private bool TrySetPlayerPrefab(in int playerPrefabIndex)
        {
            if (playerPrefabIndex >= 0)
            {
                if (connection.PlayerCharactersHashes.Count > playerPrefabIndex)
                {
                    response.PlayerPrefabHash = connection.PlayerCharactersHashes[playerPrefabIndex];
                }
                else
                {
                    Debug.LogError($"Client provided player prefab index of {playerPrefabIndex} when there are onlky {connection.PlayerCharactersHashes.Count} entries!");
                    return false;
                }
            }
            return true;
        }
    }
}

