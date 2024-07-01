using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Weapons
{
    public class WeaponController : NetworkBehaviour
    {
        [SerializeField] private List<Weapon> weaponsPrefabs;
        [SerializeField] private Transform weaponPosition;
        [SerializeField]
        [CustomAttributes.ReadOnlyField]
        private Weapon currentWeapon;
        private KeyCode attack;
        private NetworkVariable<int> currentWeaponId;
        private NetworkList<ulong> weaponsObjIds;

        private void Awake()
        {
            weaponsObjIds = new NetworkList<ulong>();
            currentWeaponId = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        }

        public override void OnNetworkSpawn()
        {
            currentWeaponId.OnValueChanged += SwapWeapon;
        }

        private void SwapWeapon(int prev, int current)
        {
            if (prev < weaponsObjIds.Count && current < weaponsObjIds.Count)
            {
                Debug.Log($"Swaped weapon\nPrev: {prev}\tCurrent: {current}");
                DisablePreviousWeapon(prev);
                ChangeCurrentWeapon(current);
            }
        }

        private void DisablePreviousWeapon(int id)
        {
            if (id != -1)
            {
                Weapon weapon = GetWeaponById(id);
                weapon.DropShootEvent();
                weapon.IsActive = false;
            }
        }

        private void ChangeCurrentWeapon(int id)
        {
            if (id == -1)
                return;
            Weapon previousWeapon = currentWeapon;
            currentWeapon = GetWeaponById(id);
            WeaponChanched?.Invoke(previousWeapon);
            currentWeapon.IsActive = true;
            currentWeapon.transform.position = weaponPosition.position;
            currentWeapon.transform.rotation = transform.rotation;
            currentWeapon.NetworkObject.TrySetParent(transform);
            currentWeapon.HolderArm = weaponPosition;
            if (currentWeapon.TryGetComponent(out Rigidbody weaponRig))
            {
                weaponRig.freezeRotation = true;
                weaponRig.constraints = RigidbodyConstraints.FreezeAll;
            }
        }

        private void Start()
        {
            attack = KeyCode.G;
            if (IsLocalPlayer)
            {
                SpawnGunsServerRpc(NetworkManager.LocalClientId);
                SwapWeaponServerRpc(0, NetworkManager.LocalClientId);
            }
            if ((currentWeaponId?.Value ?? -1) != -1)
                currentWeapon = GetWeaponById(currentWeaponId.Value);
        }

        private Weapon GetWeaponById(int weaponID) => GetNetworkObject(weaponsObjIds[weaponID]).GetComponent<Weapon>();

        [ServerRpc(RequireOwnership = false)]
        private void SpawnGunsServerRpc(ulong clientId)
        {
            if (NetworkManager.ConnectedClients.ContainsKey(clientId))
            {
                NetworkClient client = NetworkManager.ConnectedClients[clientId];
                WeaponController controller = client.PlayerObject.GetComponent<WeaponController>();
                Transform clientTransform = client.PlayerObject.transform;
                Teammate clientMate = client.PlayerObject.GetComponent<Teammate>();
                for (int i = 0; i < weaponsPrefabs.Count; i++)
                {
                    Weapon weapon = Instantiate(weaponsPrefabs[i], transform.position, transform.rotation);
                    weapon.NetworkObject.SpawnWithOwnership(clientId);
                    weapon.NetworkObject.TrySetParent(clientTransform);
                    weapon.NetworkObject.DontDestroyWithOwner = false;
                    weapon.IsActive = false;
                    weapon.Side = clientMate.Side;
                    controller.weaponsObjIds.Add(weapon.NetworkObjectId);
                }
            }
        }

        public override void OnDestroy()
        {
            currentWeaponId.OnValueChanged -= SwapWeapon;
            base.OnDestroy();
        }

        public override void OnNetworkDespawn()
        {
            if (IsClient)
            {
                DispawnChildrenServerRpc(OwnerClientId);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void DispawnChildrenServerRpc(ulong clientId)
        {
            if (NetworkManager.ConnectedClients.ContainsKey(clientId))
            {
                NetworkClient client = NetworkManager.ConnectedClients[clientId];
                foreach (NetworkObject child in client.OwnedObjects)
                {
                    child.Despawn();
                }
            }
        }

        private void OnGUI()
        {
            if (Connection.ConnectionHandler.Instance.IsGameStarted == false || IsLocalPlayer == false)
                return;
            Event currentInput = Event.current;
            if (currentInput.isKey)
            {
                KeyCode currentKey = currentInput.keyCode;
                if (currentKey != KeyCode.None && currentKey != KeyCode.W && currentKey != KeyCode.S && currentKey != KeyCode.A && currentKey != KeyCode.D)
                {
                    if (currentKey == attack && currentWeaponId.Value != -1)
                    {
                        currentWeapon ??= GetWeaponById(currentWeaponId.Value);
                        currentWeapon?.AttackServerRpc();
                    }
                    if (KeyCode.Alpha1 <= currentKey && currentKey <= KeyCode.Alpha9 && currentKey - KeyCode.Alpha1 < weaponsObjIds.Count)
                    {
                        SwapWeaponServerRpc(currentKey - KeyCode.Alpha1, NetworkManager.LocalClientId);
                    }
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void SwapWeaponServerRpc(int weaponId, ulong clietId)
        {
            if (NetworkManager.ConnectedClients.ContainsKey(clietId))
            {
                NetworkClient client = NetworkManager.ConnectedClients[clietId];
                WeaponController controller = client.PlayerObject.GetComponent<WeaponController>();
                controller.currentWeaponId.Value = weaponId;
            }
        }

        public event UnityAction<Weapon> WeaponChanched;

        public Weapon CurrentWeapon => currentWeapon;

        public KeyCode MagicAttack
        {
            get => attack;
            set => attack = value;
        }
    }
}
