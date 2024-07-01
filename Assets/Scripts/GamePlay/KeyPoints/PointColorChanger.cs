using Unity.Netcode;
using UnityEngine;

namespace Gameplay
{
    public class PointColorChanger : NetworkBehaviour
    {
        private KeyPoint keyPoint;
        [SerializeField] private Transform positionalShift;
        [SerializeField] private GameObject indicatorPrefab;
        private GameObject indicator;
        private Renderer indicatorRenderer;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                indicator = Instantiate(indicatorPrefab, transform);
                indicatorRenderer = indicator.GetComponent<Renderer>();
                NetworkObject indicatorNet = indicator.GetComponent<NetworkObject>();
                indicatorNet.Spawn();
                if (positionalShift != null)
                {
                    indicator.transform.position = positionalShift.position;
                    indicator.transform.rotation = positionalShift.rotation;
                }
                indicator.SetActive(false);
            }
        }

        private void Start()
        {
            keyPoint = GetComponent<KeyPoint>();
            keyPoint.Captured += OnStateChanged;
        }

        private void OnStateChanged(Belonging belonging)
        {
            switch (belonging)
            {
                case Belonging.None:
                    indicator.SetActive(false);
                    break;
                case Belonging.Red:
                case Belonging.FullRed:
                    indicator.SetActive(true);
                    indicatorRenderer.material.color = Color.red;
                    break;
                case Belonging.Blue:
                case Belonging.FullBlue:
                    indicator.SetActive(true);
                    indicatorRenderer.material.color = Color.blue;
                    break;
                default:
                    break;
            }
        }

        public override void OnDestroy()
        {
            keyPoint.Captured -= OnStateChanged;
            base.OnDestroy();
        }
    }
}
