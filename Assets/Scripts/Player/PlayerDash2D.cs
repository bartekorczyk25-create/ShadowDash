using ShadowDash.Input;
using UnityEngine;

namespace ShadowDash.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerKeyboardInput))]
    public sealed class PlayerDash2D : MonoBehaviour
    {
        [SerializeField] private float dashSpeed = 18f;
        [SerializeField] private float dashDuration = 0.12f;
        [SerializeField] private float dashCooldown = 0.55f;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Vector3 dashScale = new Vector3(1.2f, 0.8f, 1f);

        public bool IsDashing { get; private set; }

        private Rigidbody2D body;
        private PlayerKeyboardInput input;
        private Vector2 dashDirection = Vector2.right;
        private Vector2 lastMoveDirection = Vector2.right;
        private Vector3 defaultScale;
        private float dashTimeRemaining;
        private float cooldownRemaining;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            input = GetComponent<PlayerKeyboardInput>();
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            if (visualRoot == null)
            {
                visualRoot = transform;
            }

            defaultScale = visualRoot.localScale;
        }

        private void Update()
        {
            if (cooldownRemaining > 0f)
            {
                cooldownRemaining -= Time.deltaTime;
            }

            if (input.MoveDirection.sqrMagnitude > 0.001f)
            {
                lastMoveDirection = input.MoveDirection.normalized;
            }

            if (input.DashPressed)
            {
                TryStartDash();
            }
        }

        private void FixedUpdate()
        {
            if (!IsDashing)
            {
                return;
            }

            body.linearVelocity = dashDirection * dashSpeed;
            dashTimeRemaining -= Time.fixedDeltaTime;

            if (dashTimeRemaining <= 0f)
            {
                EndDash();
            }
        }

        private void TryStartDash()
        {
            if (IsDashing || cooldownRemaining > 0f)
            {
                return;
            }

            dashDirection = input.MoveDirection.sqrMagnitude > 0.001f
                ? input.MoveDirection.normalized
                : lastMoveDirection;

            IsDashing = true;
            dashTimeRemaining = dashDuration;
            cooldownRemaining = dashCooldown;
            visualRoot.localScale = new Vector3(
                defaultScale.x * dashScale.x,
                defaultScale.y * dashScale.y,
                defaultScale.z * dashScale.z);
        }

        private void EndDash()
        {
            IsDashing = false;
            visualRoot.localScale = defaultScale;
        }

        private void OnDisable()
        {
            if (visualRoot != null)
            {
                visualRoot.localScale = defaultScale;
            }

            IsDashing = false;
        }
    }
}
