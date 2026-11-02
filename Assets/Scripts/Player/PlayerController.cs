using DungeonGame.Combat;
using DungeonGame.Core;
using UnityEngine;

namespace DungeonGame.Player
{
    /// <summary>
    /// Điều khiển nhân vật di chuyển 8 hướng mượt mà, lướt (Dash) né đòn có I-Frames,
    /// và quay mặt hướng về con trỏ chuột.
    /// Tương thích 100% với cả Unity 6 New Input System lẫn Legacy Input.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Tốc độ & Di chuyển")]
        [SerializeField] private float moveSpeed = 5.5f;

        [Header("Cơ chế Lướt né đòn (Dash)")]
        [SerializeField] private float dashSpeed = 15f;
        [SerializeField] private float dashDuration = 0.2f;
        [SerializeField] private float dashCooldown = 1f;

        [Header("Tham chiếu Component")]
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private HealthComponent health;
        [SerializeField] private TrailRenderer dashTrail;

        private Vector2 movementInput;
        private Vector2 lastMoveDirection = Vector2.down;
        private Vector2 dashDirection;

        private bool isDashing = false;
        private float dashTimer = 0f;
        private float dashCooldownTimer = 0f;

        public bool IsDashing => isDashing;
        public Vector2 MovementInput => movementInput;

        private void Awake()
        {
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (health == null) health = GetComponent<HealthComponent>();
            if (dashTrail == null) dashTrail = GetComponentInChildren<TrailRenderer>();

            if (dashTrail != null) dashTrail.emitting = false;
        }

        private void Update()
        {
            HandleCooldowns();

            // Nếu đang trong trạng thái Dash thì không nhận input di chuyển thông thường
            if (isDashing) return;

            ProcessInput();
            HandleAimFacing();
        }

        private void FixedUpdate()
        {
            if (isDashing)
            {
                ApplyDashMovement();
            }
            else
            {
                ApplyNormalMovement();
            }
        }

        private void ProcessInput()
        {
            // Đọc input từ GameInput (hỗ trợ cả New Input System và Legacy)
            movementInput = GameInput.GetMovementVector();

            if (movementInput != Vector2.zero)
            {
                lastMoveDirection = movementInput;
            }

            // Kích hoạt Dash bằng phím Space hoặc chuột phải
            if (GameInput.IsDashPressed() && dashCooldownTimer <= 0f)
            {
                StartDash();
            }
        }

        private void HandleCooldowns()
        {
            if (dashCooldownTimer > 0f)
            {
                dashCooldownTimer -= Time.deltaTime;
            }
        }

        private void StartDash()
        {
            isDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;

            // Nếu đang đứng yên mà dash thì dash theo hướng nhìn gần nhất
            dashDirection = movementInput != Vector2.zero ? movementInput : lastMoveDirection;

            // Bật dải vệt mờ (Trail)
            if (dashTrail != null) dashTrail.emitting = true;

            // Kích hoạt miễn nhiễm sát thương trong suốt thời gian lướt
            if (health != null)
            {
                health.SetInvulnerable(dashDuration);
            }
        }

        private void ApplyDashMovement()
        {
            rb.linearVelocity = dashDirection * dashSpeed;
            dashTimer -= Time.fixedDeltaTime;

            if (dashTimer <= 0f)
            {
                EndDash();
            }
        }

        private void EndDash()
        {
            isDashing = false;
            rb.linearVelocity = Vector2.zero;

            if (dashTrail != null)
            {
                dashTrail.emitting = false;
            }
        }

        private void ApplyNormalMovement()
        {
            rb.linearVelocity = movementInput * moveSpeed;
        }

        private void HandleAimFacing()
        {
            if (spriteRenderer == null) return;

            Vector3 mouseWorldPos = GameInput.GetMouseWorldPosition(Camera.main);
            bool isMouseOnRight = mouseWorldPos.x > transform.position.x;

            spriteRenderer.flipX = !isMouseOnRight;
        }
    }
}
