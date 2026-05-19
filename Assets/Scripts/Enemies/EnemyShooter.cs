using ShadowDash.Combat;
using ShadowDash.Player;
using UnityEngine;

namespace ShadowDash.Enemies
{
    public sealed class EnemyShooter : MonoBehaviour
    {
        [SerializeField] private EnemyProjectile2D projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float fireRate = 0.75f;
        [SerializeField] private float range = 7f;
        [SerializeField] private int projectileDamage = 1;
        [SerializeField] private float projectileSpeed = 7f;

        private PlayerHealth target;
        private float nextFireTime;

        private void Awake()
        {
            if (firePoint == null)
            {
                firePoint = transform;
            }
        }

        private void Update()
        {
            ResolveTarget();

            if (projectilePrefab == null || target == null || !target.IsAlive || Time.time < nextFireTime)
            {
                return;
            }

            Vector2 toTarget = (Vector2)target.transform.position - (Vector2)firePoint.position;
            if (toTarget.sqrMagnitude > range * range)
            {
                return;
            }

            EnemyProjectile2D projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            projectile.Initialize(toTarget.normalized, projectileDamage, projectileSpeed);
            nextFireTime = Time.time + 1f / fireRate;
        }

        private void ResolveTarget()
        {
            if (target != null)
            {
                return;
            }

            target = FindAnyObjectByType<PlayerHealth>();
        }
    }
}
