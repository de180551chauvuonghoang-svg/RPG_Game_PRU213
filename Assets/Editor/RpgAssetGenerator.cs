#if UNITY_EDITOR
using System.IO;
using DungeonGame.Cameras;
using DungeonGame.Combat;
using DungeonGame.Core;
using DungeonGame.Data;
using DungeonGame.Enemy;
using DungeonGame.Player;
using DungeonGame.UI;
using DungeonGame.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DungeonGame.EditorScripts
{
    [InitializeOnLoad]
    public static class RpgAssetGenerator
    {
        static RpgAssetGenerator()
        {
            EditorApplication.delayCall += CheckAndGenerateRpgAssets;
        }

        [MenuItem("DungeonGame/Triển Khai Giai Đoạn 2 (Thế Giới Mở + Spawn Quái + Điểm Số)", priority = 0)]
        public static void ForceGenerateStage2()
        {
            GenerateAllRpgAssets();
            ApplyStage2ToCurrentScene();
        }

        private static void CheckAndGenerateRpgAssets()
        {
            if (!File.Exists("Assets/Sprites/Weapon_Staff_Multi.png") ||
                !File.Exists("Assets/Sprites/Weapon_Staff_Titan.png") ||
                !File.Exists("Assets/Sprites/Weapon_Staff_Speed.png") ||
                !File.Exists("Assets/Sprites/Item_HolyPotion.png") ||
                !File.Exists("Assets/Sprites/Shop_Stall.png") || 
                GameObject.Find("Dummy_Enemy_1") != null || 
                GameObject.Find("Dummy_Enemy_2") != null ||
                UnityEngine.Object.FindAnyObjectByType<UpgradeShopSpawner>() == null)
            {
                GenerateAllRpgAssets();
                ApplyStage2ToCurrentScene();
            }
        }

        public static void GenerateAllRpgAssets()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Sprites"))
                AssetDatabase.CreateFolder("Assets", "Sprites");
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");

            // 1. Tạo các Sprite Nhân Vật & Quái Vật RPG chuẩn 64x64 Pixel Art
            SaveTextureAsPng(GenerateKnightHero64(), "Assets/Sprites/Hero_Knight.png");
            SaveTextureAsPng(GenerateDemonSlime64(), "Assets/Sprites/Enemy_Slime.png");
            SaveTextureAsPng(GenerateSkeleton64(), "Assets/Sprites/Enemy_Skeleton.png");
            SaveTextureAsPng(GenerateFireDemon64(), "Assets/Sprites/Enemy_Demon.png");
            SaveTextureAsPng(GenerateShopStall64(), "Assets/Sprites/Shop_Stall.png");
            SaveTextureAsPng(GenerateDungeonFloor64(), "Assets/Sprites/Dungeon_Floor.png");

            // 2. Tạo Sprite Vũ Khí Căn Bản & Đạn Phép Thuật
            SaveTextureAsPng(GenerateMagicStaff48(), "Assets/Sprites/Weapon_Staff.png");
            SaveTextureAsPng(GenerateFireballBullet32(), "Assets/Sprites/Magic_Bullet.png");

            // 3. Tạo Sprite 4 Phiên Bản Vũ Khí Nâng Cấp Thần Thoại Mới
            SaveTextureAsPng(GenerateMultiStaff48(), "Assets/Sprites/Weapon_Staff_Multi.png");
            SaveTextureAsPng(GenerateTitanStaff48(), "Assets/Sprites/Weapon_Staff_Titan.png");
            SaveTextureAsPng(GenerateSpeedStaff48(), "Assets/Sprites/Weapon_Staff_Speed.png");
            SaveTextureAsPng(GenerateHolyPotion32(), "Assets/Sprites/Item_HolyPotion.png");

            AssetDatabase.Refresh();

            // Cấu hình import sprite chuẩn Pixel Art không mờ
            ConfigureSpriteImport("Assets/Sprites/Hero_Knight.png", 32);
            ConfigureSpriteImport("Assets/Sprites/Enemy_Slime.png", 32);
            ConfigureSpriteImport("Assets/Sprites/Enemy_Skeleton.png", 32);
            ConfigureSpriteImport("Assets/Sprites/Enemy_Demon.png", 32);
            ConfigureSpriteImport("Assets/Sprites/Shop_Stall.png", 32);
            ConfigureSpriteImport("Assets/Sprites/Dungeon_Floor.png", 32);
            ConfigureSpriteImport("Assets/Sprites/Weapon_Staff.png", 32);
            ConfigureSpriteImport("Assets/Sprites/Magic_Bullet.png", 32);

            ConfigureSpriteImport("Assets/Sprites/Weapon_Staff_Multi.png", 32);
            ConfigureSpriteImport("Assets/Sprites/Weapon_Staff_Titan.png", 32);
            ConfigureSpriteImport("Assets/Sprites/Weapon_Staff_Speed.png", 32);
            ConfigureSpriteImport("Assets/Sprites/Item_HolyPotion.png", 32);

            AssetDatabase.Refresh();
        }

        private static void ConfigureSpriteImport(string path, int ppu)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = ppu;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        public static void ApplyStage2ToCurrentScene()
        {
            Sprite heroSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Hero_Knight.png");
            Sprite slimeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Enemy_Slime.png");
            Sprite skeletonSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Enemy_Skeleton.png");
            Sprite demonSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Enemy_Demon.png");
            Sprite shopSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Shop_Stall.png");
            Sprite staffSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Weapon_Staff.png");
            Sprite bulletSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Magic_Bullet.png");
            Sprite floorSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Dungeon_Floor.png");

            Sprite multiSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Weapon_Staff_Multi.png");
            Sprite titanSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Weapon_Staff_Titan.png");
            Sprite speedSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Weapon_Staff_Speed.png");
            Sprite potionSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Item_HolyPotion.png");

            // 1. PREFAB ĐẠN (MAGIC BULLET)
            string bulletPath = "Assets/Prefabs/Bullet.prefab";
            GameObject bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(bulletPath);
            if (bulletPrefab == null)
            {
                GameObject bObj = new GameObject("Bullet");
                bObj.AddComponent<SpriteRenderer>().sprite = bulletSprite;
                var bRb = bObj.AddComponent<Rigidbody2D>();
                bRb.gravityScale = 0f;
                bRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                var bCol = bObj.AddComponent<CircleCollider2D>();
                bCol.isTrigger = true;
                bCol.radius = 0.28f;
                bObj.AddComponent<Projectile>();
                bulletPrefab = PrefabUtility.SaveAsPrefabAsset(bObj, bulletPath);
                Object.DestroyImmediate(bObj);
            }
            else
            {
                SpriteRenderer bSr = bulletPrefab.GetComponent<SpriteRenderer>();
                if (bSr != null && bulletSprite != null) bSr.sprite = bulletSprite;
                EditorUtility.SetDirty(bulletPrefab);
            }

            // 2. TẠO HOẶC CẬP NHẬT PREFAB GIAN HÀNG MA THUẬT (SHOP STALL) VỚI ĐỦ 4 SPRITE NÂNG CẤP
            string shopPath = "Assets/Prefabs/Shop_Stall.prefab";
            AssetDatabase.DeleteAsset(shopPath); // Tái tạo mới để nạp đủ 4 sprite nâng cấp
            AssetDatabase.Refresh();

            GameObject shopObj = new GameObject("Shop_Stall");
            SpriteRenderer sSr = shopObj.AddComponent<SpriteRenderer>();
            sSr.sprite = shopSprite;
            sSr.sortingOrder = 8;
            var sCol = shopObj.AddComponent<CircleCollider2D>();
            sCol.isTrigger = true;
            sCol.radius = 1.4f;
            ShopStall stallComp = shopObj.AddComponent<ShopStall>();
            stallComp.SetUpgradeSprites(multiSprite, titanSprite, speedSprite, potionSprite);

            SerializedObject stallSo = new SerializedObject(stallComp);
            stallSo.FindProperty("multiShotSprite").objectReferenceValue = multiSprite;
            stallSo.FindProperty("damageSprite").objectReferenceValue = titanSprite;
            stallSo.FindProperty("speedSprite").objectReferenceValue = speedSprite;
            stallSo.FindProperty("holyPotionSprite").objectReferenceValue = potionSprite;
            stallSo.ApplyModifiedProperties();

            GameObject shopPrefab = PrefabUtility.SaveAsPrefabAsset(shopObj, shopPath);
            Object.DestroyImmediate(shopObj);

            // 3. TẠO WEAPON DATA
            string weaponPath = "Assets/Data/PistolData.asset";
            WeaponData weaponData = AssetDatabase.LoadAssetAtPath<WeaponData>(weaponPath);
            if (weaponData == null)
            {
                weaponData = ScriptableObject.CreateInstance<WeaponData>();
                AssetDatabase.CreateAsset(weaponData, weaponPath);
            }
            weaponData.weaponName = "Gậy Phép Hỏa Long (RPG)";
            weaponData.weaponIcon = staffSprite;
            weaponData.damage = 35f;
            weaponData.projectileSpeed = 16f;
            weaponData.fireRate = 0.16f;
            weaponData.spreadAngle = 3.5f;
            weaponData.knockbackForce = 9f;
            weaponData.projectileLifetime = 3f;
            weaponData.projectilePoolTag = "PlayerBullet";
            EditorUtility.SetDirty(weaponData);

            // 4. XÓA PREFAB QUÁI CŨ ĐỂ TẠO LẠI VỚI SPRITE MỚI VÀ SÁT THƯƠNG 5 MÁU
            AssetDatabase.DeleteAsset("Assets/Prefabs/Enemy_Slime.prefab");
            AssetDatabase.DeleteAsset("Assets/Prefabs/Enemy_Skeleton.prefab");
            AssetDatabase.DeleteAsset("Assets/Prefabs/Enemy_Demon.prefab");
            AssetDatabase.Refresh();

            // 5. TẠO 3 PREFAB QUÁI VẬT RPG 64x64
            GameObject slimePrefab = CreateEnemyPrefab("Assets/Prefabs/Enemy_Slime.prefab", slimeSprite, 40f, 2.4f, 100, 5f, 1.0f);
            GameObject skeletonPrefab = CreateEnemyPrefab("Assets/Prefabs/Enemy_Skeleton.prefab", skeletonSprite, 80f, 1.8f, 250, 5f, 1.1f);
            GameObject demonPrefab = CreateEnemyPrefab("Assets/Prefabs/Enemy_Demon.prefab", demonSprite, 150f, 1.3f, 500, 5f, 1.35f);

            // 6. CẬP NHẬT PLAYER
            PlayerController player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            if (player == null)
            {
                GameObject pObj = new GameObject("Player");
                pObj.transform.position = Vector3.zero;
                SpriteRenderer pSr = pObj.AddComponent<SpriteRenderer>();
                pSr.sprite = heroSprite;
                pSr.sortingOrder = 10;
                var pRb = pObj.AddComponent<Rigidbody2D>();
                pRb.gravityScale = 0f;
                pRb.constraints = RigidbodyConstraints2D.FreezeRotation;
                var pCol = pObj.AddComponent<CircleCollider2D>();
                pCol.radius = 0.5f;
                player = pObj.AddComponent<PlayerController>();
                pObj.AddComponent<HealthComponent>();
                pObj.AddComponent<HitFlash>();
                pObj.AddComponent<PlayerCombat>();
            }

            player.transform.localScale = Vector3.one;
            SpriteRenderer playerSr = player.GetComponent<SpriteRenderer>();
            if (playerSr != null) playerSr.sprite = heroSprite;

            HealthComponent pHealth = player.GetComponent<HealthComponent>();
            if (pHealth != null)
            {
                SerializedObject pHealthSo = new SerializedObject(pHealth);
                pHealthSo.FindProperty("maxHealth").floatValue = 100f;
                pHealthSo.FindProperty("currentHealth").floatValue = 100f;
                pHealthSo.FindProperty("invulnerabilityDuration").floatValue = 0.5f;
                pHealthSo.ApplyModifiedProperties();
            }

            // Weapon Pivot & FirePoint cho Player
            Transform pivot = player.transform.Find("WeaponPivot");
            if (pivot == null)
            {
                GameObject pivObj = new GameObject("WeaponPivot");
                pivObj.transform.SetParent(player.transform, false);
                pivot = pivObj.transform;
            }

            Transform gunVisual = pivot.Find("GunVisual");
            if (gunVisual == null)
            {
                GameObject gObj = new GameObject("GunVisual");
                gObj.transform.SetParent(pivot, false);
                gunVisual = gObj.transform;
            }
            gunVisual.localPosition = new Vector3(0.6f, 0f, 0f);
            gunVisual.localScale = Vector3.one;
            SpriteRenderer gSr = gunVisual.GetComponent<SpriteRenderer>();
            if (gSr == null) gSr = gunVisual.gameObject.AddComponent<SpriteRenderer>();
            gSr.sprite = staffSprite;
            gSr.sortingOrder = 11;

            Transform firePoint = gunVisual.Find("FirePoint");
            if (firePoint == null)
            {
                GameObject fObj = new GameObject("FirePoint");
                fObj.transform.SetParent(gunVisual, false);
                fObj.transform.localPosition = new Vector3(0.6f, 0f, 0f);
                firePoint = fObj.transform;
            }

            PlayerCombat combat = player.GetComponent<PlayerCombat>();
            if (combat != null)
            {
                SerializedObject combatSo = new SerializedObject(combat);
                combatSo.FindProperty("currentWeapon").objectReferenceValue = weaponData;
                combatSo.FindProperty("weaponPivot").objectReferenceValue = pivot;
                combatSo.FindProperty("firePoint").objectReferenceValue = firePoint;
                combatSo.FindProperty("weaponSpriteRenderer").objectReferenceValue = gSr;
                combatSo.FindProperty("fallbackBulletPrefab").objectReferenceValue = bulletPrefab;
                combatSo.ApplyModifiedProperties();
            }

            // 7. CẬP NHẬT OBJECT POOLER & GAME MANAGER & GIAN HÀNG SPAWNER
            GameObject managersObj = GameObject.Find("[Managers]");
            if (managersObj == null) managersObj = new GameObject("[Managers]");

            GameManager gm = managersObj.GetComponent<GameManager>();
            if (gm == null) gm = managersObj.AddComponent<GameManager>();

            GameUIManager uiMgr = managersObj.GetComponent<GameUIManager>();
            if (uiMgr == null) uiMgr = managersObj.AddComponent<GameUIManager>();

            InGameHUD hud = managersObj.GetComponent<InGameHUD>();
            if (hud != null) Object.DestroyImmediate(hud);

            // Spawner Gian Hàng nâng cấp theo mốc điểm
            UpgradeShopSpawner shopSpawner = managersObj.GetComponent<UpgradeShopSpawner>();
            if (shopSpawner == null) shopSpawner = managersObj.AddComponent<UpgradeShopSpawner>();
            SerializedObject shopSo = new SerializedObject(shopSpawner);
            shopSo.FindProperty("shopPrefab").objectReferenceValue = shopPrefab;
            shopSo.ApplyModifiedProperties();

            // Pool Đạn
            ObjectPooler pooler = managersObj.GetComponent<ObjectPooler>();
            if (pooler == null) pooler = managersObj.AddComponent<ObjectPooler>();

            SerializedObject poolSo = new SerializedObject(pooler);
            SerializedProperty poolsProp = poolSo.FindProperty("pools");
            poolsProp.ClearArray();
            poolsProp.InsertArrayElementAtIndex(0);
            SerializedProperty pool0 = poolsProp.GetArrayElementAtIndex(0);
            pool0.FindPropertyRelative("tag").stringValue = "PlayerBullet";
            pool0.FindPropertyRelative("prefab").objectReferenceValue = bulletPrefab;
            pool0.FindPropertyRelative("size").intValue = 60;
            poolSo.ApplyModifiedProperties();

            // 8. CẬP NHẬT THẾ GIỚI MỞ VÔ TẬN (INFINITE WORLD)
            GameObject oldStaticFloor = GameObject.Find("[DungeonFloor]");
            if (oldStaticFloor != null) Object.DestroyImmediate(oldStaticFloor);

            InfiniteWorldManager worldMgr = UnityEngine.Object.FindAnyObjectByType<InfiniteWorldManager>();
            if (worldMgr == null)
            {
                GameObject wObj = new GameObject("[InfiniteWorldManager]");
                worldMgr = wObj.AddComponent<InfiniteWorldManager>();
            }
            worldMgr.SetFloorSprite(floorSprite);

            // 9. CẬP NHẬT BỘ SINH QUÁI VẬT TỰ ĐỘNG (ENEMY SPAWNER)
            EnemySpawner spawner = UnityEngine.Object.FindAnyObjectByType<EnemySpawner>();
            if (spawner == null)
            {
                GameObject spawnerObj = new GameObject("[EnemySpawner]");
                spawner = spawnerObj.AddComponent<EnemySpawner>();
            }
            spawner.ClearConfigs();
            spawner.AddEnemyConfig("Slime Quỷ", slimePrefab, 60, 0f);
            spawner.AddEnemyConfig("Xương Ma Ám", skeletonPrefab, 30, 15f);
            spawner.AddEnemyConfig("Quỷ Dung Nham", demonPrefab, 10, 30f);

            // 10. XÓA CANVAS CŨ ĐỂ DUY TRÌ 1 GIAO DIỆN DUY NHẤT
            CreateOrUpdateHudCanvas();

            // 11. GẮN CAMERA THEO DÕI
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                CameraFollow2D camFollow = mainCam.GetComponent<CameraFollow2D>();
                if (camFollow == null) camFollow = mainCam.gameObject.AddComponent<CameraFollow2D>();

                SerializedObject camSo = new SerializedObject(camFollow);
                camSo.FindProperty("target").objectReferenceValue = player.transform;
                camSo.ApplyModifiedProperties();
                mainCam.backgroundColor = new Color(0.06f, 0.06f, 0.1f, 1f);
            }

            // 12. XÓA DUMMY ENEMIES CŨ
            DeleteOldDummy("Dummy_Enemy_1");
            DeleteOldDummy("Dummy_Enemy_2");
            DeleteOldDummy("Dummy_Enemy_3");
            DeleteOldDummy("Enemy_Dummy");

            foreach (var oldEnemy in UnityEngine.Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None))
            {
                if (oldEnemy != null && !EditorUtility.IsPersistent(oldEnemy.gameObject))
                {
                    Object.DestroyImmediate(oldEnemy.gameObject);
                }
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();

            Debug.Log("<color=#00FF99><b>[Giai Đoạn 2] NÂNG CẤP HOÀN TẤT! Đã tích hợp tính năng TẠM DỪNG GAME khi mở Shop, hiển thị ảnh xem trước vũ khí mới và đổi ngoại hình trực tiếp trên tay nhân vật!</b></color>");
        }

        private static void DeleteOldDummy(string name)
        {
            GameObject dummy = GameObject.Find(name);
            if (dummy != null) Object.DestroyImmediate(dummy);
        }

        private static GameObject CreateEnemyPrefab(string path, Sprite sprite, float hp, float speed, int score, float contactDmg, float scale)
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            string objName = Path.GetFileNameWithoutExtension(path);
            GameObject obj = new GameObject(objName);
            try { obj.tag = "Enemy"; } catch {}

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 9;
            obj.transform.localScale = new Vector3(scale, scale, 1f);

            var rb = obj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            var col = obj.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;

            var health = obj.AddComponent<HealthComponent>();
            SerializedObject hSo = new SerializedObject(health);
            hSo.FindProperty("maxHealth").floatValue = hp;
            hSo.FindProperty("currentHealth").floatValue = hp;
            hSo.ApplyModifiedProperties();

            obj.AddComponent<HitFlash>();

            var dmg = obj.AddComponent<DamageOnContact>();
            SerializedObject dmgSo = new SerializedObject(dmg);
            dmgSo.FindProperty("damage").floatValue = contactDmg;
            dmgSo.FindProperty("targetLayer").intValue = ~0;
            dmgSo.ApplyModifiedProperties();

            var enemyCtrl = obj.AddComponent<EnemyController>();
            enemyCtrl.SetStats(speed, score);

            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
            return savedPrefab;
        }

        private static void CreateOrUpdateHudCanvas()
        {
            foreach (var c in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                if (c != null && !EditorUtility.IsPersistent(c.gameObject))
                {
                    Object.DestroyImmediate(c.gameObject);
                }
            }
        }

        private static void SaveTextureAsPng(Texture2D tex, string path)
        {
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            Object.DestroyImmediate(tex);
        }

        // ================= VẼ PIXEL ART RPG CAO CẤP (64x64 & 48x48) =================

        /// <summary>
        /// Hiệp Sĩ RPG (64x64): Giáp sắt viền vàng kim, áo choàng đỏ hoàng gia, mũ chiến binh lông vũ lấp lánh và kính ngắm dạ quang
        /// </summary>
        private static Texture2D GenerateKnightHero64()
        {
            Texture2D tex = CreateEmptyTex(64, 64);

            Color outline = new Color(0.1f, 0.1f, 0.15f);
            Color armorDark = new Color(0.45f, 0.5f, 0.6f);
            Color armorMid = new Color(0.7f, 0.75f, 0.85f);
            Color armorLight = new Color(0.9f, 0.95f, 1f);
            Color gold = new Color(1f, 0.82f, 0.2f);
            Color goldDark = new Color(0.75f, 0.55f, 0.1f);
            Color capeDark = new Color(0.55f, 0.1f, 0.15f);
            Color capeBright = new Color(0.9f, 0.22f, 0.25f);
            Color visorGlow = new Color(0.2f, 0.95f, 1f);
            Color bootsLeather = new Color(0.35f, 0.25f, 0.18f);

            // 1. Áo choàng đỏ phía sau (Cape)
            FillRect(tex, 16, 6, 32, 38, capeDark);
            FillRect(tex, 18, 10, 28, 30, capeBright);
            FillRect(tex, 24, 8, 4, 30, capeDark);
            FillRect(tex, 36, 8, 4, 30, capeDark);

            // 2. Chân và Ủng chiến binh (Boots)
            FillRect(tex, 20, 4, 8, 12, outline);
            FillRect(tex, 36, 4, 8, 12, outline);
            FillRect(tex, 22, 6, 5, 8, bootsLeather);
            FillRect(tex, 38, 6, 5, 8, bootsLeather);
            FillRect(tex, 21, 14, 7, 5, armorMid);
            FillRect(tex, 37, 14, 7, 5, armorMid);

            // 3. Giáp ngực và thắt lưng (Chestplate & Belt)
            FillRect(tex, 18, 18, 28, 22, outline);
            FillRect(tex, 20, 20, 24, 18, armorMid);
            FillRect(tex, 22, 24, 8, 12, armorLight);
            FillRect(tex, 34, 24, 8, 12, armorDark);
            FillRect(tex, 20, 19, 24, 4, goldDark);
            FillRect(tex, 29, 18, 6, 6, gold);

            // 4. Giáp vai dũng mãnh (Pauldrons)
            FillCircle(tex, 16, 36, 7, outline);
            FillCircle(tex, 16, 36, 5, armorMid);
            FillCircle(tex, 16, 36, 3, gold);

            FillCircle(tex, 48, 36, 7, outline);
            FillCircle(tex, 48, 36, 5, armorMid);
            FillCircle(tex, 48, 36, 3, gold);

            // 5. Mũ Giáp Hiệp Sĩ (Knight Helm)
            FillRect(tex, 22, 38, 20, 20, outline);
            FillRect(tex, 24, 40, 16, 16, armorMid);
            FillRect(tex, 26, 42, 6, 12, armorLight);
            FillRect(tex, 22, 54, 20, 3, gold);

            // Kính ngắm dạ quang
            FillRect(tex, 24, 46, 16, 4, Color.black);
            FillRect(tex, 26, 47, 5, 2, visorGlow);
            FillRect(tex, 33, 47, 5, 2, visorGlow);

            // Lông vũ đỏ hoàng gia trên chóp mũ
            FillRect(tex, 30, 56, 4, 7, gold);
            FillCircle(tex, 32, 60, 4, capeBright);
            FillCircle(tex, 35, 58, 3, capeDark);

            tex.Apply();
            return tex;
        }

        /// <summary>
        /// Quái Slime Quỷ 64x64: Thân thạch lựu ma quái nhiều lớp bóng bẩy, lõi dung nham rực sáng, sừng quỷ nhọn hoắt và nanh sắc
        /// </summary>
        private static Texture2D GenerateDemonSlime64()
        {
            Texture2D tex = CreateEmptyTex(64, 64);

            Color outline = new Color(0.25f, 0.05f, 0.1f);
            Color slimeOuter = new Color(0.85f, 0.15f, 0.35f);
            Color slimeMid = new Color(0.98f, 0.3f, 0.5f);
            Color slimeLight = new Color(1f, 0.65f, 0.8f);
            Color coreFire = new Color(1f, 0.85f, 0.2f);
            Color horns = new Color(0.18f, 0.15f, 0.25f);
            Color hornsTip = new Color(1f, 0.4f, 0.1f);

            FillRect(tex, 12, 42, 8, 14, horns);
            FillRect(tex, 10, 50, 6, 8, hornsTip);
            FillRect(tex, 44, 42, 8, 14, horns);
            FillRect(tex, 48, 50, 6, 8, hornsTip);

            FillCircle(tex, 32, 26, 25, outline);
            FillCircle(tex, 32, 26, 23, slimeOuter);
            FillCircle(tex, 32, 28, 19, slimeMid);

            FillCircle(tex, 32, 24, 11, coreFire);
            FillCircle(tex, 32, 24, 6, Color.white);

            FillCircle(tex, 24, 38, 5, slimeLight);
            FillCircle(tex, 23, 39, 2, Color.white);

            FillRect(tex, 20, 28, 8, 8, Color.black);
            FillRect(tex, 36, 28, 8, 8, Color.black);
            FillRect(tex, 22, 30, 4, 4, Color.yellow);
            FillRect(tex, 38, 30, 4, 4, Color.yellow);
            FillRect(tex, 24, 31, 2, 2, Color.red);
            FillRect(tex, 40, 31, 2, 2, Color.red);

            FillRect(tex, 26, 17, 3, 4, Color.white);
            FillRect(tex, 35, 17, 3, 4, Color.white);

            tex.Apply();
            return tex;
        }

        /// <summary>
        /// Xương Ma Ám 64x64: Hộp sọ cổ đại rực mắt tím hư không, áo choàng phù thủy rách rưới và ngọn lửa linh hồn
        /// </summary>
        private static Texture2D GenerateSkeleton64()
        {
            Texture2D tex = CreateEmptyTex(64, 64);

            Color outline = new Color(0.1f, 0.08f, 0.15f);
            Color boneDark = new Color(0.7f, 0.68f, 0.6f);
            Color boneMid = new Color(0.92f, 0.9f, 0.82f);
            Color boneLight = new Color(1f, 1f, 0.95f);
            Color robeDark = new Color(0.18f, 0.14f, 0.26f);
            Color robeMid = new Color(0.32f, 0.25f, 0.45f);
            Color soulFlame = new Color(0.85f, 0.2f, 0.98f);
            Color soulLight = new Color(0.4f, 0.9f, 1f);

            FillRect(tex, 16, 4, 32, 36, outline);
            FillRect(tex, 18, 6, 28, 32, robeDark);
            FillRect(tex, 22, 10, 20, 24, robeMid);

            FillRect(tex, 26, 18, 12, 14, outline);
            FillCircle(tex, 32, 25, 5, soulFlame);
            FillCircle(tex, 32, 25, 2, soulLight);

            FillRect(tex, 24, 28, 16, 2, boneMid);
            FillRect(tex, 25, 24, 14, 2, boneMid);
            FillRect(tex, 26, 20, 12, 2, boneMid);

            FillRect(tex, 12, 34, 8, 10, boneDark);
            FillRect(tex, 44, 34, 8, 10, boneDark);

            FillCircle(tex, 32, 46, 14, outline);
            FillCircle(tex, 32, 46, 12, boneMid);
            FillCircle(tex, 30, 48, 8, boneLight);

            FillRect(tex, 26, 36, 12, 6, boneDark);
            FillRect(tex, 27, 37, 2, 3, boneLight);
            FillRect(tex, 31, 37, 2, 3, boneLight);
            FillRect(tex, 35, 37, 2, 3, boneLight);

            FillCircle(tex, 26, 46, 4, Color.black);
            FillCircle(tex, 38, 46, 4, Color.black);
            FillCircle(tex, 26, 46, 2, soulFlame);
            FillCircle(tex, 38, 46, 2, soulFlame);
            FillRect(tex, 26, 46, 1, 1, Color.white);
            FillRect(tex, 38, 46, 1, 1, Color.white);

            FillRect(tex, 31, 41, 2, 3, Color.black);

            tex.Apply();
            return tex;
        }

        /// <summary>
        /// Quỷ Dung Nham 64x64: Khổng lồ dung nham đá núi lửa, cặp sừng đại ma vương rực lửa và vết nứt dung nham sôi sục
        /// </summary>
        private static Texture2D GenerateFireDemon64()
        {
            Texture2D tex = CreateEmptyTex(64, 64);

            Color rockDark = new Color(0.16f, 0.12f, 0.14f);
            Color rockMid = new Color(0.28f, 0.22f, 0.25f);
            Color lavaCore = new Color(1f, 0.25f, 0.05f);
            Color lavaBright = new Color(1f, 0.75f, 0.1f);
            Color hornColor = new Color(0.45f, 0.12f, 0.14f);
            Color hornLight = new Color(0.85f, 0.25f, 0.15f);

            FillCircle(tex, 14, 48, 9, hornColor);
            FillCircle(tex, 10, 55, 7, hornLight);
            FillCircle(tex, 50, 48, 9, hornColor);
            FillCircle(tex, 54, 55, 7, hornLight);

            FillCircle(tex, 32, 28, 22, rockDark);
            FillCircle(tex, 32, 28, 20, rockMid);

            FillCircle(tex, 12, 34, 7, lavaCore);
            FillCircle(tex, 12, 34, 4, lavaBright);
            FillCircle(tex, 52, 34, 7, lavaCore);
            FillCircle(tex, 52, 34, 4, lavaBright);

            FillRect(tex, 20, 26, 24, 4, lavaCore);
            FillRect(tex, 22, 27, 20, 2, lavaBright);
            FillRect(tex, 30, 16, 4, 18, lavaCore);
            FillRect(tex, 31, 18, 2, 14, lavaBright);
            FillRect(tex, 24, 20, 16, 3, lavaCore);

            FillCircle(tex, 32, 44, 12, rockDark);
            FillCircle(tex, 32, 44, 10, rockMid);

            FillRect(tex, 23, 44, 6, 5, lavaCore);
            FillRect(tex, 35, 44, 6, 5, lavaCore);
            FillRect(tex, 24, 45, 3, 3, lavaBright);
            FillRect(tex, 36, 45, 3, 3, lavaBright);

            FillRect(tex, 26, 36, 12, 4, lavaCore);
            FillRect(tex, 28, 37, 8, 2, lavaBright);

            tex.Apply();
            return tex;
        }

        /// <summary>
        /// Gian Hàng Ma Thuật (Shop Stall) 64x64
        /// </summary>
        private static Texture2D GenerateShopStall64()
        {
            Texture2D tex = CreateEmptyTex(64, 64);

            Color woodDark = new Color(0.35f, 0.2f, 0.12f);
            Color woodMid = new Color(0.55f, 0.35f, 0.2f);
            Color woodLight = new Color(0.75f, 0.5f, 0.3f);
            Color stoneBase = new Color(0.25f, 0.25f, 0.3f);
            Color gold = new Color(1f, 0.85f, 0.2f);
            Color clothPurple = new Color(0.42f, 0.18f, 0.55f);
            Color orbGlow = new Color(0.2f, 0.95f, 1f);
            Color potionRed = new Color(0.95f, 0.2f, 0.25f);
            Color potionBlue = new Color(0.2f, 0.5f, 1f);

            FillRect(tex, 8, 4, 48, 8, stoneBase);
            FillRect(tex, 10, 8, 44, 3, new Color(0.35f, 0.35f, 0.42f));

            FillRect(tex, 12, 12, 40, 16, woodDark);
            FillRect(tex, 14, 14, 36, 12, woodMid);
            FillRect(tex, 14, 18, 36, 2, woodDark);
            FillRect(tex, 14, 22, 36, 2, woodDark);
            FillRect(tex, 10, 26, 44, 4, woodLight);

            FillRect(tex, 18, 30, 6, 2, gold);
            FillCircle(tex, 21, 36, 6, orbGlow);
            FillCircle(tex, 20, 38, 2, Color.white);

            FillRect(tex, 32, 30, 4, 6, potionRed);
            FillRect(tex, 33, 36, 2, 2, woodLight);

            FillRect(tex, 40, 30, 4, 6, potionBlue);
            FillRect(tex, 41, 36, 2, 2, woodLight);

            FillRect(tex, 12, 28, 4, 24, woodDark);
            FillRect(tex, 13, 28, 2, 24, woodLight);
            FillRect(tex, 48, 28, 4, 24, woodDark);
            FillRect(tex, 49, 28, 2, 24, woodLight);

            FillRect(tex, 6, 48, 52, 12, woodDark);
            for (int x = 8; x < 56; x += 8)
            {
                FillRect(tex, x, 50, 4, 10, clothPurple);
                FillRect(tex, x + 4, 50, 4, 10, gold);
            }
            for (int x = 8; x < 56; x += 4)
            {
                FillCircle(tex, x + 2, 49, 2, gold);
            }

            FillRect(tex, 31, 46, 2, 5, woodDark);
            FillCircle(tex, 32, 44, 3, gold);
            FillCircle(tex, 32, 44, 1, Color.white);

            tex.Apply();
            return tex;
        }

        /// <summary>
        /// Gạch Lát Hầm Ngục Cổ Đại 64x64
        /// </summary>
        private static Texture2D GenerateDungeonFloor64()
        {
            Texture2D tex = CreateEmptyTex(64, 64);

            Color stoneDark = new Color(0.12f, 0.12f, 0.16f);
            Color stoneMid1 = new Color(0.24f, 0.24f, 0.30f);
            Color stoneMid2 = new Color(0.20f, 0.20f, 0.25f);
            Color stoneHighlight = new Color(0.32f, 0.32f, 0.40f);
            Color mossGreen = new Color(0.15f, 0.35f, 0.22f);
            Color runeGlow = new Color(0.3f, 0.6f, 0.85f, 0.45f);

            FillRect(tex, 0, 0, 64, 64, stoneDark);

            FillRect(tex, 2, 34, 28, 28, stoneMid1);
            FillRect(tex, 4, 60, 24, 1, stoneHighlight);
            FillRect(tex, 4, 36, 1, 24, stoneHighlight);

            FillRect(tex, 34, 34, 28, 28, stoneMid2);
            FillRect(tex, 36, 60, 24, 1, stoneHighlight);
            FillRect(tex, 36, 36, 1, 24, stoneHighlight);

            FillRect(tex, 2, 2, 28, 28, stoneMid2);
            FillRect(tex, 4, 28, 24, 1, stoneHighlight);
            FillRect(tex, 4, 4, 1, 24, stoneHighlight);

            FillRect(tex, 34, 2, 28, 28, stoneMid1);
            FillRect(tex, 36, 28, 24, 1, stoneHighlight);
            FillRect(tex, 36, 4, 1, 24, stoneHighlight);

            FillCircle(tex, 32, 32, 4, mossGreen);
            FillCircle(tex, 2, 32, 3, mossGreen);
            FillCircle(tex, 62, 32, 3, mossGreen);
            FillCircle(tex, 32, 2, 3, mossGreen);
            FillCircle(tex, 32, 62, 3, mossGreen);

            FillCircle(tex, 32, 32, 8, runeGlow);
            FillCircle(tex, 32, 32, 6, stoneDark);
            FillRect(tex, 31, 27, 2, 10, runeGlow);
            FillRect(tex, 27, 31, 10, 2, runeGlow);

            tex.Apply();
            return tex;
        }

        /// <summary>
        /// Gậy Phép Hỏa Long Căn Bản (48x24)
        /// </summary>
        private static Texture2D GenerateMagicStaff48()
        {
            Texture2D tex = CreateEmptyTex(48, 24);

            Color wood = new Color(0.55f, 0.35f, 0.18f);
            Color gold = new Color(1f, 0.85f, 0.2f);
            Color ruby = new Color(1f, 0.2f, 0.35f);
            Color rubyLight = new Color(1f, 0.7f, 0.8f);

            FillRect(tex, 4, 10, 32, 4, wood);
            FillRect(tex, 16, 9, 4, 6, gold);
            FillRect(tex, 28, 9, 4, 6, gold);

            FillRect(tex, 34, 6, 6, 12, gold);
            FillCircle(tex, 42, 12, 6, ruby);
            FillCircle(tex, 41, 14, 2, rubyLight);

            tex.Apply();
            return tex;
        }

        /// <summary>
        /// 1. Gậy Phép Tam Xạ Phượng Hoàng (48x28): 3 Nhánh thần lửa với 3 viên ngọc lửa rực sáng
        /// </summary>
        private static Texture2D GenerateMultiStaff48()
        {
            Texture2D tex = CreateEmptyTex(48, 28);

            Color goldDark = new Color(0.75f, 0.55f, 0.1f);
            Color gold = new Color(1f, 0.88f, 0.2f);
            Color flameCore = new Color(1f, 0.95f, 0.4f);
            Color ruby = new Color(1f, 0.18f, 0.25f);

            // Cán gậy mạ vàng
            FillRect(tex, 2, 12, 26, 4, goldDark);
            FillRect(tex, 4, 13, 22, 2, gold);

            // Khung cánh quạt 3 nhánh vương trượng
            FillRect(tex, 26, 10, 6, 8, goldDark);
            FillRect(tex, 30, 6, 8, 16, gold);

            // Nhánh Trên & Ngọc trên
            FillRect(tex, 34, 19, 6, 4, gold);
            FillCircle(tex, 42, 21, 4, ruby);
            FillCircle(tex, 42, 21, 2, flameCore);

            // Nhánh Giữa & Ngọc giữa
            FillRect(tex, 36, 12, 6, 4, gold);
            FillCircle(tex, 44, 14, 4, ruby);
            FillCircle(tex, 44, 14, 2, flameCore);

            // Nhánh Dưới & Ngọc dưới
            FillRect(tex, 34, 5, 6, 4, gold);
            FillCircle(tex, 42, 7, 4, ruby);
            FillCircle(tex, 42, 7, 2, flameCore);

            tex.Apply();
            return tex;
        }

        /// <summary>
        /// 2. Đại Trượng Hủy Diệt Titan (48x28): Thân hắc thạch titan đen tím khổng lồ, vuốt rồng vàng ngậm đại ngọc dung nham rực lửa
        /// </summary>
        private static Texture2D GenerateTitanStaff48()
        {
            Texture2D tex = CreateEmptyTex(48, 28);

            Color obsidian = new Color(0.15f, 0.12f, 0.22f);
            Color titanMetal = new Color(0.35f, 0.32f, 0.45f);
            Color lavaCore = new Color(1f, 0.25f, 0.05f);
            Color lavaHot = new Color(1f, 0.85f, 0.1f);
            Color gold = new Color(1f, 0.8f, 0.2f);

            // Thân trượng titan dày và đồ sộ
            FillRect(tex, 2, 10, 26, 8, obsidian);
            FillRect(tex, 4, 12, 22, 4, titanMetal);

            // Vết nứt dung nham trên thân gậy
            FillRect(tex, 8, 13, 6, 2, lavaCore);
            FillRect(tex, 18, 13, 6, 2, lavaHot);

            // Vuốt rồng hắc thạch to lớn
            FillCircle(tex, 32, 14, 8, gold);
            FillCircle(tex, 32, 14, 6, obsidian);

            // Đại ngọc dung nham Titan khổng lồ
            FillCircle(tex, 40, 14, 7, lavaCore);
            FillCircle(tex, 40, 14, 4, lavaHot);
            FillCircle(tex, 39, 15, 2, Color.white);

            tex.Apply();
            return tex;
        }

        /// <summary>
        /// 3. Trượng Thần Tốc Phong Lôi (48x24): Thiết kế cánh thiên thần mạ vàng, ngọc sapphire điện quang xanh lam lấp lánh tia sét
        /// </summary>
        private static Texture2D GenerateSpeedStaff48()
        {
            Texture2D tex = CreateEmptyTex(48, 24);

            Color silver = new Color(0.85f, 0.9f, 0.95f);
            Color gold = new Color(1f, 0.85f, 0.2f);
            Color lightningBlue = new Color(0.2f, 0.9f, 1f);
            Color lightningCore = new Color(0.9f, 0.98f, 1f);

            // Cán trượng thanh thoát bằng bạc nạm vàng
            FillRect(tex, 4, 10, 26, 4, silver);
            FillRect(tex, 14, 9, 3, 6, gold);
            FillRect(tex, 24, 9, 3, 6, gold);

            // Cánh thiên thần vàng xòe ra đón gió
            FillRect(tex, 28, 4, 6, 16, gold);
            FillCircle(tex, 34, 18, 4, silver);
            FillCircle(tex, 34, 6, 4, silver);

            // Ngọc bích lôi điện (Lightning Sapphire)
            FillCircle(tex, 40, 12, 6, lightningBlue);
            FillCircle(tex, 40, 12, 3, lightningCore);
            FillRect(tex, 38, 11, 4, 2, Color.white);

            tex.Apply();
            return tex;
        }

        /// <summary>
        /// 4. Bình Tiên Dược Thánh Hoàng Gia (32x32): Bình thuốc phục hồi 100% sinh lực
        /// </summary>
        private static Texture2D GenerateHolyPotion32()
        {
            Texture2D tex = CreateEmptyTex(32, 32);

            Color gold = new Color(1f, 0.85f, 0.2f);
            Color glass = new Color(0.85f, 0.95f, 1f, 0.8f);
            Color elixir = new Color(1f, 0.15f, 0.3f);
            Color elixirLight = new Color(1f, 0.65f, 0.75f);

            // Chân đế vàng
            FillRect(tex, 10, 4, 12, 4, gold);

            // Thân bình thủy tinh tròn bầu
            FillCircle(tex, 16, 15, 10, glass);
            FillCircle(tex, 16, 14, 8, elixir);
            FillCircle(tex, 14, 17, 3, elixirLight);
            FillCircle(tex, 13, 18, 1, Color.white);

            // Cổ bình nạm vàng & Nút bần thánh kim
            FillRect(tex, 13, 23, 6, 3, gold);
            FillRect(tex, 14, 26, 4, 4, new Color(0.8f, 0.55f, 0.3f));

            // Hào quang lấp lánh xung quanh
            FillCircle(tex, 7, 24, 2, gold);
            FillCircle(tex, 25, 24, 2, gold);
            FillCircle(tex, 16, 30, 1, Color.white);

            tex.Apply();
            return tex;
        }

        /// <summary>
        /// Đạn Lửa Phép Thuật (32x32)
        /// </summary>
        private static Texture2D GenerateFireballBullet32()
        {
            Texture2D tex = CreateEmptyTex(32, 32);

            Color flameOuter = new Color(1f, 0.2f, 0.05f);
            Color flameMid = new Color(1f, 0.65f, 0.1f);
            Color flameCore = new Color(1f, 0.95f, 0.4f);

            FillCircle(tex, 16, 16, 11, flameOuter);
            FillCircle(tex, 16, 16, 8, flameMid);
            FillCircle(tex, 16, 16, 4, flameCore);
            FillCircle(tex, 15, 17, 2, Color.white);

            tex.Apply();
            return tex;
        }

        private static Texture2D CreateEmptyTex(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color[] clear = new Color[w * h];
            for (int i = 0; i < clear.Length; i++) clear[i] = Color.clear;
            tex.SetPixels(clear);
            return tex;
        }

        private static void FillRect(Texture2D tex, int x, int y, int w, int h, Color color)
        {
            for (int px = x; px < x + w && px < tex.width; px++)
            {
                for (int py = y; py < y + h && py < tex.height; py++)
                {
                    if (px >= 0 && py >= 0) tex.SetPixel(px, py, color);
                }
            }
        }

        private static void FillCircle(Texture2D tex, int cx, int cy, int radius, Color color)
        {
            for (int x = cx - radius; x <= cx + radius; x++)
            {
                for (int y = cy - radius; y <= cy + radius; y++)
                {
                    if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
                    {
                        if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= radius * radius)
                        {
                            tex.SetPixel(x, y, color);
                        }
                    }
                }
            }
        }
    }
}
#endif
