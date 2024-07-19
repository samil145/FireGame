using Unity.Netcode;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

namespace Gameplay.Boosters
{
    public class SpeedBooster : Booster
    {
        [SerializeField][Range(1f, 5f)] private float speedMultiplier;

        protected override void ApplyModifier(NetworkObject player)
        {
            MovementRigidbody movementRigidbody = player.GetComponent<MovementRigidbody>();
            movementRigidbody.IncreaseSpeed(speedMultiplier);
        }
    }
}
