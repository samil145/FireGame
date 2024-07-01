using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Creatures
{
    public class CreatureTest : NetworkBehaviour, ILivingCreature
    {
        [SerializeField] 
        private int maxHp;
        private NetworkVariable<int> hp;
        [SerializeField]
        [CustomAttributes.ReadOnlyField]
        private int currentHp;

        private void Awake()
        {
            hp = new NetworkVariable<int>(maxHp, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
            hp.OnValueChanged += InspectHP;
        }

        private void InspectHP(int prev, int current)
        {
            Debug.Log($"Previous: {prev}\tCurrent: {current}");
            currentHp = current;
        }

        public int HP 
        { 
            get => hp.Value;
            set
            {
                if (IsServer)
                {
                    DecreaseHP(value);
                }
            }
        }

        public int MaxHP { get => maxHp; set => maxHp = value; }

        private void DecreaseHP(int value)
        {
            if (value <= 0)
            {
                hp.Value = 0;
                Debug.Log("Died");
            }
            else
            {
                hp.Value = value;
                Debug.Log($"HP:\t{hp.Value}");
            }
        }
    }
}

