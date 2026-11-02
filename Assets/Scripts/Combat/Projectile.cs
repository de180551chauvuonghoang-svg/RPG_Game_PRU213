using DungeonGame.Player;
using UnityEngine;

namespace DungeonGame.Combat
{
    /// <summary>
    /// Điều khiển viên đạn bay, va chạm với IDamageable và tự tái sinh qua ObjectPool.
    /// Tự động bỏ qua va chạm với Player (người bắn) và các viên đạn khác.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        [Header("Cài đặt Đạn")]
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private LayerMask targetLayers = ~0;
        [SerializeField] private GameObject hitEffectPrefab;

        private float damage;
        private float speed;
        private float knockback;
        private float lifeTimer;
        private bool isLaunched = false;

        private void Awake()
        {
            if (rb == null) rb = GetComponent<Rigidbody2D>();
        }

        public void Launch(Vector2 direction, float bulletSpeed, float bulletDamage, float bulletKnockback, float lifetime)
        {
            speed = bulletSpeed;
            damage = bulletDamage;
            knockback = bulletKnockback;
            lifeTimer = lifetime;

            // Xoay đầu đạn theo hướng bay
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            Vector2 vel = direction.normalized * speed;
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = vel;
#else
            rb.velocity = vel;
#endif
            isLaunched = true;
            gameObject.SetActive(true);
        }

        private void Update()
        {
            if (!isLaunched) return;

            lifeTimer -= Time.deltaTime;
            if (lifeTimer <= 0f)
            {
                Deactivate();
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // 1. Tuyệt đối bỏ qua Player (người bắn) và các viên đạn khác
            if (collision.GetComponent<PlayerController>() != null || 
                collision.name.Contains("Player") || 
                collision.GetComponent<Projectile>() != null)
            {
                return;
            }

            // 2. Gây sát thương nếu va chạm với đối tượng có IDamageable (Quái, Boss, Thùng)
            if (collision.TryGetComponent<IDamageable>(out var damageable))
            {
                Vector2 hitDirection = transform.right;
                damageable.TakeDamage(damage, hitDirection, knockback);
                SpawnHitEffect();
                Deactivate();
                return;
            }

            // 3. Va chạm với Tường / Vật cản tĩnh
            bool isObstacle = false;
            try { if (collision.CompareTag("Obstacle")) isObstacle = true; } catch {}
            int obsLayer = LayerMask.NameToLayer("Obstacles");
            if (obsLayer != -1 && collision.gameObject.layer == obsLayer) isObstacle = true;
            if (collision.gameObject.name.Contains("Wall") || collision.gameObject.name.Contains("Obstacle")) isObstacle = true;

            if (isObstacle)
            {
                SpawnHitEffect();
                Deactivate();
            }
        }

        private void SpawnHitEffect()
        {
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }
        }

        private void Deactivate()
        {
            isLaunched = false;
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector2.zero;
#else
            rb.velocity = Vector2.zero;
#endif
            gameObject.SetActive(false);
        }
    }
}
