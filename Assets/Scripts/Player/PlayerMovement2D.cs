using ShadowDash.Input;
using UnityEngine;

namespace ShadowDash.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerKeyboardInput))]
    [RequireComponent(typeof(PlayerDash2D))]
    public sealed class PlayerMovement2D : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 7f;

        private Rigidbody2D body;
        private PlayerKeyboardInput input;
        private PlayerDash2D dash;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            input = GetComponent<PlayerKeyboardInput>();
            dash = GetComponent<PlayerDash2D>();
            body.gravityScale = 0f;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        private void FixedUpdate()
        {
            if (dash.IsDashing)
            {
                return;
            }

            body.linearVelocity = input.MoveDirection * moveSpeed;
        }
    }
}
