using DungeonGame.Combat;
using DungeonGame.Core;
using DungeonGame.Player;
using DungeonGame.World;
using UnityEngine;

namespace DungeonGame.Enemy
{
    /// <summary>
    /// Điều khiển AI quái vật: Tự động tìm đường, bám đuổi Player,
    /// xoay hướng mặt và kích hoạt cộng điểm khi bị tiêu diệt.
    /// An toàn 100% trước MissingReferenceException.
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
            try
            {
                if (playerTarget == null)
                {
                    var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
                    if (player != null)
                    {
                        playerTarget = player.transform;
                    }
                }
            }
            catch (System.Exception)
            {
                playerTarget = null;
            }
        }

        private void FixedUpdate()
        {
            if (isDead) return;

            try
            {
                if (playerTarget == null)
                {
                    FindPlayerTarget();
                    if (playerTarget == null) return;
                }

                Vector2 direction = (playerTarget.position - transform.position).normalized;

#if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = direction * moveSpeed;
#else
                rb.velocity = direction * moveSpeed;
#endif

                // Lật mặt Sprite theo hướng chạy
                if (spriteRenderer != null && direction.x != 0)
                {
                    spriteRenderer.flipX = direction.x < 0;
                }
            }
            catch (System.Exception)
            {
                playerTarget = null;
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

            // 2. Rơi đồng tiền vàng (Coin Drop): 85% cơ hội rơi 1 - 2 đồng coin
            if (Random.value <= 0.85f)
            {
                int coinAmt = Random.Range(1, 3);
                CoinPickup.Spawn(transform.position, coinAmt);
            }

            // 3. Sinh hiệu ứng nổ tan biến nếu có
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
