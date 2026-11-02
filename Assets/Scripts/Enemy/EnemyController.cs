using DungeonGame.Combat;
using DungeonGame.Core;
using DungeonGame.Player;
using UnityEngine;

namespace DungeonGame.Enemy
{
    /// <summary>
    /// Điều khiển AI quái vật: Tự động tìm đường, bám đuổi Player,
    /// xoay hướng mặt và kích hoạt cộng điểm khi bị tiêu diệt.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(HealthComponent))]
    public class EnemyController : MonoBehaviour
    {
        [Header("Chỉ số AI")]
        [SerializeField] private float moveSpeed = 3.2f;
        [SerializeField] private int scoreValue = 100;
        [SerializeField] private GameObject deathEffectPrefab;

        [Header("Tham chiếu Component")]
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private HealthComponent health;

        private Transform playerTarget;
        private bool isDead = false;

        private void Awake()
        {
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (health == null) health = GetComponent<HealthComponent>();
        }

        private void Start()
        {
            FindPlayerTarget();
        }

        private void OnEnable()
        {
            isDead = false;
            if (health != null)
            {
                health.OnDeath += HandleDeath;
            }
            FindPlayerTarget();
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.OnDeath -= HandleDeath;
            }
        }

        private void FindPlayerTarget()
        {
            if (playerTarget == null)
            {
                var player = Object.FindAnyObjectByType<PlayerController>();
                if (player != null)
                {
                    playerTarget = player.transform;
                }
            }
        }

        private void FixedUpdate()
        {
            if (isDead) return;

            if (playerTarget == null)
            {
                FindPlayerTarget();
                return;
            }

            Vector2 direction = (playerTarget.position - transform.position).normalized;

            // Di chuyển đuổi theo Player
            rb.velocity = direction * moveSpeed;

            // Lật mặt Sprite theo hướng chạy
            if (spriteRenderer != null && direction.x != 0)
            {
                spriteRenderer.flipX = direction.x < 0;
            }
        }

        private void HandleDeath()
        {
            if (isDead) return;
            isDead = true;

            // 1. Cộng điểm và ghi nhận mạng tiêu diệt vào GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(scoreValue);
                GameManager.Instance.RegisterKill();
            }

            // 2. Sinh hiệu ứng nổ tan biến nếu có
            if (deathEffectPrefab != null)
            {
                Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            }

            // 3. Biến mất hoặc thu hồi
            Destroy(gameObject);
        }

        public void SetStats(float speed, int score)
        {
            moveSpeed = speed;
            scoreValue = score;
        }
    }
}
