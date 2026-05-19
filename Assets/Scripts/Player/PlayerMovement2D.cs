using ShadowDash.Input;
using UnityEngine;

namespace ShadowDash.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerKeyboardInput))]
    public sealed class PlayerMovement2D : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 7f;

        private Rigidbody2D body;
        private PlayerKeyboardInput input;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            input = GetComponent<PlayerKeyboardInput>();
            body.gravityScale = 0f;
        }

        private void FixedUpdate()
        {
            body.linearVelocity = input.MoveDirection * moveSpeed;
        }
    }
}
