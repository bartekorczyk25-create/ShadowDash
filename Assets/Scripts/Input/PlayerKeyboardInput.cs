using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ShadowDash.Input
{
    public sealed class PlayerKeyboardInput : MonoBehaviour
    {
        public Vector2 MoveDirection { get; private set; }

        private void Update()
        {
            Vector2 moveDirection = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                MoveDirection = Vector2.zero;
                return;
            }

            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            {
                moveDirection.x -= 1f;
            }

            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            {
                moveDirection.x += 1f;
            }

            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            {
                moveDirection.y -= 1f;
            }

            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            {
                moveDirection.y += 1f;
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (UnityEngine.Input.GetKey(KeyCode.A) || UnityEngine.Input.GetKey(KeyCode.LeftArrow))
            {
                moveDirection.x -= 1f;
            }

            if (UnityEngine.Input.GetKey(KeyCode.D) || UnityEngine.Input.GetKey(KeyCode.RightArrow))
            {
                moveDirection.x += 1f;
            }

            if (UnityEngine.Input.GetKey(KeyCode.S) || UnityEngine.Input.GetKey(KeyCode.DownArrow))
            {
                moveDirection.y -= 1f;
            }

            if (UnityEngine.Input.GetKey(KeyCode.W) || UnityEngine.Input.GetKey(KeyCode.UpArrow))
            {
                moveDirection.y += 1f;
            }
#endif
            MoveDirection = Vector2.ClampMagnitude(moveDirection, 1f);
        }
    }
}
