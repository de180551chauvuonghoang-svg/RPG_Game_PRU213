using DungeonGame.Combat;
using DungeonGame.Core;
using DungeonGame.Enemy;
using DungeonGame.Player;
using DungeonGame.World;
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

        [Header("Kỹ năng Bắn Đạn Từ Xa (Dành cho Boss Màn 2)")]
        [SerializeField] private bool canShootRanged = false;
        [SerializeField] private GameObject bossBulletPrefab;
        [SerializeField] private float shootCooldown = 1.7f;
        [SerializeField] private float bulletSpeed = 8.0f;
        [SerializeField] private float bulletDamage = 14f;
        [SerializeField] private bool multiShotOnLowHp = true;

        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private HealthComponent health;
        private Transform playerTarget;
        private bool isDead = false;
        private float nextShootTime = 0f;

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

            nextShootTime = Time.time + 1.2f;
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

        private void Update()
        {
            if (isDead || !canShootRanged || Time.timeScale <= 0f) return;

            if (playerTarget == null)
            {
                FindPlayer();
                if (playerTarget == null) return;
            }

            if (Time.time >= nextShootTime)
            {
                nextShootTime = Time.time + shootCooldown;
                ShootAtPlayer();
            }
        }

        private void ShootAtPlayer()
        {
            if (playerTarget == null) return;

            if (bossBulletPrefab == null)
            {
                bossBulletPrefab = Resources.Load<GameObject>("Boss_Fireball");
#if UNITY_EDITOR
                if (bossBulletPrefab == null) bossBulletPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Boss_Fireball.prefab");
#endif
            }
            if (bossBulletPrefab == null) return;

            Vector2 dir = (playerTarget.position - transform.position).normalized;
            bool isLowHp = (CurrentHealth / MaxHealth) <= 0.5f;

            if (multiShotOnLowHp && isLowHp)
            {
                // Khi máu dưới 50%, Boss nổi giận bắn loạt 3 quả cầu lửa hình nón
                FireSingleBullet(dir);
                FireSingleBullet(RotateVector(dir, 20f));
                FireSingleBullet(RotateVector(dir, -20f));
            }
            else
            {
                FireSingleBullet(dir);
            }
        }

        private void FireSingleBullet(Vector2 direction)
        {
            Vector3 spawnPos = transform.position + (Vector3)(direction * 1.3f);
            GameObject bObj = Instantiate(bossBulletPrefab, spawnPos, Quaternion.identity);
            var proj = bObj.GetComponent<DungeonGame.Combat.BossProjectile>();
            if (proj != null)
            {
                proj.Launch(direction, bulletSpeed, bulletDamage, 4.5f);
            }
        }

        private Vector2 RotateVector(Vector2 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(rad);
            float cos = Mathf.Cos(rad);
            return new Vector2(cos * v.x - sin * v.y, sin * v.x + cos * v.y);
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

            // 2. Mưa tiền vàng của Boss (Rơi 12 đồng coin bung tỏa xung quanh)
            for (int i = 0; i < 12; i++)
            {
                Vector2 scatter = Random.insideUnitCircle * 2.2f;
                CoinPickup.Spawn(transform.position + (Vector3)scatter, 2);
            }

            // 3. Ngừng hoàn toàn việc sinh quái và dọn dẹp các con quái phụ
            var spawner = UnityEngine.Object.FindAnyObjectByType<EnemySpawner>();
            if (spawner != null)
            {
                spawner.StopSpawning();
                spawner.ClearAllEnemies();
            }

            // 3. Nếu chưa phải Màn cuối (Màn 4): Triệu hồi Cổng Dịch Chuyển qua Màn kế tiếp
            // Nếu là Màn 4: Mở modal toàn thắng Victory!
            int curStage = GameManager.Instance != null ? GameManager.Instance.CurrentStage : 1;
            bool isFinalStage = curStage >= 4;
            if (!isFinalStage)
            {
                if (portalPrefab != null)
                {
                    Instantiate(portalPrefab, transform.position, Quaternion.identity);
                }
                else
                {
                    var loaded = Resources.Load<GameObject>("Stage_Portal");
#if UNITY_EDITOR
                    if (loaded == null) loaded = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Stage_Portal.prefab");
#endif
                    if (loaded != null) Instantiate(loaded, transform.position, Quaternion.identity);
                }
            }

            // 4. Báo cho GameManager biết Boss đã bị tiêu diệt
            if (GameManager.Instance != null)
            {
                GameManager.Instance.NotifyBossDefeated(transform.position);
                if (isFinalStage)
                {
                    GameManager.Instance.CompleteStage();
                }
            }

            Debug.Log($"<color=cyan><b>[BOSS DEFEATED] {bossName} đã bị tiêu diệt! {(isFinalStage ? "TOÀN THẮNG TRÒ CHƠI RPG!" : $"Cổng Dịch Chuyển Màn {curStage + 1} đã mở!")}</b></color>");

            Destroy(gameObject, 0.2f);
        }
    }
}
