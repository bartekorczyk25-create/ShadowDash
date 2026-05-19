using ShadowDash.Enemies;
using UnityEngine;

namespace ShadowDash.Combat
{
    [RequireComponent(typeof(EnemyTargeting))]
    public sealed class PlayerAutoShooter : MonoBehaviour
    {
        [SerializeField] private Projectile2D projectilePrefab = null;
        [SerializeField] private Transform firePoint = null;
        [SerializeField] private float fireRate = 1.5f;
        [SerializeField] private float range = 8f;
        [SerializeField] private int projectileDamage = 1;

        private EnemyTargeting targeting;
        private float nextFireTime;

        private void Awake()
        {
            targeting = GetComponent<EnemyTargeting>();

            if (firePoint == null)
            {
                firePoint = transform;
            }
        }

        private void Update()
        {
            if (projectilePrefab == null || Time.time < nextFireTime)
            {
                return;
            }

            EnemyHealth target = targeting.FindNearestEnemy(transform.position, range);
            if (target == null)
            {
                return;
            }

            Vector2 direction = ((Vector2)target.transform.position - (Vector2)firePoint.position).normalized;
            Projectile2D projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            projectile.Initialize(direction, projectileDamage);
            nextFireTime = Time.time + 1f / fireRate;
        }
    }
}
