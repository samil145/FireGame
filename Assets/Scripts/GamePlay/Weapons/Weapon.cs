using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Weapons
{
    public abstract class Weapon : NetworkBehaviour
    {
        [SerializeField] private string nameOfAttack;
        [SerializeField]
        [Tooltip("Time is exressed in seconds")]
        private float reloadingTime;
        private bool isReloading;
        public event UnityAction<string, ulong> Attacked;
        private NetworkVariable<bool> isActive;
        private Transform holderArm;
        protected Belonging side;

        private void Awake()
        {
            isActive = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
            isActive.OnValueChanged += SetActive;
        }

        private void FixedUpdate()
        {
            if (holderArm != null && IsClient)
            {
                ChangePositionAccordingToHolderPos(NetworkObjectId, holderArm.position, holderArm.rotation);
            }
        }

        private void ChangePositionAccordingToHolderPos(ulong id, Vector3 pos, Quaternion rotation)
        {
            NetworkObject weapon = GetNetworkObject(id);
            weapon.transform.position = pos;
            weapon.transform.rotation = rotation;
        }

        private void SetActive(bool prev, bool current)
        {
            gameObject.SetActive(current);
        }

        [ServerRpc(RequireOwnership = false)]
        public virtual void AttackServerRpc()
        {
            if (isReloading)
                return;
            if (TryAttack())
            {
                Attacked?.Invoke(nameOfAttack, NetworkObjectId);
                isReloading = true;
                NetworkManager.StartCoroutine(ReloadCoroutine());
            }
        }

        protected virtual void Start()
        {
            Debug.Log($"Weapon {gameObject.name}; ID {NetworkObjectId} is {isActive.Value}");
            gameObject.SetActive(isActive.Value);
        }

        protected abstract bool TryAttack();

        private IEnumerator ReloadCoroutine()
        {
            yield return new WaitForSeconds(reloadingTime);
            isReloading = false;
        }

        public void DropShootEvent()
        {
            if (Attacked != null)
            {
                Delegate[] methods = Attacked.GetInvocationList();
                for (int i = 0; i < methods.Length; i++)
                {
                    Attacked -= methods[i] as UnityAction<string, ulong>;
                }
            }
        }

        public string AttackName => nameOfAttack;

        public bool IsActive
        {
            get => isActive.Value;
            set
            {
                if (IsClient)
                {
                    SetActiveServerRpc(value, NetworkObjectId);
                }
                else
                {
                    isActive.Value = value;
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetActiveServerRpc(bool state, ulong weaponId)
        {
            NetworkObject weaponObj = GetNetworkObject(weaponId);
            if (weaponObj)
            {
                Weapon weapon = weaponObj.GetComponent<Weapon>();
                weapon.isActive.Value = state;
            }
        }

        public Transform HolderArm { get => holderArm; set => holderArm = value; }

        public Belonging Side { get => side; set => side = value; }
    }
}
