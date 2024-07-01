using Connection;
using Gameplay;
using Utils;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections;

public class ApprovalHandler
{
    private NetworkManager.ConnectionApprovalRequest request;
    private NetworkManager.ConnectionApprovalResponse response;
    private ConnectionHandler connection;

    public ApprovalHandler(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        connection = ConnectionHandler.Instance;
        InjectDependencies(request, response);
    }

    public void InjectDependencies(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
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
        userData.side = numberOfClientsOnServer + 1 > (uint)connection.MaxNumberOfPlayers / 2 ? Belonging.Red : Belonging.Blue;
        if (TrySetSpawn(userData.side) == false)
        {
            return;
        }
        if (TrySetPlayerPrefab(connectionData.prefabId) == false)
        {
            return;
        }
        userData.clientId = request.ClientNetworkId;
        userData.playerName = new FixedString32Bytes(connectionData.playerName);
        ServerDataDB.Instance.ClientsData.Add(request.ClientNetworkId, userData);
        response.Approved = true;
        response.CreatePlayerObject = true;
        response.Pending = false;
    }

    private bool TrySetSpawn(in Belonging side)
    {
        int firstIndex = 0, endIndex = connection.MaxNumberOfPlayers / 2;
        if (side == Belonging.Red)
        {
            firstIndex = connection.MaxNumberOfPlayers / 2;
            endIndex = connection.MaxNumberOfPlayers;
        }
        bool flag = false;
        for (int i = firstIndex; flag == false && i < endIndex; i++)
        {
            ServerDataDB.SpawnPoint spawnPoint = ServerDataDB.Instance.PlayerSpawnPositions[i];
            if (spawnPoint.isOccupied == false)
            {
                flag = true;
                response.Position = spawnPoint.position.position;
                response.Rotation = spawnPoint.position.rotation;
                spawnPoint.isOccupied = true;
                spawnPoint.ownerId = request.ClientNetworkId;
            }
        }
        return flag;
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
