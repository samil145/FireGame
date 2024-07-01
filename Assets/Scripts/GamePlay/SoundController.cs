using Unity.Netcode;
using UnityEngine;

namespace Gameplay
{
    using Weapons;

    [RequireComponent(typeof(AudioSource))]
    public class SoundController : NetworkBehaviour
    {
        private WeaponConnector weaponConnector;
        private AudioSource audioSource;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            if (TryGetComponent(out WeaponController weaponController))
            {
                weaponConnector = new WeaponConnector(weaponController, OnWeaponAttacked);
            }
        }

        private void OnWeaponAttacked(string attackName, ulong weaponId)
        {
            if (IsServer)
            {
                OnWeaponAttackedClientRpc(attackName, weaponId);
            }
        }

        [ClientRpc]
        private void OnWeaponAttackedClientRpc(string attackName, ulong weaponId)
        {
            Debug.Log($"[Client] Weapon sound of {NetworkObject.OwnerClientId} Player recived the following: {attackName}");
            PlayWeaponSound(attackName, weaponId);
        }

        private void PlayWeaponSound(string attackName, ulong weaponId)
        {
            NetworkObject weapon = GetNetworkObject(weaponId);
            if (weapon.TryGetComponent(out AudioSource audioSource))
            {
                audioSource.PlayOneShot(SoundsDB.Instance.AudioClips[attackName]);
            }
        }

        public override void OnDestroy()
        {
            weaponConnector.Disconnect();
            base.OnDestroy();
        }
    }
}
