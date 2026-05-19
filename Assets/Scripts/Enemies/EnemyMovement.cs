using ShadowDash.Player;
using UnityEngine;

namespace ShadowDash.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class EnemyMovement : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float moveSpeed = 2.2f;
        [SerializeField] private float stopDistance = 0.75f;

        private Rigidbody2D body;
        private PlayerHealth playerHealth;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
        }

        private void Start()
        {
            ResolveTarget();
        }

        private void FixedUpdate()
        {
            ResolveTarget();

            if (target == null || playerHealth == null || !playerHealth.IsAlive)
            {
                body.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 toTarget = (Vector2)target.position - body.position;
            if (toTarget.sqrMagnitude <= stopDistance * stopDistance)
            {
                body.linearVelocity = Vector2.zero;
                return;
            }

            body.linearVelocity = toTarget.normalized * moveSpeed;
        }

        private void ResolveTarget()
        {
            if (target != null && playerHealth != null)
            {
                return;
            }

            playerHealth = FindAnyObjectByType<PlayerHealth>();
            if (playerHealth != null)
            {
                target = playerHealth.transform;
            }
        }
    }
}
