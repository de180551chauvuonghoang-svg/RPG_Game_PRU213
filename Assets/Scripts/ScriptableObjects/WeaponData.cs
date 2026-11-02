using UnityEngine;

namespace DungeonGame.Data
{
    /// <summary>
    /// ScriptableObject chứa thông số vũ khí (Data-Driven Design).
    /// Cho phép tạo nhiều loại vũ khí (Pistol, Shotgun, Magic Wand...) trực tiếp trên Unity Editor mà không cần sửa code.
    /// </summary>
    [CreateAssetMenu(fileName = "NewWeaponData", menuName = "DungeonGame/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [Header("Thông tin cơ bản")]
        public string weaponName = "Cung Tên Cổ";
        public Sprite weaponIcon;

        [Header("Thông số bắn")]
        [Tooltip("Sát thương mỗi viên đạn")]
        public float damage = 25f;

        [Tooltip("Tốc độ đạn bay")]
        public float projectileSpeed = 14f;

        [Tooltip("Thời gian chờ giữa 2 lần bắn (giây). 0.2s = 5 phát/giây")]
        public float fireRate = 0.25f;

        [Tooltip("Độ giật/lệch góc ngẫu nhiên (tính bằng độ)")]
        public float spreadAngle = 3f;

        [Tooltip("Lực nảy lùi đẩy văng kẻ địch")]
        public float knockbackForce = 6f;

        [Tooltip("Thời gian sống tối đa của viên đạn trước khi tự hủy")]
        public float projectileLifetime = 3f;

        [Header("Pool Tag")]
        [Tooltip("Tên định danh trong Object Pool")]
        public string projectilePoolTag = "PlayerBullet";
    }
}
