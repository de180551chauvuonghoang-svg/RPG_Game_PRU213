using UnityEngine;

namespace DungeonGame.Combat
{
    /// <summary>
    /// Interface cho các đối tượng có thể nhận sát thương (Player, Quái, Thùng gỗ, Boss...)
    /// Chuẩn OOP - Giúp hệ thống vũ khí gây sát thương lên bất kỳ ai mà không phụ thuộc loại đối tượng.
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(float damage, Vector2 knockbackDirection, float knockbackForce = 0f);
        bool IsDead { get; }
    }
}
