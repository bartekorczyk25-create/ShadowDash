using ShadowDash.Player;
using UnityEngine;

namespace ShadowDash.Combat
{
    public sealed class EnemyProjectile2D : MonoBehaviour
    {
        [SerializeField] private float speed = 7f;
        [SerializeField] private int damage = 1;
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private float hitRadius = 0.25f;
        [SerializeField] private LayerMask playerLayerMask = ~0;

        private Vector2 direction = Vector2.left;
        private float lifeRemaining;

        private void Awake()
        {
            lifeRemaining = lifetime;
        }

        public void Initialize(Vector2 travelDirection, int projectileDamage, float projectileSpeed)
        {
            if (travelDirection.sqrMagnitude > 0.001f)
            {
                direction = travelDirection.normalized;
            }

            damage = projectileDamage;
            speed = projectileSpeed;
        }

        private void Update()
        {
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
            lifeRemaining -= Time.deltaTime;

            if (TryHitPlayer() || lifeRemaining <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private bool TryHitPlayer()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, hitRadius, playerLayerMask);
            foreach (Collider2D hit in hits)
            {
                if (hit.TryGetComponent(out PlayerHealth playerHealth) && playerHealth.IsAlive)
                {
                    playerHealth.TakeDamage(damage);
                    return true;
                }
            }

            return false;
        }
    }
}
