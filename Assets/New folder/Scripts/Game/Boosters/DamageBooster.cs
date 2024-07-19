using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Boosters
{
    public class DamageBooster : Booster
    {
        [SerializeField] private int damageBooster;

        protected override void ApplyModifier(NetworkObject player)
        {
            Teammate teammate = player.GetComponent<Teammate>();
            teammate.Damage += damageBooster;
        }
    }
}
