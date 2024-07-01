using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GeneralPooling : NetworkBehaviour
{
    private class Container
    {
        private int currentObj;
        private int numberOfObjects;
        private int maxIndex;

        public Container()
        {
            currentObj = 0;
            numberOfObjects = -1;
        }

        public Container(int currentId, int numberOfObjects)
        {
            this.currentObj = currentId;
            this.numberOfObjects = numberOfObjects;
            maxIndex = currentId + numberOfObjects;
        }

        public int CurrentObj => currentObj;

        public void ShiftNextCurrentObj()
        {
            currentObj += 1;
            if (currentObj >= maxIndex)
            {
                currentObj = maxIndex - numberOfObjects;
            }
        }

        public void ResetCurrentId() => currentObj = 0;
    }

    [Serializable]
    private struct PooledObjectAndOwner
    {
        public GameObject Owner;
        public NetworkObject PooledObject;
    }

    private static GeneralPooling instance;
    [SerializeField]
    [Tooltip("All of them MUST realize IPooledOwner interface")]
    private List<PooledObjectAndOwner> poolObjectPrefabs;
    private List<IPooledObject> pooledObjects;
    private Dictionary<string, Container> currentIdOfObject;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
            pooledObjects = new List<IPooledObject>();
            currentIdOfObject = new Dictionary<string, Container>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Pool();
        }
    }

    private void Pool()
    {
        int i = 0;
        int maxNumberOfPooledObjectsOfPrefab = 0;
        foreach (var item in poolObjectPrefabs)
        {
            IPooledOwner pooledOwner = item.Owner.GetComponent<IPooledOwner>();
            maxNumberOfPooledObjectsOfPrefab += pooledOwner.NumberOfPooledObjects * Connection.ConnectionHandler.Instance.MaxNumberOfPlayers;
            for (; i < maxNumberOfPooledObjectsOfPrefab; i++)
            {
                NetworkObject pooledNetObject = Instantiate(item.PooledObject, transform.position, Quaternion.identity);
                pooledNetObject.Spawn();
                pooledNetObject.TrySetParent(transform);
                IPooledObject pooledObject = pooledNetObject.GetComponent<IPooledObject>();
                pooledObjects.Add(pooledObject);
                pooledObject.PushBack();
                if (i % pooledOwner.NumberOfPooledObjects == 0)
                {
                    Container container = new Container(i, pooledOwner.NumberOfPooledObjects);
                    currentIdOfObject.Add(pooledOwner.GenerateId(), container);
                }
            }
        }
    }

    public IPooledObject GetNext(string parentId)
    {
        Container container = currentIdOfObject[parentId];
        IPooledObject obj = pooledObjects[container.CurrentObj];
        container.ShiftNextCurrentObj();
        return obj;
    }

    public void PushBackAll(string parentId)
    {
        Container container = currentIdOfObject[parentId];
        container.ResetCurrentId();
        IPooledObject obj;
        do
        {
            obj = pooledObjects[container.CurrentObj];
            if (obj.IsPushedBack == false)
            {
                obj.PushBack();
            }
            container.ShiftNextCurrentObj();
        } while (container.CurrentObj != 0);
    }

    public void Despawn()
    {

    }

    public static GeneralPooling Instance => instance;
}