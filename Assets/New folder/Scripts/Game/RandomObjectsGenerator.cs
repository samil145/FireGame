using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay
{
    public class RandomObjectsGenerator : NetworkBehaviour
    {
        [SerializeField] private int numberOfObjects;
        [SerializeField] private List<NetworkObject> prefabs;
        [SerializeField] private float radius;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                RandomSeveralObjects();
            }
        }

        public void RandomSeveralObjects()
        {
            for (int i = 0; i < numberOfObjects; i++)
            {
                RandomGenerate();
            }
        }

        public void RandomGenerate()
        {
            int index = Random.Range(0, prefabs.Count);
            Vector3 pos = GeneratePosition();
            var obj = Instantiate(prefabs[index], pos, Quaternion.identity);
            obj.Spawn();
        }

        private Vector3 GeneratePosition()
        {
            float randRadius = Random.Range(0f, radius);
            float randAngle = Random.Range(0f, 360f);
            Vector3 pos = Vector3.zero;
            pos.z = randRadius * Mathf.Sin(randAngle);
            pos.x = randRadius * Mathf.Cos(randAngle);
            pos += transform.position;
            return pos;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
