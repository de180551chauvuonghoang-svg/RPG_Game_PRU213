using DungeonGame.Combat;
using DungeonGame.Core;
using DungeonGame.Enemy;
using DungeonGame.Player;
using UnityEngine;

namespace DungeonGame.Enemy
{
    /// <summary>
    /// Điều khiển Trùm Cuối (Final Boss) của từng màn:
    /// - Máu khủng (1,000 HP), kích thước to lớn uy nghi
    /// - Mỗi đòn tấn công / va chạm trừ đúng 10 MÁU của người chơi
    /// - Khi bị tiêu diệt: Ngừng sinh quái vật, dọn quái phụ và triệu hồi Cổng Dịch Chuyển qua Màn 2
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(HealthComponent))]
    public class BossController : MonoBehaviour
    {
        [Header("Thông tin Boss")]
        [SerializeField] private string bossName = "CHÚA TỂ HẦM NGỤC - GOLIATH";
        [SerializeField] private float moveSpeed = 1.8f;
        [SerializeField] private int scoreReward = 2500;
        [SerializeField] private GameObject portalPrefab;

        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private HealthComponent health;
        private Transform playerTarget;
        private bool isDead = false;

        public string BossName => bossName;
        public float CurrentHealth => health != null ? health.CurrentHealth : 0f;
        public float MaxHealth => health != null ? health.MaxHealth : 1000f;
        public bool IsDead => isDead;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            health = GetComponent<HealthComponent>();
        }

        private void Start()
        {
            FindPlayer();

            // Đăng ký sự kiện Boss xuất hiện với GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterActiveBoss(this);
            }
        }

        private void OnEnable()
        {
            isDead = false;
            if (health != null)
            {
                health.OnDeath += HandleDeath;
            }
            FindPlayer();
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.OnDeath -= HandleDeath;
            }
        }

        private void FindPlayer()
        {
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            if (player != null)
            {
                playerTarget = player.transform;
            }
        }

        private void FixedUpdate()
        {
            if (isDead || Time.timeScale <= 0f) return;

            if (playerTarget == null)
            {
                FindPlayer();
                if (playerTarget == null) return;
            }

            Vector2 direction = (playerTarget.position - transform.position).normalized;

#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = direction * moveSpeed;
#else
            rb.velocity = direction * moveSpeed;
#endif

            if (spriteRenderer != null && direction.x != 0)
            {
                spriteRenderer.flipX = direction.x < 0;
            }
        }

        private void HandleDeath()
        {
            if (isDead) return;
            isDead = true;

            // 1. Cộng điểm hạ gục Boss
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(scoreReward);
                GameManager.Instance.RegisterKill();
            }

            // 2. Ngừng hoàn toàn việc sinh quái và dọn dẹp các con quái phụ
            var spawner = UnityEngine.Object.FindAnyObjectByType<EnemySpawner>();
            if (spawner != null)
            {
                spawner.StopSpawning();
                spawner.ClearAllEnemies();
            }

            // 3. Triệu hồi Cổng Dịch Chuyển Không Gian ngay tại vị trí Boss hy sinh
            if (portalPrefab != null)
            {
                Instantiate(portalPrefab, transform.position, Quaternion.identity);
            }
            else
            {
                var loaded = Resources.Load<GameObject>("Stage_Portal");
                if (loaded != null) Instantiate(loaded, transform.position, Quaternion.identity);
            }

            // 4. Báo cho GameManager biết Boss đã bị tiêu diệt
            if (GameManager.Instance != null)
            {
                GameManager.Instance.NotifyBossDefeated(transform.position);
            }

            Debug.Log($"<color=cyan><b>[BOSS DEFEATED] {bossName} đã bị tiêu diệt! Cổng Dịch Chuyển đã mở ra!</b></color>");

            Destroy(gameObject, 0.2f);
        }
    }
}
