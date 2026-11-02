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
        [SerializeField] private int multiShotCount = 1;       // 1 = Bắn 1 tia, 3 = Bắn chùm 3 tia cánh quạt
        [SerializeField] private float damageMultiplier = 1f;   // Nhân sát thương (1x, 1.6x, 2.5x...)
        [SerializeField] private float fireRateMultiplier = 1f; // Nhân tốc độ bắn (càng cao bắn càng nhanh)
        [SerializeField] private float bulletScale = 1f;        // Kích thước viên đạn

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

        // --- CÁC HÀM NÂNG CẤP VŨ KHÍ TỪ GIAN HÀNG ---
        public void UpgradeMultiShot()
        {
            multiShotCount = Mathf.Min(5, multiShotCount + 2); // 1 -> 3 -> 5 tia
        }

        public void UpgradeDamage()
        {
            damageMultiplier += 0.6f; // +60% sát thương
            bulletScale += 0.25f;     // Đạn to hơn tạo cảm giác uy lực
        }

        public void UpgradeFireRate()
        {
            fireRateMultiplier += 0.5f; // Bắn nhanh hơn 50%
        }
    }
}
