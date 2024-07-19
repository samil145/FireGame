using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Boosters
{
    public class HealthBooster : Booster
    {
        [SerializeField] private int healthBoost;

        protected override void ApplyModifier(NetworkObject player)
        {
            Teammate teammate = player.GetComponent<Teammate>();
            teammate.HP += healthBoost;
        }
    }
}
