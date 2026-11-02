#if UNITY_EDITOR
using System.IO;
using DungeonGame.Cameras;
using DungeonGame.Combat;
using DungeonGame.Core;
using DungeonGame.Data;
using DungeonGame.Player;
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

        [MenuItem("DungeonGame/Thiết Kế Lại Toàn Bộ Nhân Vật Kiểu RPG (1-Click)", priority = 1)]
        public static void ForceGenerate()
        {
            GenerateAllRpgAssets();
            ApplyToCurrentScene();
        }

        private static void CheckAndGenerateRpgAssets()
        {
            if (!File.Exists("Assets/Sprites/Hero_Knight.png"))
            {
                GenerateAllRpgAssets();
                ApplyToCurrentScene();
            }
        }

        public static void GenerateAllRpgAssets()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Sprites"))
                AssetDatabase.CreateFolder("Assets", "Sprites");

            SaveTextureAsPng(GenerateKnightHero(), "Assets/Sprites/Hero_Knight.png");
            SaveTextureAsPng(GenerateDemonSlime(), "Assets/Sprites/Enemy_Slime.png");
            SaveTextureAsPng(GenerateSkeleton(), "Assets/Sprites/Enemy_Skeleton.png");
            SaveTextureAsPng(GenerateFireDemon(), "Assets/Sprites/Enemy_Demon.png");
            SaveTextureAsPng(GenerateMagicStaff(), "Assets/Sprites/Weapon_Staff.png");
            SaveTextureAsPng(GenerateFireballBullet(), "Assets/Sprites/Magic_Bullet.png");
            SaveTextureAsPng(GenerateDungeonFloor(), "Assets/Sprites/Dungeon_Floor.png");

            AssetDatabase.Refresh();

            // Cấu hình import settings cho các file PNG mới
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

        public static void ApplyToCurrentScene()
        {
            Sprite heroSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Hero_Knight.png");
            Sprite slimeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Enemy_Slime.png");
            Sprite skeletonSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Enemy_Skeleton.png");
            Sprite demonSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Enemy_Demon.png");
            Sprite staffSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Weapon_Staff.png");
            Sprite bulletSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Magic_Bullet.png");
            Sprite floorSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Dungeon_Floor.png");

            // 1. CẬP NHẬT PREFAB ĐẠN (MAGIC BULLET)
            string bulletPath = "Assets/Prefabs/Bullet.prefab";
            GameObject bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(bulletPath);
            if (bulletPrefab != null)
            {
                SpriteRenderer bSr = bulletPrefab.GetComponent<SpriteRenderer>();
                if (bSr != null && bulletSprite != null)
                {
                    bSr.sprite = bulletSprite;
                    bSr.color = Color.white;
                }
                EditorUtility.SetDirty(bulletPrefab);
            }

            // 2. CẬP NHẬT WEAPON DATA
            string weaponPath = "Assets/Data/PistolData.asset";
            WeaponData weaponData = AssetDatabase.LoadAssetAtPath<WeaponData>(weaponPath);
            if (weaponData != null)
            {
                weaponData.weaponName = "Gậy Phép Hỏa Long (RPG)";
                weaponData.weaponIcon = staffSprite;
                weaponData.damage = 35f;
                weaponData.projectileSpeed = 16f;
                weaponData.fireRate = 0.16f;
                weaponData.spreadAngle = 4f;
                weaponData.knockbackForce = 9f;
                EditorUtility.SetDirty(weaponData);
            }

            // 3. CẬP NHẬT PLAYER
            PlayerController player = Object.FindAnyObjectByType<PlayerController>();
            if (player != null)
            {
                SpriteRenderer pSr = player.GetComponent<SpriteRenderer>();
                if (pSr != null && heroSprite != null)
                {
                    pSr.sprite = heroSprite;
                    pSr.color = Color.white;
                }
                player.transform.localScale = new Vector3(1.4f, 1.4f, 1f);

                // Cập nhật súng
                Transform pivot = player.transform.Find("WeaponPivot");
                if (pivot != null)
                {
                    Transform gunVisual = pivot.Find("GunVisual");
                    if (gunVisual != null)
                    {
                        SpriteRenderer gSr = gunVisual.GetComponent<SpriteRenderer>();
                        if (gSr != null && staffSprite != null)
                        {
                            gSr.sprite = staffSprite;
                            gSr.color = Color.white;
                            gunVisual.localScale = new Vector3(1.2f, 1.2f, 1f);
                            gunVisual.localPosition = new Vector3(0.6f, 0f, 0f);
                        }
                    }
                }

                // Gán fallback bullet prefab và weapon data cho PlayerCombat
                PlayerCombat combat = player.GetComponent<PlayerCombat>();
                if (combat != null)
                {
                    SerializedObject combatSo = new SerializedObject(combat);
                    combatSo.FindProperty("currentWeapon").objectReferenceValue = weaponData;
                    combatSo.FindProperty("fallbackBulletPrefab").objectReferenceValue = bulletPrefab;
                    combatSo.ApplyModifiedProperties();
                }
            }

            // 4. CẬP NHẬT CÁC QUÁI VẬT THÀNH RPG MONSTERS
            ApplySpriteToEnemy("Dummy_Enemy_1", slimeSprite, 1.3f, 80f);
            ApplySpriteToEnemy("Dummy_Enemy_2", skeletonSprite, 1.4f, 120f);
            ApplySpriteToEnemy("Dummy_Enemy_3", demonSprite, 1.6f, 200f);

            // 5. TẠO SÀN ĐÁ HẦM NGỤC (DUNGEON TILE FLOOR) ĐỂ KHÔNG BỊ XANH TRƠN
            CreateDungeonFloorGrid(floorSprite);

            // 6. CẬP NHẬT MÀU NỀN CAMERA
            if (Camera.main != null)
            {
                Camera.main.backgroundColor = new Color(0.08f, 0.08f, 0.12f, 1f);
            }

            // 7. CẬP NHẬT OBJECT POOLER
            ObjectPooler pooler = Object.FindAnyObjectByType<ObjectPooler>();
            if (pooler != null && bulletPrefab != null)
            {
                SerializedObject poolSo = new SerializedObject(pooler);
                SerializedProperty poolsProp = poolSo.FindProperty("pools");
                poolsProp.ClearArray();
                poolsProp.InsertArrayElementAtIndex(0);
                SerializedProperty pool0 = poolsProp.GetArrayElementAtIndex(0);
                pool0.FindPropertyRelative("tag").stringValue = "PlayerBullet";
                pool0.FindPropertyRelative("prefab").objectReferenceValue = bulletPrefab;
                pool0.FindPropertyRelative("size").intValue = 40;
                poolSo.ApplyModifiedProperties();
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();

            Debug.Log("<color=#FF9900><b>[RPG Setup] Đã đổi toàn bộ nhân vật sang phong cách RPG Pixel Art (Hiệp Sĩ, Slime, Bộ Xương, Quỷ Lửa)! Bấm PLAY để trải nghiệm ngay!</b></color>");
        }

        private static void ApplySpriteToEnemy(string name, Sprite sprite, float scale, float maxHp)
        {
            GameObject enemy = GameObject.Find(name);
            if (enemy != null && sprite != null)
            {
                SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sprite = sprite;
                    sr.color = Color.white;
                }
                enemy.transform.localScale = new Vector3(scale, scale, 1f);

                HealthComponent health = enemy.GetComponent<HealthComponent>();
                if (health != null)
                {
                    SerializedObject hSo = new SerializedObject(health);
                    hSo.FindProperty("maxHealth").floatValue = maxHp;
                    hSo.FindProperty("currentHealth").floatValue = maxHp;
                    hSo.ApplyModifiedProperties();
                }
            }
        }

        private static void CreateDungeonFloorGrid(Sprite floorSprite)
        {
            if (floorSprite == null) return;
            GameObject oldFloor = GameObject.Find("[DungeonFloor]");
            if (oldFloor != null) Object.DestroyImmediate(oldFloor);

            GameObject floorContainer = new GameObject("[DungeonFloor]");
            floorContainer.transform.position = Vector3.zero;

            int radius = 10;
            for (int x = -radius; x <= radius; x += 2)
            {
                for (int y = -radius; y <= radius; y += 2)
                {
                    GameObject tile = new GameObject($"Tile_{x}_{y}");
                    tile.transform.SetParent(floorContainer.transform, false);
                    tile.transform.position = new Vector3(x, y, 0f);
                    tile.transform.localScale = new Vector3(2f, 2f, 1f);

                    SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
                    sr.sprite = floorSprite;
                    sr.sortingOrder = -10;
                }
            }
        }

        private static void SaveTextureAsPng(Texture2D tex, string path)
        {
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            Object.DestroyImmediate(tex);
        }

        // ================= VẼ PIXEL ART CHO TỪNG NHÂN VẬT RPG =================

        // 1. HIỆP SĨ RPG (KNIGHT HERO 32x32)
        private static Texture2D GenerateKnightHero()
        {
            Texture2D tex = CreateEmptyTex(32, 32);
            Color armor = new Color(0.75f, 0.78f, 0.85f);     // Giáp sắt bạc
            Color armorDark = new Color(0.45f, 0.48f, 0.55f); // Bóng giáp
            Color gold = new Color(0.95f, 0.78f, 0.2f);       // Viền vàng kim
            Color visorGlow = new Color(0.2f, 0.95f, 1f);     // Kính mắt ma thuật xanh
            Color cape = new Color(0.85f, 0.2f, 0.25f);       // Áo choàng đỏ
            Color boots = new Color(0.25f, 0.2f, 0.15f);      // Ủng da

            // Áo choàng đỏ sau lưng
            FillRect(tex, 8, 4, 16, 18, cape);

            // Chân & Ủng
            FillRect(tex, 10, 2, 4, 5, boots);
            FillRect(tex, 18, 2, 4, 5, boots);

            // Thân giáp
            FillRect(tex, 9, 7, 14, 12, armor);
            FillRect(tex, 11, 7, 10, 3, gold); // Đai lưng vàng
            FillRect(tex, 14, 11, 4, 6, armorDark); // Rãnh hoa văn ngực

            // Mũ giáp sắt
            FillRect(tex, 8, 18, 16, 12, armor);
            FillRect(tex, 14, 29, 4, 3, gold); // Đỉnh mũ vàng hoàng gia

            // Khe mắt phát sáng xanh Arcane
            FillRect(tex, 11, 23, 10, 3, Color.black);
            FillRect(tex, 13, 23, 2, 2, visorGlow);
            FillRect(tex, 17, 23, 2, 2, visorGlow);

            tex.Apply();
            return tex;
        }

        // 2. SLIME ĐỊA NGỤC (CRIMSON DEMON SLIME 32x32)
        private static Texture2D GenerateDemonSlime()
        {
            Texture2D tex = CreateEmptyTex(32, 32);
            Color slimeBody = new Color(0.92f, 0.2f, 0.35f);  // Gel đỏ tía
            Color slimeDark = new Color(0.6f, 0.1f, 0.2f);    // Viền bóng
            Color coreGlow = new Color(1f, 0.85f, 0.2f);      // Lõi năng lượng vàng
            Color eye = Color.yellow;
            Color pupil = Color.black;

            // Hình dáng giọt gel
            FillCircle(tex, 16, 12, 12, slimeBody);
            FillCircle(tex, 16, 21, 6, slimeBody);
            FillCircle(tex, 16, 26, 3, slimeDark); // Đỉnh nhọn

            // Lõi phát sáng bên trong
            FillCircle(tex, 16, 13, 5, coreGlow);

            // Mắt quỷ hung dữ
            FillRect(tex, 10, 16, 4, 4, eye);
            FillRect(tex, 18, 16, 4, 4, eye);
            FillRect(tex, 12, 17, 2, 2, pupil);
            FillRect(tex, 18, 17, 2, 2, pupil);

            // Răng nanh nhọn
            FillRect(tex, 13, 10, 2, 2, Color.white);
            FillRect(tex, 17, 10, 2, 2, Color.white);

            tex.Apply();
            return tex;
        }

        // 3. BỘ XƯƠNG MA ÁM (SKELETON WARRIOR 32x32)
        private static Texture2D GenerateSkeleton()
        {
            Texture2D tex = CreateEmptyTex(32, 32);
            Color bone = new Color(0.9f, 0.9f, 0.82f);
            Color darkRobe = new Color(0.2f, 0.18f, 0.28f);
            Color voidEye = new Color(0.85f, 0.2f, 0.95f); // Mắt ma thuật tím

            // Áo choàng rách đen
            FillRect(tex, 8, 3, 16, 16, darkRobe);

            // Xương sườn
            FillRect(tex, 12, 10, 8, 2, bone);
            FillRect(tex, 13, 13, 6, 2, bone);

            // Đầu lâu xương
            FillCircle(tex, 16, 22, 7, bone);
            FillRect(tex, 13, 16, 6, 4, bone); // Hàm

            // Hốc mắt rực lửa tím
            FillRect(tex, 12, 21, 3, 3, Color.black);
            FillRect(tex, 17, 21, 3, 3, Color.black);
            FillRect(tex, 13, 22, 1, 1, voidEye);
            FillRect(tex, 18, 22, 1, 1, voidEye);

            tex.Apply();
            return tex;
        }

        // 4. QUỶ LỬA DUNG NHAM (FIRE DEMON 32x32)
        private static Texture2D GenerateFireDemon()
        {
            Texture2D tex = CreateEmptyTex(32, 32);
            Color rock = new Color(0.22f, 0.18f, 0.2f);      // Thân đá đen
            Color lava = new Color(1f, 0.45f, 0.05f);        // Rãnh nham thạch cam
            Color fireCore = new Color(1f, 0.9f, 0.2f);      // Lõi vàng rực
            Color horns = new Color(0.7f, 0.15f, 0.15f);     // Sừng quỷ đỏ

            // Thân đá
            FillCircle(tex, 16, 14, 11, rock);

            // Rãnh dung nham phát sáng
            FillRect(tex, 10, 12, 12, 3, lava);
            FillRect(tex, 14, 8, 4, 10, fireCore);

            // Sừng quỷ
            FillRect(tex, 7, 22, 4, 7, horns);
            FillRect(tex, 21, 22, 4, 7, horns);

            // Mắt lửa
            FillRect(tex, 11, 16, 3, 3, Color.yellow);
            FillRect(tex, 18, 16, 3, 3, Color.yellow);

            tex.Apply();
            return tex;
        }

        // 5. GẬY PHÉP THUẬT CỔ (MAGIC STAFF 32x16)
        private static Texture2D GenerateMagicStaff()
        {
            Texture2D tex = CreateEmptyTex(32, 16);
            Color wood = new Color(0.55f, 0.35f, 0.18f);
            Color gold = new Color(0.95f, 0.8f, 0.2f);
            Color gem = new Color(0.95f, 0.2f, 0.3f);     // Viên Ruby đỏ ở đầu gậy
            Color glow = new Color(1f, 0.7f, 0.8f);

            // Thân gậy
            FillRect(tex, 2, 7, 22, 3, wood);
            FillRect(tex, 10, 6, 3, 5, gold); // Vòng khuyên vàng

            // Đầu gậy nạm vàng
            FillRect(tex, 22, 4, 4, 9, gold);

            // Viên ngọc đỏ phát sáng
            FillCircle(tex, 28, 8, 4, gem);
            FillRect(tex, 28, 8, 2, 2, glow);

            tex.Apply();
            return tex;
        }

        // 6. CẦU LỬA MA PHÁP (FIREBALL PROJECTILE 16x16)
        private static Texture2D GenerateFireballBullet()
        {
            Texture2D tex = CreateEmptyTex(16, 16);
            Color outer = new Color(1f, 0.25f, 0.05f); // Đỏ cam
            Color mid = new Color(1f, 0.7f, 0.1f);     // Vàng lửa
            Color core = Color.white;                  // Lõi trắng sáng

            FillCircle(tex, 8, 8, 6, outer);
            FillCircle(tex, 8, 8, 4, mid);
            FillCircle(tex, 8, 8, 2, core);

            tex.Apply();
            return tex;
        }

        // 7. SÀN ĐÁ HẦM NGỤC (DUNGEON TILE 32x32)
        private static Texture2D GenerateDungeonFloor()
        {
            Texture2D tex = CreateEmptyTex(32, 32);
            Color stone1 = new Color(0.24f, 0.24f, 0.3f);
            Color stone2 = new Color(0.18f, 0.18f, 0.23f);
            Color border = new Color(0.12f, 0.12f, 0.16f);

            FillRect(tex, 0, 0, 32, 32, stone1);

            // Vân gạch đá
            FillRect(tex, 2, 2, 13, 13, stone2);
            FillRect(tex, 17, 2, 13, 13, stone2);
            FillRect(tex, 2, 17, 28, 13, stone2);

            // Rãnh vữa đen
            FillRect(tex, 0, 15, 32, 2, border);
            FillRect(tex, 15, 0, 2, 16, border);

            tex.Apply();
            return tex;
        }

        // --- CÁC HÀM TIỆN ÍCH VẼ PIXEL ---
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
