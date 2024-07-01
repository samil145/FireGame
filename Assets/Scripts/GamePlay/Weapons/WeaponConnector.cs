using UnityEngine.Events;

namespace Gameplay.Weapons
{
    public class WeaponConnector
    {
        private WeaponController controller;
        private UnityAction<string, ulong> attackHandler;
        private UnityAction<string, ulong> reloadingHandler;

        public WeaponConnector(WeaponController controller, UnityAction<string, ulong> attackHandler)
        {
            this.attackHandler = attackHandler;
            this.controller = controller;
            this.controller.WeaponChanched += OnPickedUp;
            AttachWeapon(controller.CurrentWeapon);
        }

        public WeaponConnector(WeaponController controller, UnityAction<string, ulong> attackHandler, UnityAction<string,ulong> reloadingHandler) : this(controller, attackHandler)
        {
            this.reloadingHandler = reloadingHandler;
            DetachWeapon(controller.CurrentWeapon);
            AttachWeapon(controller.CurrentWeapon);
        }

        private void OnPickedUp(Weapon previouWeapon)
        {
            DetachWeapon(previouWeapon);
            AttachWeapon(controller.CurrentWeapon);
        }

        private void AttachWeapon(Weapon weapon)
        {
            if (weapon)
            {
                weapon.Attacked += attackHandler;
                DistanceWeapon distanceWeapon = weapon as DistanceWeapon;
                if (distanceWeapon != null)
                {
                    distanceWeapon.MagazineReloaded += reloadingHandler;
                }
            }
        }

        public void Disconnect()
        {
            controller.WeaponChanched -= OnPickedUp;
            DetachWeapon(controller.CurrentWeapon);
            attackHandler = null;
            reloadingHandler = null;
        }

        private void DetachWeapon(Weapon weapon)
        {
            if (weapon)
            {
                weapon.Attacked -= attackHandler;
                DistanceWeapon distanceWeapon = weapon as DistanceWeapon;
                if (distanceWeapon != null)
                {
                    distanceWeapon.MagazineReloaded -= reloadingHandler;
                }
            }
        }
    }
}
