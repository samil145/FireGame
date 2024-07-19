using System;
using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Gameplay
{
    public class KeyPoint : NetworkBehaviour
    {
        private const int TO_FULL_BELONGING_SHIFT = 1;
        [SerializeField] private float EMMISION_INTENSITY = 2f;

        private int numberOfBluePlayers;
        private int numberOfRedPlayers;
        private bool isFreshlyOccupied;
        [SerializeField] private Image imagePoint;
        [SerializeField] private GameObject point;
        private Material pointMaterial;
        [SerializeField, Tooltip("Steps per second")] private float stepFrequency;
        private float stepPeriod;
        [SerializeField, Tooltip("Expressed in seconds")] private float timeToCapture;
        [SerializeField, Range(1f, 10f)] private float fillingSpeedMultiplier;
        private float step;
        private NetworkVariable<Belonging> currentStatus;
        private NetworkVariable<float> imageFilling;
        private NetworkVariable<bool> isRestoring;
        [SerializeField] private Color blueTeam;
        [SerializeField] private Color redTeam;
        [SerializeField] private Color initial;
        private Color currentColor;
        private float time;
        [SerializeField] private TMP_Text score;
        /* Debug code
        private int numberOfSteps;
        private DateTime startOfCapture;
        private bool debug;
        */
        private void Awake()
        {
            /* Debug Code
            debug = true;
            numberOfSteps = 0;
            */
            currentColor = initial;
            pointMaterial = point.GetComponent<Renderer>().material;
            time = 0f;
            stepPeriod = 1f / stepFrequency;
            //period = 1 / freq
            //period per step ---- step
            //time to capture ---- 1
            //step = period per step * 1 / time to capture = 1 / (time to capture * freq)
            pointMaterial.EnableKeyword("_EMISSION");
            pointMaterial.SetColor("_EmissionColor", currentColor);
            step = stepPeriod / timeToCapture;
            isRestoring = new NetworkVariable<bool>(true);
            currentStatus = new NetworkVariable<Belonging>(Belonging.None);
            currentStatus.OnValueChanged += OnStatusChanged;
            imageFilling = new NetworkVariable<float>(0f);

        }

        private void Update()
        {
            Color futureColor = isRestoring.Value ? initial : ((currentStatus.Value == Belonging.Blue ? blueTeam : redTeam) * EMMISION_INTENSITY);
            currentColor = Color.Lerp(currentColor, futureColor, (isRestoring.Value ? 3 : 1) * Time.deltaTime / timeToCapture);
            pointMaterial.SetColor("_EmissionColor", currentColor);
            if (IsClient)
            {
                imagePoint.fillAmount = Mathf.Lerp(imagePoint.fillAmount, imageFilling.Value, fillingSpeedMultiplier * Time.deltaTime);
                score.text = ((int)(imageFilling.Value * 100f)).ToString();
            }
            else //IsServer
            {
                if (time >= stepPeriod)
                {
                    time -= stepPeriod;
                    if ((numberOfBluePlayers == 0 && numberOfRedPlayers == 0) == false) //Some one stands on the point
                    {
                        if (numberOfBluePlayers != numberOfRedPlayers)
                        {
                            Clash(numberOfBluePlayers > numberOfRedPlayers ? Belonging.Blue : Belonging.Red);
                            /*++numberOfSteps; Debug Code*/
                        }
                    }
                    else if (ImageFilling != 0 && ImageFilling != 1)
                    {
                        if (isRestoring.Value == false)
                        {
                            isRestoring.Value = true;
                        }
                        ImageFilling += isFreshlyOccupied ? -step : step;
                        if (ImageFilling == 0)
                        {
                            currentStatus.Value = Belonging.None;
                        }
                    }
                }
                time += Time.deltaTime;
            }
        }

        private void Clash(Belonging allySide)
        {
            if (currentStatus.Value == Belonging.None)
            {
                currentStatus.Value = allySide;
                isRestoring.Value = false;
            }
            if (currentStatus.Value == allySide || currentStatus.Value == allySide + TO_FULL_BELONGING_SHIFT)
            {
                ImageFilling += step;
                if (ImageFilling >= 1)
                {
                    isFreshlyOccupied = false;
                }
            }
            else
            {
                ImageFilling -= step;
                if (ImageFilling <= 0)
                {
                    isFreshlyOccupied = true;
                    currentStatus.Value = allySide;
                    isRestoring.Value = false;
                    
                }
            }
        }

        private void Start()
        {
            isFreshlyOccupied = true;
        }

        private void OnStatusChanged(Belonging prev, Belonging current)
        {
            Debug.Log($"State of key point with ID {NetworkObjectId} is {current}");
            ChangeImageColorAccordingToSide(current);
        }

        public override void OnNetworkSpawn()
        {
            ChangeImageColorAccordingToSide(currentStatus.Value);
            imagePoint.fillAmount = imageFilling.Value;
        }

        private void ChangeImageColorAccordingToSide(Belonging belonging)
        {
            if (belonging == Belonging.Blue || belonging == Belonging.Blue + TO_FULL_BELONGING_SHIFT)
            {
                imagePoint.color = blueTeam;
            }
            else
            {
                imagePoint.color = redTeam;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.TryGetComponent(out Teammate teammate))
            {
                if (teammate.IsLocalPlayer)
                {
                    CalculateNumberOfTeammatesServerRpc(teammate.Side, false);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.TryGetComponent(out Teammate teammate))
            {
                if (teammate.IsLocalPlayer)
                {
                    CalculateNumberOfTeammatesServerRpc(teammate.Side, true);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void CalculateNumberOfTeammatesServerRpc(Belonging teammateSide, bool isLeaving)
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

        public override void OnDestroy()
        {
            currentStatus.OnValueChanged -= OnStatusChanged;
            base.OnDestroy();
        }

        private float ImageFilling
        {
            get => imageFilling.Value;
            set
            {
                /* Debug Code
                if (imageFilling.Value == 0f)
                {
                    startOfCapture = DateTime.Now;
                    numberOfSteps = 0;
                }
                else if (imageFilling.Value == 1f && debug)
                {
                    debug = false;
                    Debug.Log("Time To Capture: " + (DateTime.Now - startOfCapture).TotalSeconds);
                    Debug.Log("Number of Steps: " + numberOfSteps);
                    numberOfSteps = 0;
                }
                */
                if (value < 0)
                {
                    value = 0;
                    if (currentStatus.Value == Belonging.FullRed || currentStatus.Value == Belonging.FullBlue)
                    {
                        currentStatus.Value -= TO_FULL_BELONGING_SHIFT;
                    }
                }
                else if (value >= 1)
                {
                    value = 1;
                    if (currentStatus.Value == Belonging.Red || currentStatus.Value == Belonging.Blue)
                    {
                        Connection.ConnectionHandler.Instance.GameOver(currentStatus.Value);
                        currentStatus.Value += TO_FULL_BELONGING_SHIFT;
                    }
                }
                imageFilling.Value = value;
            }
        }
    }
}