using Cinemachine;
using Connection;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityStandardAssets.Characters.ThirdPerson;

namespace Gameplay
{
    public class Teammate : NetworkBehaviour
    {
        private NetworkVariable<Belonging> side;
        [SerializeField]
        public Charachter charachter;
        private NetworkVariable<int> currenthp;
        private NetworkVariable<int> dm;
        private NetworkVariable<int> sp;
        private NetworkVariable<int> ul;
        [SerializeField] private FireballLogic go;
        [SerializeField] private FireballLogic ugo;
        [SerializeField] private Transform hand;
        private Animator m_Animator;
        [SerializeField] private float ultaMultipicator;
        private NetworkVariable<int> ulta;
        float damageAngle;


        public void StopAtack(int a) => m_Animator.SetInteger("Atack", 0);

        public void StartAtack(int a)
        {
            if (a == 2)
            {
                if (m_Animator.GetInteger("Atack") == 2)
                {
                    var obj = CreateFireball(true);
                    UiController.Instance.charachtersUl[OwnerClientId].fillAmount = 0f;
                }
            }
            else if (a == 1)
                if (m_Animator.GetInteger("Atack") == 1)
                    CreateFireball(false);
        }

        public void StopImpact(int a)
        {
            if (m_Animator.GetInteger("Damage") != 2)
                m_Animator.SetInteger("Damage", 0);
        }

        private FireballLogic CreateFireball(bool isUlta)
        {
            var obj = Instantiate(isUlta ? ugo : go, hand.position, Quaternion.identity);
            obj.Damage = isUlta ? dm.Value * 3 : dm.Value;
            obj.Side = side.Value;
            obj.transform.forward = transform.forward;
            return obj;
        }

        private void Awake()
        {
            side = new NetworkVariable<Belonging>(Belonging.Blue, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
            currenthp = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
            ulta = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
            dm = new NetworkVariable<int>(charachter.Damage, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
            sp = new NetworkVariable<int>(charachter.Speed, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
            ul = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
            currenthp.OnValueChanged += OnHpChanged;
            ulta.OnValueChanged += OnUltimateChanged;
            side.OnValueChanged += OnSideChange;
            m_Animator = GetComponent<Animator>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsLocalPlayer)
                GetClientDataServerRpc(NetworkManager.LocalClientId);
            rimLightSet(side.Value);
        }

        private void OnSideChange(Belonging pr, Belonging c)
        {
            var a = GetComponentsInChildren<Renderer>();
            rimLightSet(c);
        }

        [ServerRpc(RequireOwnership = false)]
        private void GetClientDataServerRpc(ulong id)
        {
            if (NetworkManager.ConnectedClients.ContainsKey(id))
            {
                UserData userData = ConnectionHandler.Instance.ClientsData[id];
                NetworkClient client = NetworkManager.ConnectedClients[id];
                Teammate teammate = client.PlayerObject.GetComponent<Teammate>();
                teammate.side.Value = userData.side;
                Debug.Log(teammate.side.Value);
            }
        }

        private void OnUltimateChanged(int prev, int current) => UiController.Instance.UpdateUlta(OwnerClientId, (float)current / 100f);

        private void OnHpChanged(int prev, int current)
        {
            Debug.Log($"Previous: {prev}\tCurrent: {current}  {current / 100f}");
            UiController.Instance.UpdateHp(OwnerClientId, current / 100f);
            if (current <= 0)
            {
                m_Animator.SetInteger("Damage", 2);
                if (IsClient && IsOwner)
                {
                    gameObject.GetComponent<MovementRigidbody>().m_Rigidbody.constraints = RigidbodyConstraints.FreezePosition;
                }
                if (IsServer)
                    ConnectionHandler.Instance.CheckIfTeamIsDead(OwnerClientId);
            }
        }

        public Belonging Side { get => side.Value; set => side.Value = value; }

        public int HP
        {
            get => currenthp.Value;
            set
            {
                if (IsServer)
                    currenthp.Value = Mathf.Clamp(value, 0, 100);
                else
                    ChangeCurrentHealthServerRpc(value);
            }
        }



        [ServerRpc(RequireOwnership = false)]
        private void ChangeCurrentHealthServerRpc(int value)
        {
            currenthp.Value = Mathf.Clamp(value, 0, 100);
        }

        public int Damage
        {
            get => dm.Value;
            set
            {
                if (IsServer)
                    dm.Value = value;
                else
                    ChangeDamageServerRpc(value);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void ChangeDamageServerRpc(int value) => Damage = value;

        public int Ulta
        {
            get => ulta.Value;
            set
            {
                if (IsServer)
                    ulta.Value = Mathf.Clamp(value, 0, 100);
                else
                    ChangeUltaServerRpc(value);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void ChangeUltaServerRpc(int value) => Ulta = value;


        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Fireball")
            {
                var fl = other.gameObject.GetComponent<FireballLogic>();
                if (fl.Side != side.Value)
                {
                    Ulta += (int)(fl.Damage * ultaMultipicator);
                    currenthp.Value = currenthp.Value - fl.Damage / 3;
                    m_Animator.SetInteger("Damage", currenthp.Value == 0 ? 2 : 1);
                    //if (currenthp.Value != 0)
                    //{
                    //    m_Animator.SetFloat("Impact", damageAngle);
                    //}
                }
                Vector3 forward = transform.TransformDirection(Vector3.forward);
                Vector3 toOther = other.transform.position - transform.position;
                damageAngle = Vector3.Dot(forward, toOther);
            }
        }

        private void rimLightSet(Belonging c)
        {
            if (IsClient)
            {
                var a = GetComponentsInChildren<Renderer>();
                for (int i = 0; i < a.Length; i++)
                {
                    for (int j = 0; j < a[i].materials.Length; j++)
                    {
                        if (c == Belonging.Red)
                            a[i].materials[j].SetColor("_RimLightColor", Color.red);
                        else if (c == Belonging.Blue)
                            a[i].materials[j].SetColor("_RimLightColor", Color.blue);
                        if (!IsOwner)
                        {
                            a[i].materials[j].SetFloat("_RimLight_Power", 0.6f);
                        }
                    }

                }
            }
        }


        public override void OnDestroy()
        {
            currenthp.OnValueChanged -= OnHpChanged;
            base.OnDestroy();
        }
    }
}

