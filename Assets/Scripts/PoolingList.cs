using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetCodePoolingList<T> where T : NetworkBehaviour, IPooledObject
{
    private List<T> pooledObjects;
    private int numberOfObjects;
    private T pooledType;
    private GameObject parent;
    private int currentObj;
    private ulong clientId;

    public NetCodePoolingList(in int numberOfObjects, T pooledType, GameObject parent, in ulong id)
    {
        clientId = id;
        currentObj = 0;
        pooledObjects = new List<T>();
        this.numberOfObjects = numberOfObjects;
        this.parent = parent;
        this.pooledType = pooledType;
    }

    public void Pool()
    {
        if (pooledObjects.Count == 0)
        {
            for (int i = 0; i < numberOfObjects; i++)
            {
                pooledObjects.Add(Object.Instantiate(pooledType, parent.transform.position, parent.transform.rotation));
                pooledObjects[i].NetworkObject.SpawnWithOwnership(clientId);
                pooledObjects[i].NetworkObject.DontDestroyWithOwner = false;
                pooledObjects[i].Parent = parent;
                pooledObjects[i].PushBack();
            }
        }
    }

    public T GetNext()
    {
        if (currentObj >= numberOfObjects)
            currentObj = 0;
        pooledObjects[currentObj].PushBack();
        return pooledObjects[currentObj++];
    }

    public int Count
    {
        get => numberOfObjects;
        set
        {
            if (value < 0)
                throw new System.ArgumentException("Number of objects cannot be less than 0!");
            else if (value > numberOfObjects)
            {
                for (int i = numberOfObjects; i < value; i++)
                {
                    pooledObjects.Add(Object.Instantiate(pooledType, parent.transform.position, parent.transform.rotation));
                    pooledObjects[i].PushBack();
                }
                currentObj = 0;
                numberOfObjects = value;
            }
            else if (value < numberOfObjects)
            {
                pooledObjects.RemoveRange(value, numberOfObjects - value);
                currentObj = 0;
                numberOfObjects = value;
            }
        }
    }

    public void Despawn()
    {
        foreach (var item in pooledObjects)
        {
            item.NetworkObject.Despawn(true);
        }
    }
}

public interface IPooledObject
{
    bool IsPushedBack { get; }
    GameObject Parent { get; set; }
    void PushBack();
    void BecomeActive();
}

public interface IPooledOwner
{
    int NumberOfPooledObjects { get; }
    string GenerateId();
}