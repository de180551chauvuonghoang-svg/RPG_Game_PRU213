using UnityEngine;

namespace DungeonGame.Combat
{
    /// <summary>
    /// Gây sát thương khi va chạm vật lý với đối tượng mang IDamageable.
    /// Dùng cho Quái vật cận chiến chạm vào Player, Bẫy gai, Lửa cháy...
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class DamageOnContact : MonoBehaviour
    {
        [Header("Thông số sát thương")]
        [SerializeField] private float damage = 10f;
        [SerializeField] private float knockbackForce = 8f;
        [SerializeField] private LayerMask targetLayer;

        private void OnTriggerStay2D(Collider2D collision)
        {
            ApplyDamage(collision.gameObject);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            ApplyDamage(collision.gameObject);
        }

        private void ApplyDamage(GameObject target)
        {
            if ((targetLayer.value & (1 << target.layer)) > 0)
            {
                if (target.TryGetComponent<IDamageable>(out var damageable))
                {
                    Vector2 knockbackDir = (target.transform.position - transform.position).normalized;
                    damageable.TakeDamage(damage, knockbackDir, knockbackForce);
                }
            }
        }
    }
}
