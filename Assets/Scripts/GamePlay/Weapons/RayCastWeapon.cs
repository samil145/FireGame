using UnityEngine;

namespace Gameplay.Weapons
{
    using Creatures;

    public sealed class RayCastWeapon : DistanceWeapon
    {
        [SerializeField] private int damage;
        private Ray attackRay;
        [Range(0f, 1000f)]
        [SerializeField]
        private float distance;

        private new void Start()
        {
            base.Start();
            attackRay = new Ray();
        }

        private void Update()
        {
            attackRay.direction = transform.forward;
            attackRay.origin = barrelEnd.position;
        }

        protected override void Shoot()
        {
            if (Physics.Raycast(attackRay, out RaycastHit hit, distance))
            {
                ILivingCreature enemy = hit.collider.gameObject.GetComponent<ILivingCreature>();
                enemy.HP -= damage;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 initialPosition = barrelEnd.position;
            Vector3 targetPoint = barrelEnd.position + barrelEnd.forward * distance;
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(initialPosition, 0.05f);
            Gizmos.DrawSphere(targetPoint, 0.05f);
            Gizmos.DrawLine(initialPosition, targetPoint);
        }
    }
}

