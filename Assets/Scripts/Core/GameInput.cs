using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace DungeonGame.Core
{
    /// <summary>
    /// Hệ thống Input tương thích cả Unity 6 New Input System lẫn Legacy Input.
    /// Hỗ trợ cả bàn phím, chuột và tự động bắt sự kiện mượt mà nhất.
    /// </summary>
    public static class GameInput
    {
        public static Vector2 GetMovementVector()
        {
            Vector2 move = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) move.x -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) move.x += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) move.y -= 1f;
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) move.y += 1f;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            try
            {
                float h = Input.GetAxisRaw("Horizontal");
                float v = Input.GetAxisRaw("Vertical");
                if (h != 0 || v != 0) move = new Vector2(h, v);
            }
            catch {}
#endif
            return move.normalized;
        }

        public static bool IsDashPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame) return true;
            if (mouse != null && mouse.rightButton.wasPressedThisFrame) return true;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            try
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(1)) return true;
            }
            catch {}
#endif
            return false;
        }

        public static bool IsShootingHeld()
        {
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse != null)
            {
                if (mouse.leftButton.isPressed || mouse.leftButton.wasPressedThisFrame)
                {
                    return true;
                }
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            try
            {
                if (Input.GetMouseButton(0) || Input.GetMouseButtonDown(0)) return true;
            }
            catch {}
#endif
            return false;
        }

        public static bool IsPausePressed()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.escapeKey.wasPressedThisFrame || keyboard.pKey.wasPressedThisFrame))
                return true;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            try
            {
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) return true;
            }
            catch {}
#endif
            return false;
        }

        public static Vector3 GetMouseScreenPosition()
        {
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse != null)
            {
                Vector2 pos = mouse.position.ReadValue();
                return new Vector3(pos.x, pos.y, 0f);
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            try
            {
                return Input.mousePosition;
            }
            catch {}
#endif
            return Vector3.zero;
        }

        public static Vector3 GetMouseWorldPosition(Camera cam)
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) return Vector3.zero;

            Vector3 screenPos = GetMouseScreenPosition();
            screenPos.z = -cam.transform.position.z;
            Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);
            worldPos.z = 0f;
            return worldPos;
        }
    }
}
