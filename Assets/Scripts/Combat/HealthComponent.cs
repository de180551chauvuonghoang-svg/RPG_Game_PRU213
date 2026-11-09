using System;
using UnityEngine;

namespace DungeonGame.Combat
{
    /// <summary>
    /// Component quản lý Máu (HP), nhận sát thương, hồi máu và lực nảy lùi (Knockback).
    /// Áp dụng cho cả Player lẫn Kẻ địch (tái sử dụng code theo chuẩn Component Pattern).
    /// </summary>
    public class HealthComponent : MonoBehaviour, IDamageable
    {
        [Header("Chỉ số Máu")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;

        [Header("Bảo vệ sau sát thương (I-Frames)")]
        [SerializeField] private float invulnerabilityDuration = 0.2f;
        private float lastDamageTime = -999f;

        [Header("Tham chiếu vật lý")]
        [SerializeField] private Rigidbody2D rb;

        // Các Events để UI và các hệ thống khác lắng nghe (Observer Pattern)
        public event Action<float, float> OnHealthChanged; // current, max
        public event Action<float> OnDamageTaken;
        public event Action OnDeath;

        public bool IsDead => currentHealth <= 0;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;

        private void Awake()
        {
            if (rb == null)
            {
                rb = GetComponent<Rigidbody2D>();
            }
            currentHealth = maxHealth;
        }

        private void Start()
        {
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void TakeDamage(float damage, Vector2 knockbackDirection, float knockbackForce = 0f)
        {
            if (IsDead) return;

            // Kiểm tra miễn nhiễm sát thương theo thời gian hồi
            if (Time.time < lastDamageTime + invulnerabilityDuration)
            {
                return;
            }

            lastDamageTime = Time.time;
            currentHealth = Mathf.Max(0, currentHealth - damage);

            // Kích hoạt knockback vật lý nếu có Rigidbody2D
            if (rb != null && knockbackForce > 0f)
            {
                rb.linearVelocity = Vector2.zero; // Reset quán tính cũ
                rb.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode2D.Impulse);
            }

            OnDamageTaken?.Invoke(damage);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (IsDead) return;

            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void SetInvulnerable(float duration)
        {
            lastDamageTime = Time.time - invulnerabilityDuration + duration;
        }

        public void SetMaxHealth(float newMaxHealth, bool resetCurrentHealth = true)
        {
            maxHealth = newMaxHealth;
            if (resetCurrentHealth)
            {
                currentHealth = maxHealth;
            }
            else
            {
                currentHealth = Mathf.Min(currentHealth, maxHealth);
            }
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        private void Die()
        {
            OnDeath?.Invoke();
        }
    }
}
