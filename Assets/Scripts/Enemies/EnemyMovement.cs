using ShadowDash.Player;
using UnityEngine;

namespace ShadowDash.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(EnemyPathfinder2D))]
    public sealed class EnemyMovement : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float moveSpeed = 2.2f;
        [SerializeField] private float stopDistance = 0.75f;

        private Rigidbody2D body;
        private EnemyPathfinder2D pathfinder;
        private PlayerHealth playerHealth;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            pathfinder = GetComponent<EnemyPathfinder2D>();
            body.gravityScale = 0f;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
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

            Vector2 moveTarget = pathfinder.GetMoveTarget(body.position, target);
            Vector2 toTarget = moveTarget - body.position;
            float distanceToTarget = toTarget.magnitude;
            float stoppingDistance = pathfinder.IsUsingDirectPath ? stopDistance : pathfinder.WaypointReachDistance;
            if (distanceToTarget <= stoppingDistance)
            {
                body.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 desiredDirection = toTarget / distanceToTarget;
            body.linearVelocity = desiredDirection * moveSpeed;
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
