using CustomAttributes;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Connection
{
    public class ServerDataDB : NetworkBehaviour
    {
        [Serializable]
        public class SpawnPoint
        {
            public Transform position;
            [SerializeField]
            [ReadOnlyField]
            public bool isOccupied;
            public ulong? ownerId;

            public override string ToString() => $"Position: {position.position};Rotation: {position.rotation.eulerAngles};IsOccupied: {isOccupied};OwnerId: {ownerId}";
        }

        private static ServerDataDB instance;
        private Dictionary<ulong, UserData> clientsData;
        [SerializeField] private List<SpawnPoint> spawnPoints;

        private void Start()
        {
            if (instance == null)
            {
                instance = this;
                clientsData = new Dictionary<ulong, UserData>();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void CleanDB()
        {
            clientsData.Clear();
        }

        public void RemoveClient(ulong clientId)
        {
            clientsData.Remove(clientId);
        }

        public static ServerDataDB Instance => instance;

        public Dictionary<ulong, UserData> ClientsData => clientsData;

        public List<SpawnPoint> PlayerSpawnPositions => spawnPoints;
    }
}
