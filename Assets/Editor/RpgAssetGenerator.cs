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
using UnityEngine.UI;

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
            if (!File.Exists("Assets/Sprites/Hero_Knight.png") || 
                GameObject.Find("Dummy_Enemy_1") != null || 
                GameObject.Find("Dummy_Enemy_2") != null ||
                UnityEngine.Object.FindAnyObjectByType<InGameHUD>() == null)
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

            SaveTextureAsPng(GenerateKnightHero(), "Assets/Sprites/Hero_Knight.png");
            SaveTextureAsPng(GenerateDemonSlime(), "Assets/Sprites/Enemy_Slime.png");
            SaveTextureAsPng(GenerateSkeleton(), "Assets/Sprites/Enemy_Skeleton.png");
            SaveTextureAsPng(GenerateFireDemon(), "Assets/Sprites/Enemy_Demon.png");
            SaveTextureAsPng(GenerateMagicStaff(), "Assets/Sprites/Weapon_Staff.png");
            SaveTextureAsPng(GenerateFireballBullet(), "Assets/Sprites/Magic_Bullet.png");
            SaveTextureAsPng(GenerateDungeonFloor(), "Assets/Sprites/Dungeon_Floor.png");

            AssetDatabase.Refresh();

            ConfigureSpriteImport("Assets/Sprites/Hero_Knight.png", 32);
            ConfigureSpriteImport("Assets/Sprites/Enemy_Slime.png", 32);
            ConfigureSpriteImport("Assets/Sprites/Enemy_Skeleton.png", 32);
            ConfigureSpriteImport("Assets/Sprites/Enemy_Demon.png", 32);
            ConfigureSpriteImport("Assets/Sprites/Weapon_Staff.png", 32);
            ConfigureSpriteImport("Assets/Sprites/Magic_Bullet.png", 32);
            ConfigureSpriteImport("Assets/Sprites/Dungeon_Floor.png", 32);

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
            Sprite staffSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Weapon_Staff.png");
            Sprite bulletSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Magic_Bullet.png");
            Sprite floorSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Dungeon_Floor.png");

            // 1. TẠO HOẶC CẬP NHẬT PREFAB ĐẠN (MAGIC BULLET)
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
                bCol.radius = 0.25f;
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

            // 2. TẠO WEAPON DATA
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

            // 2.5. XÓA PREFAB QUÁI CŨ ĐỂ TẠO LẠI VỚI SÁT THƯƠNG 5 MÁU
            AssetDatabase.DeleteAsset("Assets/Prefabs/Enemy_Slime.prefab");
            AssetDatabase.DeleteAsset("Assets/Prefabs/Enemy_Skeleton.prefab");
            AssetDatabase.DeleteAsset("Assets/Prefabs/Enemy_Demon.prefab");
            AssetDatabase.Refresh();

            // 3. TẠO 3 PREFAB QUÁI VẬT RPG (TỐC ĐỘ VỪA PHẢI, MỖI LẦN CHẠM VÀO TRỪ ĐÚNG 5 MÁU)
            GameObject slimePrefab = CreateEnemyPrefab("Assets/Prefabs/Enemy_Slime.prefab", slimeSprite, 40f, 2.4f, 100, 5f, 1.3f);
            GameObject skeletonPrefab = CreateEnemyPrefab("Assets/Prefabs/Enemy_Skeleton.prefab", skeletonSprite, 80f, 1.8f, 250, 5f, 1.4f);
            GameObject demonPrefab = CreateEnemyPrefab("Assets/Prefabs/Enemy_Demon.prefab", demonSprite, 150f, 1.3f, 500, 5f, 1.6f);

            // 4. CẬP NHẬT PLAYER (MÁU 100% VÀ BẢO VỆ I-FRAMES 0.5s)
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

            player.transform.localScale = new Vector3(1.4f, 1.4f, 1f);
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
            gunVisual.localScale = new Vector3(1.2f, 1.2f, 1f);
            SpriteRenderer gSr = gunVisual.GetComponent<SpriteRenderer>();
            if (gSr == null) gSr = gunVisual.gameObject.AddComponent<SpriteRenderer>();
            gSr.sprite = staffSprite;
            gSr.sortingOrder = 11;

            Transform firePoint = gunVisual.Find("FirePoint");
            if (firePoint == null)
            {
                GameObject fObj = new GameObject("FirePoint");
                fObj.transform.SetParent(gunVisual, false);
                fObj.transform.localPosition = new Vector3(0.5f, 0f, 0f);
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

            // 5. CẬP NHẬT OBJECT POOLER & GAME MANAGER & IN-GAME HUD
            GameObject managersObj = GameObject.Find("[Managers]");
            if (managersObj == null) managersObj = new GameObject("[Managers]");

            GameManager gm = managersObj.GetComponent<GameManager>();
            if (gm == null) gm = managersObj.AddComponent<GameManager>();

            InGameHUD hud = managersObj.GetComponent<InGameHUD>();
            if (hud != null) Object.DestroyImmediate(hud);

            ObjectPooler pooler = managersObj.GetComponent<ObjectPooler>();
            if (pooler == null) pooler = managersObj.AddComponent<ObjectPooler>();

            SerializedObject poolSo = new SerializedObject(pooler);
            SerializedProperty poolsProp = poolSo.FindProperty("pools");
            poolsProp.ClearArray();
            poolsProp.InsertArrayElementAtIndex(0);
            SerializedProperty pool0 = poolsProp.GetArrayElementAtIndex(0);
            pool0.FindPropertyRelative("tag").stringValue = "PlayerBullet";
            pool0.FindPropertyRelative("prefab").objectReferenceValue = bulletPrefab;
            pool0.FindPropertyRelative("size").intValue = 50;
            poolSo.ApplyModifiedProperties();

            // 6. TẠO HỆ THỐNG THẾ GIỚI MỞ VÔ TẬN (INFINITE WORLD MANAGER)
            GameObject oldStaticFloor = GameObject.Find("[DungeonFloor]");
            if (oldStaticFloor != null) Object.DestroyImmediate(oldStaticFloor);

            InfiniteWorldManager worldMgr = UnityEngine.Object.FindAnyObjectByType<InfiniteWorldManager>();
            if (worldMgr == null)
            {
                GameObject wObj = new GameObject("[InfiniteWorldManager]");
                worldMgr = wObj.AddComponent<InfiniteWorldManager>();
            }
            worldMgr.SetFloorSprite(floorSprite);

            // 7. TẠO BỘ SINH QUÁI VẬT TỰ ĐỘNG (ENEMY SPAWNER)
            EnemySpawner spawner = UnityEngine.Object.FindAnyObjectByType<EnemySpawner>();
            if (spawner == null)
            {
                GameObject sObj = new GameObject("[EnemySpawner]");
                spawner = sObj.AddComponent<EnemySpawner>();
            }
            spawner.ClearConfigs();
            spawner.AddEnemyConfig("Slime Quỷ", slimePrefab, 60, 0f);
            spawner.AddEnemyConfig("Xương Ma Ám", skeletonPrefab, 30, 15f);
            spawner.AddEnemyConfig("Quỷ Dung Nham", demonPrefab, 10, 30f);

            // 8. TẠO CANVAS GIAO DIỆN HUD
            CreateOrUpdateHudCanvas();

            // 9. GẮN CAMERA THEO DÕI
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                CameraFollow2D camFollow = mainCam.GetComponent<CameraFollow2D>();
                if (camFollow == null) camFollow = mainCam.gameObject.AddComponent<CameraFollow2D>();

                SerializedObject camSo = new SerializedObject(camFollow);
                camSo.FindProperty("target").objectReferenceValue = player.transform;
                camSo.ApplyModifiedProperties();
                mainCam.backgroundColor = new Color(0.08f, 0.08f, 0.12f, 1f);
            }

            // 10. XÓA HOÀN TOÀN TẤT CẢ QUÁI VẬT CŨ ĐỨNG YÊN TRONG SCENE
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

            Debug.Log("<color=#00FF99><b>[Giai Đoạn 2] THÀNH CÔNG RỰC RỠ! Đã kích hoạt Thế Giới Mở Vô Tận, Spawner sinh quái dồn dập và HUD cộng điểm lên màn hình!</b></color>");
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
            col.radius = 0.45f;

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
            // Xóa sạch Canvas cũ bị trùng lặp text, chỉ giữ lại duy nhất InGameHUD sắc nét
            Canvas existingCanvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            if (existingCanvas != null)
            {
                Object.DestroyImmediate(existingCanvas.gameObject);
            }
            GameObject oldCanvas = GameObject.Find("HUD_Canvas");
            if (oldCanvas != null)
            {
                Object.DestroyImmediate(oldCanvas);
            }
        }

        private static void SaveTextureAsPng(Texture2D tex, string path)
        {
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            Object.DestroyImmediate(tex);
        }

        // ================= VẼ PIXEL ART RPG =================
        private static Texture2D GenerateKnightHero()
        {
            Texture2D tex = CreateEmptyTex(32, 32);
            Color armor = new Color(0.75f, 0.78f, 0.85f);
            Color gold = new Color(0.95f, 0.78f, 0.2f);
            Color visorGlow = new Color(0.2f, 0.95f, 1f);
            Color cape = new Color(0.85f, 0.2f, 0.25f);
            Color boots = new Color(0.25f, 0.2f, 0.15f);

            FillRect(tex, 8, 4, 16, 18, cape);
            FillRect(tex, 10, 2, 4, 5, boots);
            FillRect(tex, 18, 2, 4, 5, boots);
            FillRect(tex, 9, 7, 14, 12, armor);
            FillRect(tex, 11, 7, 10, 3, gold);
            FillRect(tex, 8, 18, 16, 12, armor);
            FillRect(tex, 14, 29, 4, 3, gold);
            FillRect(tex, 11, 23, 10, 3, Color.black);
            FillRect(tex, 13, 23, 2, 2, visorGlow);
            FillRect(tex, 17, 23, 2, 2, visorGlow);

            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateDemonSlime()
        {
            Texture2D tex = CreateEmptyTex(32, 32);
            Color slimeBody = new Color(0.92f, 0.2f, 0.35f);
            Color coreGlow = new Color(1f, 0.85f, 0.2f);

            FillCircle(tex, 16, 12, 12, slimeBody);
            FillCircle(tex, 16, 21, 6, slimeBody);
            FillCircle(tex, 16, 13, 5, coreGlow);
            FillRect(tex, 10, 16, 4, 4, Color.yellow);
            FillRect(tex, 18, 16, 4, 4, Color.yellow);
            FillRect(tex, 12, 17, 2, 2, Color.black);
            FillRect(tex, 18, 17, 2, 2, Color.black);
            FillRect(tex, 13, 10, 2, 2, Color.white);
            FillRect(tex, 17, 10, 2, 2, Color.white);

            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateSkeleton()
        {
            Texture2D tex = CreateEmptyTex(32, 32);
            Color bone = new Color(0.9f, 0.9f, 0.82f);
            Color darkRobe = new Color(0.2f, 0.18f, 0.28f);
            Color voidEye = new Color(0.85f, 0.2f, 0.95f);

            FillRect(tex, 8, 3, 16, 16, darkRobe);
            FillRect(tex, 12, 10, 8, 2, bone);
            FillCircle(tex, 16, 22, 7, bone);
            FillRect(tex, 13, 16, 6, 4, bone);
            FillRect(tex, 12, 21, 3, 3, Color.black);
            FillRect(tex, 17, 21, 3, 3, Color.black);
            FillRect(tex, 13, 22, 1, 1, voidEye);
            FillRect(tex, 18, 22, 1, 1, voidEye);

            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateFireDemon()
        {
            Texture2D tex = CreateEmptyTex(32, 32);
            Color rock = new Color(0.22f, 0.18f, 0.2f);
            Color lava = new Color(1f, 0.45f, 0.05f);
            Color horns = new Color(0.7f, 0.15f, 0.15f);

            FillCircle(tex, 16, 14, 11, rock);
            FillRect(tex, 10, 12, 12, 3, lava);
            FillRect(tex, 7, 22, 4, 7, horns);
            FillRect(tex, 21, 22, 4, 7, horns);
            FillRect(tex, 11, 16, 3, 3, Color.yellow);
            FillRect(tex, 18, 16, 3, 3, Color.yellow);

            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateMagicStaff()
        {
            Texture2D tex = CreateEmptyTex(32, 16);
            Color wood = new Color(0.55f, 0.35f, 0.18f);
            Color gold = new Color(0.95f, 0.8f, 0.2f);
            Color gem = new Color(0.95f, 0.2f, 0.3f);

            FillRect(tex, 2, 7, 22, 3, wood);
            FillRect(tex, 10, 6, 3, 5, gold);
            FillRect(tex, 22, 4, 4, 9, gold);
            FillCircle(tex, 28, 8, 4, gem);

            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateFireballBullet()
        {
            Texture2D tex = CreateEmptyTex(16, 16);
            FillCircle(tex, 8, 8, 6, new Color(1f, 0.25f, 0.05f));
            FillCircle(tex, 8, 8, 4, new Color(1f, 0.7f, 0.1f));
            FillCircle(tex, 8, 8, 2, Color.white);
            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateDungeonFloor()
        {
            Texture2D tex = CreateEmptyTex(32, 32);
            Color stone1 = new Color(0.24f, 0.24f, 0.3f);
            Color stone2 = new Color(0.18f, 0.18f, 0.23f);
            Color border = new Color(0.12f, 0.12f, 0.16f);

            FillRect(tex, 0, 0, 32, 32, stone1);
            FillRect(tex, 2, 2, 13, 13, stone2);
            FillRect(tex, 17, 2, 13, 13, stone2);
            FillRect(tex, 2, 17, 28, 13, stone2);
            FillRect(tex, 0, 15, 32, 2, border);
            FillRect(tex, 15, 0, 2, 16, border);

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
