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
    /// - Cơ chế Self-Healing tự tạo Weapon & FirePoint nếu chưa gán, đảm bảo LUÔN BẮN ĐƯỢC 100%.
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

        private float nextFireTime = 0f;
        private Camera mainCamera;
        private PlayerController playerController;

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
            // Tự tìm WeaponPivot nếu chưa gán
            if (weaponPivot == null)
            {
                Transform foundPivot = transform.Find("WeaponPivot");
                if (foundPivot != null)
                {
                    weaponPivot = foundPivot;
                }
                else
                {
                    GameObject pivotObj = new GameObject("WeaponPivot");
                    pivotObj.transform.SetParent(transform, false);
                    weaponPivot = pivotObj.transform;
                }
            }

            // Tự tìm FirePoint nếu chưa gán
            if (firePoint == null)
            {
                Transform foundFire = weaponPivot.Find("FirePoint");
                if (foundFire != null)
                {
                    firePoint = foundFire;
                }
                else
                {
                    GameObject fpObj = new GameObject("FirePoint");
                    fpObj.transform.SetParent(weaponPivot, false);
                    fpObj.transform.localPosition = new Vector3(0.6f, 0f, 0f);
                    firePoint = fpObj.transform;
                }
            }

            // Tự tạo fallback Weapon nếu chưa có
            if (currentWeapon == null)
            {
                currentWeapon = ScriptableObject.CreateInstance<WeaponData>();
                currentWeapon.weaponName = "Gậy Phép Thuật Cổ";
                currentWeapon.damage = 30f;
                currentWeapon.projectileSpeed = 16f;
                currentWeapon.fireRate = 0.2f;
                currentWeapon.spreadAngle = 3f;
                currentWeapon.knockbackForce = 8f;
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

            // Hỗ trợ thêm phím J / phím K để bắn phòng trường hợp chuột chưa focus Game Window
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
                nextFireTime = Time.time + (currentWeapon != null ? currentWeapon.fireRate : 0.2f);
            }
        }

        private void Shoot()
        {
            ValidateReferences();

            Vector2 shootDir = firePoint.right;
            if (currentWeapon != null && currentWeapon.spreadAngle > 0f)
            {
                float spread = Random.Range(-currentWeapon.spreadAngle, currentWeapon.spreadAngle);
                Quaternion spreadRot = firePoint.rotation * Quaternion.Euler(0, 0, spread);
                shootDir = spreadRot * Vector2.right;
            }

            GameObject bulletObj = null;

            // 1. Thử lấy từ ObjectPooler
            if (ObjectPooler.Instance != null && currentWeapon != null)
            {
                bulletObj = ObjectPooler.Instance.SpawnFromPool(
                    currentWeapon.projectilePoolTag,
                    firePoint.position,
                    firePoint.rotation
                );
            }

            // 2. Fallback dự phòng: Nếu pool không có đạn, tự sinh đạn từ Prefab
            if (bulletObj == null)
            {
                GameObject prefabToSpawn = fallbackBulletPrefab;
                if (prefabToSpawn == null)
                {
                    // Tìm prefab Bullet trong Project nếu có
                    prefabToSpawn = Resources.Load<GameObject>("Bullet");
                }

                if (prefabToSpawn != null)
                {
                    bulletObj = Instantiate(prefabToSpawn, firePoint.position, firePoint.rotation);
                }
            }

            // 3. Khởi phóng đạn
            if (bulletObj != null && bulletObj.TryGetComponent<Projectile>(out var projectile))
            {
                float spd = currentWeapon != null ? currentWeapon.projectileSpeed : 16f;
                float dmg = currentWeapon != null ? currentWeapon.damage : 30f;
                float kb = currentWeapon != null ? currentWeapon.knockbackForce : 8f;
                float life = currentWeapon != null ? currentWeapon.projectileLifetime : 3f;

                projectile.Launch(shootDir, spd, dmg, kb, life);
            }
        }

        public void EquipWeapon(WeaponData newWeapon)
        {
            currentWeapon = newWeapon;
            if (weaponSpriteRenderer != null && newWeapon != null)
            {
                weaponSpriteRenderer.sprite = newWeapon.weaponIcon;
            }
        }
    }
}
