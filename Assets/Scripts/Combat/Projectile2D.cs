using ShadowDash.Enemies;
using UnityEngine;

namespace ShadowDash.Combat
{
    public sealed class Projectile2D : MonoBehaviour
    {
        [SerializeField] private float speed = 12f;
        [SerializeField] private int damage = 1;
        [SerializeField] private float lifetime = 2f;
        [SerializeField] private float hitRadius = 0.2f;
        [SerializeField] private LayerMask enemyLayerMask = ~0;

        private Vector2 direction = Vector2.right;
        private float lifeRemaining;

        private void Awake()
        {
            lifeRemaining = lifetime;
        }

        public void Initialize(Vector2 travelDirection, int projectileDamage)
        {
            if (travelDirection.sqrMagnitude > 0.001f)
            {
                direction = travelDirection.normalized;
            }

            damage = projectileDamage;
        }

        private void Update()
        {
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
            lifeRemaining -= Time.deltaTime;

            if (TryHitEnemy() || lifeRemaining <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private bool TryHitEnemy()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, hitRadius, enemyLayerMask);
            foreach (Collider2D hit in hits)
            {
                if (hit.TryGetComponent(out EnemyHealth enemy) && enemy.IsAlive)
                {
                    enemy.TakeDamage(damage);
                    return true;
                }
            }

            return false;
        }
    }
}
