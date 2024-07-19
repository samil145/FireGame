using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Boosters
{
    public class UltimateBooster : Booster
    {
        [SerializeField] private int ultimateBooster;

        protected override void ApplyModifier(NetworkObject player)
        {
            //Future
        }
    }
}
