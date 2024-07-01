using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Weapons
{
    public abstract class DistanceWeapon : Weapon
    {
        [SerializeField] protected Transform barrelEnd;
        [SerializeField] private string nameOfReloading;
        [SerializeField] private int maxMagazineAmount;
        [SerializeField] private int totalAmmo;
        [SerializeField] private int currentMagazine;
        [SerializeField]
        [Tooltip("Time is exressed in seconds")]
        private float reloadingMagazinePeriod;
        [SerializeField] private bool isInfiniteAmmo;
        private bool isMagazineReloading;
        public event UnityAction<string, ulong> MagazineReloaded;

        protected override bool TryAttack()
        {
            if (isInfiniteAmmo || isMagazineReloading == false && currentMagazine > 0)
            {
                --currentMagazine;
                Shoot();
                return true;
            }
            return false;
        }

        [ServerRpc(RequireOwnership = false)]
        public void ReloadMagazineServerRpc()
        {
            if (isInfiniteAmmo == false && totalAmmo > 0)
            {
                MagazineReloaded?.Invoke(nameOfReloading, NetworkObjectId);
                totalAmmo += currentMagazine;
                currentMagazine = (totalAmmo >= maxMagazineAmount) ? maxMagazineAmount : totalAmmo;
                totalAmmo -= maxMagazineAmount;
                if (totalAmmo < 0)
                    totalAmmo = 0;
            }
        }

        protected abstract void Shoot();

        public Transform BarrelEnd => barrelEnd;

        protected int Magazine => maxMagazineAmount;
    }
}


