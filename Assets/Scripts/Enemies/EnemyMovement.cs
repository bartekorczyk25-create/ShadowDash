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
        [SerializeField] private float cornerRetryTime = 0.35f;
        [SerializeField] private float strongSteeringDuration = 0.7f;
        [SerializeField] private LayerMask obstacleLayerMask = ~0;

        private Rigidbody2D body;
        private PlayerHealth playerHealth;
        private readonly RaycastHit2D[] castHits = new RaycastHit2D[8];
        private ContactFilter2D obstacleFilter;
        private Vector2 steeringDirection;
        private Vector2 lastStuckCheckPosition;
        private float steeringTimeRemaining;
        private float stuckCheckTimer;
        private float cornerRetryTimer;
        private float lastDistanceToTarget;
        private bool blockedThisStep;
        private int lastSteeringSide = 1;

        private static int nextSteeringSide = 1;

        private const float StuckMoveThreshold = 0.08f;
        private const float ProgressThreshold = 0.08f;
        private const float SteeringBlend = 1.1f;
        private const float StrongSteeringBlend = 1.8f;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            lastSteeringSide = nextSteeringSide;
            nextSteeringSide *= -1;

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
            if (target != null)
            {
                lastDistanceToTarget = Vector2.Distance(body.position, target.position);
            }
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
            float distanceToTarget = toTarget.magnitude;
            if (distanceToTarget <= stopDistance)
            {
                body.linearVelocity = Vector2.zero;
                lastDistanceToTarget = distanceToTarget;
                return;
            }

            Vector2 desiredDirection = toTarget / distanceToTarget;
            Vector2 moveDirection = GetMoveDirection(desiredDirection);
            body.linearVelocity = moveDirection * moveSpeed;
            UpdateUnstuckCheck(desiredDirection, distanceToTarget);
        }

        private Vector2 GetMoveDirection(Vector2 desiredDirection)
        {
            cornerRetryTimer = Mathf.Max(0f, cornerRetryTimer - Time.fixedDeltaTime);
            blockedThisStep = false;

            if (steeringTimeRemaining > 0f)
            {
                steeringTimeRemaining -= Time.fixedDeltaTime;
                blockedThisStep = IsObstacleAhead(steeringDirection);
                return steeringDirection;
            }

            if (IsObstacleAhead(desiredDirection))
            {
                blockedThisStep = true;
                BeginSteering(desiredDirection, steeringDuration, SteeringBlend);
                return steeringDirection;
            }

            return desiredDirection;
        }

        private void BeginSteering(Vector2 desiredDirection, float duration, float steeringBlend)
        {
            Vector2 leftDirection = BuildSteeringDirection(desiredDirection, 1, steeringBlend);
            Vector2 rightDirection = BuildSteeringDirection(desiredDirection, -1, steeringBlend);
            bool leftBlocked = IsObstacleAhead(leftDirection);
            bool rightBlocked = IsObstacleAhead(rightDirection);

            if (leftBlocked != rightBlocked)
            {
                lastSteeringSide = leftBlocked ? -1 : 1;
            }

            steeringDirection = BuildSteeringDirection(desiredDirection, lastSteeringSide, steeringBlend);
            steeringTimeRemaining = duration;
        }

        private void RetryOppositeSide(Vector2 desiredDirection)
        {
            if (cornerRetryTimer > 0f)
            {
                return;
            }

            lastSteeringSide *= -1;
            steeringDirection = BuildSteeringDirection(desiredDirection, lastSteeringSide, StrongSteeringBlend);
            steeringTimeRemaining = strongSteeringDuration;
            cornerRetryTimer = cornerRetryTime;
        }

        private Vector2 BuildSteeringDirection(Vector2 desiredDirection, int side, float steeringBlend)
        {
            Vector2 perpendicular = new Vector2(-desiredDirection.y, desiredDirection.x) * side;
            return (desiredDirection + perpendicular * steeringBlend).normalized;
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

        private void UpdateUnstuckCheck(Vector2 desiredDirection, float distanceToTarget)
        {
            stuckCheckTimer += Time.fixedDeltaTime;
            if (stuckCheckTimer < unstuckTime)
            {
                return;
            }

            float movedDistance = Vector2.Distance(body.position, lastStuckCheckPosition);
            bool madeProgress = lastDistanceToTarget - distanceToTarget > ProgressThreshold;
            if (movedDistance < StuckMoveThreshold || (blockedThisStep && !madeProgress))
            {
                RetryOppositeSide(desiredDirection);
            }

            stuckCheckTimer = 0f;
            lastStuckCheckPosition = body.position;
            lastDistanceToTarget = distanceToTarget;
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
