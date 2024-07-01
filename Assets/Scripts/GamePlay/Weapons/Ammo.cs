using CustomAttributes;
using Gameplay.Creatures;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Weapons
{
    public class Ammo : NetworkBehaviour, IPooledObject
    {
        [SerializeField] private int damage;
        private GameObject parent;
        private DistanceWeapon parentWeapon;
        [SerializeField] private float speed;
        private NetworkVariable<bool> isActive;
        private Belonging side;
        [SerializeField] private float maxDistance;
        [SerializeField]
        [ReadOnlyField]
        private float distanceFromOrigin;

        private void Awake()
        {
            isActive = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
            isActive.OnValueChanged += SetActive;
        }

        private void SetActive(bool prev, bool current)
        {
            gameObject.SetActive(current);
        }

        private void Start()
        {
            gameObject.SetActive(isActive.Value);
        }

        private void FixedUpdate()
        {
            if (isActiveAndEnabled && IsServer)
            {
                Fly(NetworkObjectId);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("Hitted");

            if (other.TryGetComponent(out ILivingCreature creature) == false)
            {
                return;
            }
            if (other.TryGetComponent(out Teammate teammate) == false || teammate.Side != side)
            {
                creature.HP -= damage;
                side = Belonging.None;
                PushBack();
            }
        }

        public void BecomeActive()
        {
            isActive.Value = true;
            transform.parent = null;            
            transform.position = parentWeapon.BarrelEnd.position;
            transform.rotation = parentWeapon.BarrelEnd.rotation;
        }

        private void Fly(ulong objId)
        {
            Ammo ammo = GetNetworkObject(objId).GetComponent<Ammo>();
            float speed = ammo.speed;
            ammo.transform.position += ammo.transform.forward * speed * Time.fixedDeltaTime;
            ammo.distanceFromOrigin += speed * Time.fixedDeltaTime;
            if (ammo.maxDistance <= ammo.distanceFromOrigin)
            {
                ammo.distanceFromOrigin = 0f;
                ammo.PushBack();
            }
        }

        public void PushBack()
        {
            isActive.Value = false;
            NetworkObject.TrySetParent(GeneralPooling.Instance.transform);
        }

        public GameObject Parent
        {
            get => parent;
            set
            {
                if (parent == null)
                {
                    if (value.TryGetComponent(out DistanceWeapon weaponParent))
                    {
                        this.parentWeapon = weaponParent;
                        parent = value;
                    }
                    else
                    {
                        parent = null;
                    }
                }
            }
        }
        private GameObject FindParentWithNetworkObject(GameObject gameObject)
        {
            if (gameObject.TryGetComponent(out NetworkObject parent))
            {
                return parent.gameObject;
            }
            else
            {
                FindParentWithNetworkObject(gameObject.transform.parent.gameObject);
            }
            return null;
        }

        public Belonging Side { get => side; set => side = value; }
        public bool IsPushedBack { get => isActive.Value; }
    }
}


