using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Weapons
{
    public class PoolingWeapon : DistanceWeapon, IPooledOwner
    {
        private static Dictionary<string, Queue<string>> ids = new Dictionary<string, Queue<string>>();
        private static string prevNameInGeneretingOrder = "";
        private static int idGenerator = 0;
        [SerializeField] private new string name;
        [SerializeField]
        [CustomAttributes.ReadOnlyField]
        private string id;

        public int NumberOfPooledObjects { get => Magazine; }

        public string GenerateId()
        {
            if (prevNameInGeneretingOrder != name)
            {
                prevNameInGeneretingOrder = name;
                idGenerator = 0;
            }
            string temp = name + "_" + idGenerator++;
            if (ids.ContainsKey(name) == false)
            {
                ids.Add(name, new Queue<string>());
            }
            ids[name].Enqueue(temp);
            return temp;

        }

        private new void Start()
        {
            base.Start();
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                id = ids[name].Dequeue();
                Debug.Log(id);
            }
        }

        public override void OnDestroy()
        {
            if (IsServer)
            {
                ids[name].Enqueue(id);
                GeneralPooling.Instance.PushBackAll(id);
            }
            base.OnDestroy();
        }

        protected override void Shoot()
        {
            Ammo ammo = GeneralPooling.Instance.GetNext(id) as Ammo;
            ammo.Parent = gameObject;
            ammo.Side = side;
            ammo.BecomeActive();
        }
    }
}
