using DungeonGame.Player;
using UnityEngine;

namespace DungeonGame.Combat
{
    /// <summary>
    /// Đạn Cầu Lửa Ma Thuật của Boss Màn 2 (Chúa Tể Dung Nham):
    /// Bắn từ xa nhắm vào Player, gây sát thương lớn khi trúng đòn.
    /// Tự động bỏ qua va chạm với quái vật khác.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class BossProjectile : MonoBehaviour
    {
        [Header("Cài đặt Cầu Lửa")]
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private float rotationSpeed = 360f;

        private float damage = 12f;
        private float lifeTimer = 5f;
        private bool isLaunched = false;

        private void Awake()
        {
            if (rb == null) rb = GetComponent<Rigidbody2D>();
        }

        public void Launch(Vector2 direction, float speed, float bulletDamage, float lifetime)
        {
            damage = bulletDamage;
            lifeTimer = lifetime;

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

            // Xoay tròn quả cầu lửa tạo hiệu ứng nham thạch cuộn trào
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

            lifeTimer -= Time.deltaTime;
            if (lifeTimer <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Bỏ qua Boss và các quái vật
            if (collision.name.Contains("Boss") || 
                collision.name.Contains("Enemy") || 
                collision.CompareTag("Enemy") ||
                collision.GetComponent<BossProjectile>() != null)
            {
                return;
            }

            // Va chạm với Player (Người chơi)
            bool isPlayer = collision.CompareTag("Player") ||
                            collision.GetComponent<PlayerController>() != null ||
                            collision.GetComponentInParent<PlayerController>() != null;

            if (isPlayer)
            {
                var health = collision.GetComponent<HealthComponent>();
                if (health == null) health = collision.GetComponentInParent<HealthComponent>();

                if (health != null)
                {
                    health.TakeDamage(damage, transform.right, 4f);
                }

                Destroy(gameObject);
                return;
            }

            // Va chạm với vật cản tĩnh
            bool isObstacle = false;
            try { if (collision.CompareTag("Obstacle")) isObstacle = true; } catch {}
            if (collision.name.Contains("Wall") || collision.name.Contains("Obstacle")) isObstacle = true;

            if (isObstacle)
            {
                Destroy(gameObject);
            }
        }
    }
}
