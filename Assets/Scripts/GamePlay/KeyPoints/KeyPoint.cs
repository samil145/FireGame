using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using CustomAttributes;

namespace Gameplay
{
    public class KeyPoint : NetworkBehaviour
    {
        private const int TO_FULL_BELONGING_SHIFT = 1;

        [SerializeField]
        [ReadOnlyField]
        private int numberOfBluePlayers;
        [SerializeField]
        [ReadOnlyField]
        private int numberOfRedPlayers;
        [SerializeField]
        [ReadOnlyField]
        private bool isFreshlyOccupied;
        [SerializeField] private Image imagePoint;
        [SerializeField]
        [Tooltip("Expressed in seconds")]
        private float timeToCapture;
        [SerializeField]
        [Tooltip("Expressed in seconds")]
        private float fillingPeriod;
        private float step;
        private NetworkVariable<Belonging> currentStatus;
        private NetworkVariable<float> imageFilling;
        public event UnityAction<Belonging> Captured;
        [SerializeField]
        [ReadOnlyField]
        private Belonging sideDebug;

        private void Awake()
        {
            currentStatus = new NetworkVariable<Belonging>(Belonging.None, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
            currentStatus.OnValueChanged += OnStatusChanged;
            imageFilling = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
            imageFilling.OnValueChanged += OnFillingChanged;
        }

        private void OnStatusChanged(Belonging prev, Belonging current)
        {
            Captured?.Invoke(current);
            Debug.Log($"State of key point with ID {NetworkObjectId} is {current}");
            ChangeImageColorAccordingToSide(current);
            sideDebug = current;
        }

        private void OnFillingChanged(float prev, float cur)
        {
            imagePoint.fillAmount = cur;
        }

        public override void OnNetworkSpawn()
        {
            ChangeImageColorAccordingToSide(currentStatus.Value);
            imagePoint.fillAmount = imageFilling.Value;
            sideDebug = currentStatus.Value;
        }
        private void Start()
        {
            isFreshlyOccupied = true;
            step = fillingPeriod / timeToCapture;
        }

        private void ChangeImageColorAccordingToSide(Belonging belonging)
        {
            if (belonging == Belonging.Blue || belonging == Belonging.Blue + TO_FULL_BELONGING_SHIFT)
            {
                imagePoint.color = new Color(0, 0, 1, 1);
            }
            else
            {
                imagePoint.color = new Color(1, 0, 0, 1);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.TryGetComponent(out Teammate teammate))
            {
                if (teammate.IsLocalPlayer)
                {
                    ClashForPointServerRpc(NetworkObjectId, teammate.Side, false);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.TryGetComponent(out Teammate teammate))
            {
                if (teammate.IsLocalPlayer)
                {
                    ClashForPointServerRpc(NetworkObjectId, teammate.Side, true);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void ClashForPointServerRpc(ulong pointId, Belonging side, bool isLeaving)
        {
            KeyPoint point = GetNetworkObject(pointId).GetComponent<KeyPoint>();
            point.CalculateNumberOfTeammates(side, isLeaving);
            point.StartCoroutine(Capture());
        }

        private void CalculateNumberOfTeammates(Belonging teammateSide, in bool isLeaving)
        {
            int shift = isLeaving ? -1 : 1;
            Debug.Log("Debug");
            switch (teammateSide)
            {
                case Belonging.Red:
                    numberOfRedPlayers += shift;
                    break;
                case Belonging.Blue:
                    numberOfBluePlayers += shift;
                    break;
            }
        }

        private IEnumerator Capture()
        {
            Debug.Log("Started!");
            while ((numberOfBluePlayers == 0 && numberOfRedPlayers == 0) == false)
            {
                Debug.Log("Clashing");
                if (numberOfBluePlayers != numberOfRedPlayers)
                {
                    Clash(numberOfBluePlayers > numberOfRedPlayers ? Belonging.Blue : Belonging.Red);
                }
                yield return new WaitForSeconds(step);
            }
            Debug.Log("Ended");
            NetworkManager.StartCoroutine(RestoreControl());
        }

        private void Clash(Belonging allySide)
        {
            if (currentStatus.Value == Belonging.None)
            {
                currentStatus.Value = allySide;
            }
            if (currentStatus.Value == allySide || currentStatus.Value == allySide + TO_FULL_BELONGING_SHIFT)
            {
                ImageFilling += step;

                if (imageFilling.Value >= 1)
                {
                    isFreshlyOccupied = false;
                }
            }
            else
            {
                ImageFilling -= step;
                if (imageFilling.Value <= 0)
                {
                    isFreshlyOccupied = true;
                    currentStatus.Value = allySide;
                }
            }
        }

        private IEnumerator RestoreControl()
        {
            Debug.Log("Control is restoring!");
            while (numberOfBluePlayers == numberOfRedPlayers && numberOfRedPlayers == 0 && imageFilling.Value != 0 && imageFilling.Value != 1)
            {
                ImageFilling += isFreshlyOccupied ? -step : step;
                Debug.Log("Restoring");
                yield return new WaitForSeconds(step);
            }
            if (imageFilling.Value == 0)
            {
                currentStatus.Value = Belonging.None;
            }
            Debug.Log("Control is restored!");
        }

        public override void OnDestroy()
        {
            currentStatus.OnValueChanged -= OnStatusChanged;
            imageFilling.OnValueChanged -= OnFillingChanged;
            base.OnDestroy();
        }

        private float ImageFilling
        {
            get => imageFilling.Value;
            set
            {
                if (value < 0)
                {
                    value = 0;
                    if(currentStatus.Value == Belonging.FullRed || currentStatus.Value == Belonging.FullBlue)
                    {
                        currentStatus.Value -= TO_FULL_BELONGING_SHIFT;
                    }
                }
                else if (value >= 1)
                {
                    value = 1;
                    if (currentStatus.Value == Belonging.Red || currentStatus.Value == Belonging.Blue)
                    {
                        currentStatus.Value += TO_FULL_BELONGING_SHIFT;
                    }
                }
                imageFilling.Value = value;
            }
        }
    }
}