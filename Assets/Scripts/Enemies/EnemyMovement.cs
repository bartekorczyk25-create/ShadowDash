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
        [SerializeField] private float obstacleCheckDistance = 0.85f;
        [SerializeField] private float steeringDuration = 0.45f;
        [SerializeField] private float unstuckTime = 0.75f;
        [SerializeField] private LayerMask obstacleLayerMask = ~0;

        private Rigidbody2D body;
        private PlayerHealth playerHealth;
        private readonly RaycastHit2D[] castHits = new RaycastHit2D[8];
        private ContactFilter2D obstacleFilter;
        private Vector2 steeringDirection;
        private Vector2 lastStuckCheckPosition;
        private float steeringTimeRemaining;
        private float stuckCheckTimer;
        private int lastSteeringSide = 1;

        private const float StuckMoveThreshold = 0.08f;
        private const float SteeringBlend = 1.1f;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            obstacleFilter = new ContactFilter2D
            {
                useLayerMask = true,
                useTriggers = false
            };
            obstacleFilter.SetLayerMask(obstacleLayerMask);
            lastStuckCheckPosition = body.position;
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

            Vector2 desiredDirection = toTarget.normalized;
            Vector2 moveDirection = GetMoveDirection(desiredDirection);
            body.linearVelocity = moveDirection * moveSpeed;
            UpdateUnstuckCheck(desiredDirection);
        }

        private Vector2 GetMoveDirection(Vector2 desiredDirection)
        {
            if (steeringTimeRemaining > 0f)
            {
                steeringTimeRemaining -= Time.fixedDeltaTime;
                return steeringDirection;
            }

            if (IsObstacleAhead(desiredDirection))
            {
                BeginSteering(desiredDirection);
                return steeringDirection;
            }

            return desiredDirection;
        }

        private void BeginSteering(Vector2 desiredDirection)
        {
            Vector2 leftDirection = BuildSteeringDirection(desiredDirection, 1);
            Vector2 rightDirection = BuildSteeringDirection(desiredDirection, -1);
            bool leftBlocked = IsObstacleAhead(leftDirection);
            bool rightBlocked = IsObstacleAhead(rightDirection);

            if (leftBlocked != rightBlocked)
            {
                lastSteeringSide = leftBlocked ? -1 : 1;
            }

            steeringDirection = BuildSteeringDirection(desiredDirection, lastSteeringSide);
            steeringTimeRemaining = steeringDuration;
        }

        private Vector2 BuildSteeringDirection(Vector2 desiredDirection, int side)
        {
            Vector2 perpendicular = new Vector2(-desiredDirection.y, desiredDirection.x) * side;
            return (desiredDirection + perpendicular * SteeringBlend).normalized;
        }

        private bool IsObstacleAhead(Vector2 direction)
        {
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return false;
            }

            int hitCount = body.Cast(direction, obstacleFilter, castHits, obstacleCheckDistance);
            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hitCollider = castHits[i].collider;
                if (hitCollider == null)
                {
                    continue;
                }

                if (hitCollider.GetComponentInParent<PlayerHealth>() != null ||
                    hitCollider.GetComponentInParent<EnemyHealth>() != null)
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        private void UpdateUnstuckCheck(Vector2 desiredDirection)
        {
            stuckCheckTimer += Time.fixedDeltaTime;
            if (stuckCheckTimer < unstuckTime)
            {
                return;
            }

            float movedDistance = Vector2.Distance(body.position, lastStuckCheckPosition);
            if (movedDistance < StuckMoveThreshold)
            {
                lastSteeringSide *= -1;
                BeginSteering(desiredDirection);
            }

            stuckCheckTimer = 0f;
            lastStuckCheckPosition = body.position;
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
