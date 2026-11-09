using DungeonGame.Combat;
using DungeonGame.Core;
using DungeonGame.Data;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace DungeonGame.Player
{
    /// <summary>
    /// Quản lý hệ thống chiến đấu của nhân vật RPG:
    /// - Ngắm chuột 360 độ hoặc phím
    /// - Bắn liên thanh khi giữ chuột trái (hoặc bấm phím J / phím K)
    /// - Hỗ trợ Hệ Thống Nâng Cấp Vũ Khí (Bắn 3 tia, Tăng Tốc Độ Xả Đạn, Tăng Sát Thương Khủng)
    /// </summary>
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Dữ liệu Vũ khí RPG (ScriptableObject)")]
        [SerializeField] private WeaponData currentWeapon;

        [Header("Điểm bắn & Hiển thị")]
        [SerializeField] private Transform weaponPivot;
        [SerializeField] private Transform firePoint;
        [SerializeField] private SpriteRenderer weaponSpriteRenderer;
        [SerializeField] private GameObject fallbackBulletPrefab;

        [Header("Chỉ số Nâng Cấp Vũ Khí")]
        [SerializeField] private int multiShotCount = 1;       // 1 = Bắn 1 tia, 3 = Bắn chùm 3 tia, 5 = Bắn 5 tia
        [SerializeField] private float damageMultiplier = 1f;   // Nhân sát thương (1x, 1.6x, 2.2x...)
        [SerializeField] private float fireRateMultiplier = 1f; // Nhân tốc độ bắn (1x, 1.5x, 2.0x...)
        [SerializeField] private float bulletScale = 1f;        // Kích thước viên đạn

        [Header("Cấp độ Nâng Cấp (Level 0 -> 1 -> 2 Tối đa)")]
        [SerializeField] private int multiShotLevel = 0;
        [SerializeField] private int damageLevel = 0;
        [SerializeField] private int fireRateLevel = 0;

        public const int MAX_UPGRADE_LEVEL = 2;

        public int MultiShotLevel => multiShotLevel;
        public int DamageLevel => damageLevel;
        public int FireRateLevel => fireRateLevel;

        public bool IsMultiShotMaxed => multiShotLevel >= MAX_UPGRADE_LEVEL;
        public bool IsDamageMaxed => damageLevel >= MAX_UPGRADE_LEVEL;
        public bool IsFireRateMaxed => fireRateLevel >= MAX_UPGRADE_LEVEL;

        private float nextFireTime = 0f;
        private Camera mainCamera;
        private PlayerController playerController;

        public int MultiShotCount => multiShotCount;
        public float DamageMultiplier => damageMultiplier;
        public float FireRateMultiplier => fireRateMultiplier;

        private void Awake()
        {
            mainCamera = Camera.main;
            playerController = GetComponent<PlayerController>();
            ValidateReferences();
        }

        private void Start()
        {
            ValidateReferences();
        }

        private void ValidateReferences()
        {
            if (weaponPivot == null)
            {
                Transform foundPivot = transform.Find("WeaponPivot");
                if (foundPivot != null) weaponPivot = foundPivot;
                else
                {
                    GameObject pivotObj = new GameObject("WeaponPivot");
                    pivotObj.transform.SetParent(transform, false);
                    weaponPivot = pivotObj.transform;
                }
            }

            if (firePoint == null)
            {
                Transform foundFire = weaponPivot.Find("FirePoint");
                if (foundFire != null) firePoint = foundFire;
                else
                {
                    GameObject fpObj = new GameObject("FirePoint");
                    fpObj.transform.SetParent(weaponPivot, false);
                    fpObj.transform.localPosition = new Vector3(0.6f, 0f, 0f);
                    firePoint = fpObj.transform;
                }
            }

            if (currentWeapon == null)
            {
                currentWeapon = ScriptableObject.CreateInstance<WeaponData>();
                currentWeapon.weaponName = "Gậy Phép Hỏa Long (RPG)";
                currentWeapon.damage = 35f;
                currentWeapon.projectileSpeed = 16f;
                currentWeapon.fireRate = 0.16f;
                currentWeapon.spreadAngle = 3.5f;
                currentWeapon.knockbackForce = 9f;
                currentWeapon.projectileLifetime = 3f;
                currentWeapon.projectilePoolTag = "PlayerBullet";
            }
        }

        private void Update()
        {
            if (Time.timeScale <= 0f) return;
            if (playerController != null && playerController.IsDashing) return;

            AimWeaponAtCursor();
            HandleShootingInput();
        }

        private void AimWeaponAtCursor()
        {
            if (weaponPivot == null) return;
            if (mainCamera == null) mainCamera = Camera.main;

            Vector3 mousePos = GameInput.GetMouseWorldPosition(mainCamera);
            Vector2 aimDirection = (mousePos - weaponPivot.position);

            if (aimDirection.sqrMagnitude > 0.01f)
            {
                float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
                weaponPivot.rotation = Quaternion.Euler(0, 0, angle);

                if (weaponSpriteRenderer != null)
                {
                    weaponSpriteRenderer.flipY = Mathf.Abs(angle) > 90f;
                }
            }
        }

        private void HandleShootingInput()
        {
            bool wantsToShoot = GameInput.IsShootingHeld();

#if ENABLE_INPUT_SYSTEM
            if (!wantsToShoot && Keyboard.current != null)
            {
                if (Keyboard.current.jKey.isPressed || Keyboard.current.kKey.isPressed)
                {
                    wantsToShoot = true;
                }
            }
#endif

            if (wantsToShoot && Time.time >= nextFireTime)
            {
                Shoot();
                float baseRate = currentWeapon != null ? currentWeapon.fireRate : 0.16f;
                nextFireTime = Time.time + (baseRate / fireRateMultiplier);
            }
        }

        private void Shoot()
        {
            ValidateReferences();

            // Tính toán danh sách các góc bắn (Multi-Shot)
            float[] angles;
            if (multiShotCount == 1)
            {
                angles = new float[] { 0f };
            }
            else if (multiShotCount == 2)
            {
                angles = new float[] { -8f, 8f };
            }
            else if (multiShotCount == 3)
            {
                angles = new float[] { -15f, 0f, 15f };
            }
            else
            {
                angles = new float[] { -22f, -11f, 0f, 11f, 22f };
            }

            foreach (float angleOffset in angles)
            {
                FireSingleBullet(angleOffset);
            }
        }

        private void FireSingleBullet(float angleOffset)
        {
            float spread = Random.Range(-currentWeapon.spreadAngle, currentWeapon.spreadAngle) + angleOffset;
            Quaternion bulletRotation = firePoint.rotation * Quaternion.Euler(0, 0, spread);
            Vector2 shootDir = bulletRotation * Vector2.right;

            GameObject bulletObj = null;

            // 1. Thử lấy từ ObjectPooler
            if (ObjectPooler.Instance != null && currentWeapon != null)
            {
                bulletObj = ObjectPooler.Instance.SpawnFromPool(
                    currentWeapon.projectilePoolTag,
                    firePoint.position,
                    bulletRotation
                );
            }

            // 2. Fallback dự phòng nếu Pool rỗng
            if (bulletObj == null)
            {
                GameObject prefabToSpawn = fallbackBulletPrefab;
                if (prefabToSpawn == null) prefabToSpawn = Resources.Load<GameObject>("Bullet");
                if (prefabToSpawn != null)
                {
                    bulletObj = Instantiate(prefabToSpawn, firePoint.position, bulletRotation);
                }
            }

            if (bulletObj != null)
            {
                bulletObj.transform.localScale = Vector3.one * bulletScale;

                if (bulletObj.TryGetComponent<Projectile>(out var projectile))
                {
                    float spd = currentWeapon.projectileSpeed;
                    float dmg = currentWeapon.damage * damageMultiplier;
                    float kb = currentWeapon.knockbackForce;
                    float life = currentWeapon.projectileLifetime;

                    projectile.Launch(shootDir, spd, dmg, kb, life);
                }
            }
        }

        public float BaseDamage => currentWeapon != null ? currentWeapon.damage : 35f;
        public float CurrentDamage => BaseDamage * damageMultiplier;
        public float NextDamage => BaseDamage * (1f + ((damageLevel + 1) * 0.6f));

        public float BaseFireRate => currentWeapon != null ? currentWeapon.fireRate : 0.16f;
        public float CurrentFireRatePerSec => 1f / (BaseFireRate / fireRateMultiplier);
        public float NextFireRatePerSec => 1f / (BaseFireRate / (1f + ((fireRateLevel + 1) * 0.5f)));

        public int NextMultiShotCount => multiShotLevel == 0 ? 3 : 5;

        public int GetUpgradeCost(int currentLevel)
        {
            if (currentLevel >= MAX_UPGRADE_LEVEL) return 0;
            // Level 0 -> Level 1: 15 coins
            // Level 1 -> Level 2 (MAX): 35 coins (mỗi cấp tốn nhiều coin hơn rõ rệt)
            return currentLevel == 0 ? 15 : 35;
        }

        // --- CÁC HÀM NÂNG CẤP VŨ KHÍ TỪ GIAN HÀNG ---
        public bool UpgradeMultiShot(Sprite newWeaponSprite = null)
        {
            if (multiShotLevel >= MAX_UPGRADE_LEVEL) return false;

            multiShotLevel++;
            multiShotCount = multiShotLevel == 1 ? 3 : 5;

            if (newWeaponSprite != null)
            {
                string title = multiShotLevel == 1 ? "Gậy Phép Tam Xạ Phượng Hoàng (Lv1)" : "Bát Hoang Ngũ Xạ Thần Trượng (Lv2 MAX)";
                float sc = multiShotLevel == 1 ? 1.15f : 1.30f;
                ChangeWeaponAppearance(newWeaponSprite, new Vector3(sc, sc, 1f), title);
            }
            return true;
        }

        public bool UpgradeDamage(Sprite newWeaponSprite = null)
        {
            if (damageLevel >= MAX_UPGRADE_LEVEL) return false;

            damageLevel++;
            damageMultiplier = 1f + (damageLevel * 0.6f); // 1.0 -> 1.6 -> 2.2
            bulletScale = 1f + (damageLevel * 0.35f);     // 1.0 -> 1.35 -> 1.7

            if (newWeaponSprite != null)
            {
                string title = damageLevel == 1 ? "Đại Trượng Hủy Diệt Titan (Lv1)" : "Thần Trượng Hỗn Diệt Thái Cổ (Lv2 MAX)";
                float sc = damageLevel == 1 ? 1.35f : 1.55f;
                ChangeWeaponAppearance(newWeaponSprite, new Vector3(sc, sc, 1f), title);
            }
            return true;
        }

        public bool UpgradeFireRate(Sprite newWeaponSprite = null)
        {
            if (fireRateLevel >= MAX_UPGRADE_LEVEL) return false;

            fireRateLevel++;
            fireRateMultiplier = 1f + (fireRateLevel * 0.5f); // 1.0 -> 1.5 -> 2.0

            if (newWeaponSprite != null)
            {
                string title = fireRateLevel == 1 ? "Trượng Thần Tốc Phong Lôi (Lv1)" : "Lôi Quang Siêu Tốc Thần Trượng (Lv2 MAX)";
                float sc = fireRateLevel == 1 ? 1.1f : 1.25f;
                ChangeWeaponAppearance(newWeaponSprite, new Vector3(sc, sc, 1f), title);
            }
            return true;
        }

        public void ChangeWeaponAppearance(Sprite newSprite, Vector3 newScale, string newWeaponName)
        {
            if (weaponSpriteRenderer != null && newSprite != null)
            {
                weaponSpriteRenderer.sprite = newSprite;
                weaponSpriteRenderer.transform.localScale = newScale;
            }
            if (currentWeapon != null && !string.IsNullOrEmpty(newWeaponName))
            {
                currentWeapon.weaponName = newWeaponName;
            }
        }
    }
}
