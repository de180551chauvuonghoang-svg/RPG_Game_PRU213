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
    public static class AutoSceneSetup
    {
        static AutoSceneSetup()
        {
            EditorApplication.delayCall += CheckAndSetupScene;
        }

        [MenuItem("DungeonGame/1-Click Setup Full Game (Tự Động 100%)", priority = 0)]
        public static void ForceSetup()
        {
            SetupFullScene(true);
        }

        private static void CheckAndSetupScene()
        {
            // Kiểm tra xem trong scene đã có Player chưa, nếu chưa có thì tự động setup
            if (Object.FindAnyObjectByType<PlayerController>() == null)
            {
                SetupFullScene(false);
            }
        }

        public static void SetupFullScene(bool isManual)
        {
            // Đảm bảo có các thư mục cần thiết
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");

            // 1. TẠO VŨ KHÍ SCRIPTABLE OBJECT
            string weaponPath = "Assets/Data/PistolData.asset";
            WeaponData weaponData = AssetDatabase.LoadAssetAtPath<WeaponData>(weaponPath);
            if (weaponData == null)
            {
                weaponData = ScriptableObject.CreateInstance<WeaponData>();
                weaponData.weaponName = "Súng Lục Năng Lượng";
                weaponData.damage = 25f;
                weaponData.projectileSpeed = 16f;
                weaponData.fireRate = 0.18f;
                weaponData.spreadAngle = 3.5f;
                weaponData.knockbackForce = 7f;
                weaponData.projectileLifetime = 3f;
                weaponData.projectilePoolTag = "PlayerBullet";

                AssetDatabase.CreateAsset(weaponData, weaponPath);
                AssetDatabase.SaveAssets();
            }

            // 2. TẠO SPRITES MẪU NÉT ĐẸP
            Sprite playerSprite = CreateCircleSprite(48, new Color(0.2f, 0.8f, 0.4f, 1f));
            Sprite gunSprite = CreateSquareSprite(16, 6, new Color(0.85f, 0.85f, 0.9f, 1f));
            Sprite bulletSprite = CreateCircleSprite(24, new Color(1f, 0.85f, 0.1f, 1f));
            Sprite enemySprite = CreateSquareSprite(40, 40, new Color(0.9f, 0.25f, 0.25f, 1f));
            Sprite dummy2Sprite = CreateSquareSprite(40, 40, new Color(0.95f, 0.5f, 0.15f, 1f));

            // 3. TẠO PREFAB ĐẠN (BULLET PREFAB)
            string bulletPrefabPath = "Assets/Prefabs/Bullet.prefab";
            GameObject bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(bulletPrefabPath);
            if (bulletPrefab == null)
            {
                GameObject bulletTemp = new GameObject("Bullet");
                
                SpriteRenderer bSr = bulletTemp.AddComponent<SpriteRenderer>();
                bSr.sprite = bulletSprite;
                bSr.sortingOrder = 5;

                Rigidbody2D bRb = bulletTemp.AddComponent<Rigidbody2D>();
                bRb.gravityScale = 0f;
                bRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

                CircleCollider2D bCol = bulletTemp.AddComponent<CircleCollider2D>();
                bCol.isTrigger = true;
                bCol.radius = 0.25f;

                Projectile proj = bulletTemp.AddComponent<Projectile>();
                SerializedObject projSo = new SerializedObject(proj);
                projSo.FindProperty("targetLayers").intValue = ~0; // All layers
                projSo.ApplyModifiedProperties();

                bulletPrefab = PrefabUtility.SaveAsPrefabAsset(bulletTemp, bulletPrefabPath);
                Object.DestroyImmediate(bulletTemp);
            }

            // 4. TẠO PLAYER NẾU CHƯA CÓ
            PlayerController existingPlayer = Object.FindAnyObjectByType<PlayerController>();
            GameObject playerObj = existingPlayer != null ? existingPlayer.gameObject : null;

            if (playerObj == null)
            {
                playerObj = new GameObject("Player");
                playerObj.transform.position = Vector3.zero;

                // Visual
                SpriteRenderer pSr = playerObj.AddComponent<SpriteRenderer>();
                pSr.sprite = playerSprite;
                pSr.sortingOrder = 10;

                // Physics
                Rigidbody2D pRb = playerObj.AddComponent<Rigidbody2D>();
                pRb.gravityScale = 0f;
                pRb.constraints = RigidbodyConstraints2D.FreezeRotation;
                pRb.linearDamping = 1f;

                CircleCollider2D pCol = playerObj.AddComponent<CircleCollider2D>();
                pCol.radius = 0.5f;

                // Components
                PlayerController pCtrl = playerObj.AddComponent<PlayerController>();
                HealthComponent pHealth = playerObj.AddComponent<HealthComponent>();
                HitFlash pHitFlash = playerObj.AddComponent<HitFlash>();
                PlayerCombat pCombat = playerObj.AddComponent<PlayerCombat>();

                // Weapon Pivot & Fire Point
                GameObject weaponPivot = new GameObject("WeaponPivot");
                weaponPivot.transform.SetParent(playerObj.transform, false);

                GameObject gunVisual = new GameObject("GunVisual");
                gunVisual.transform.SetParent(weaponPivot.transform, false);
                gunVisual.transform.localPosition = new Vector3(0.55f, 0f, 0f);
                SpriteRenderer gSr = gunVisual.AddComponent<SpriteRenderer>();
                gSr.sprite = gunSprite;
                gSr.sortingOrder = 11;

                GameObject firePoint = new GameObject("FirePoint");
                firePoint.transform.SetParent(gunVisual.transform, false);
                firePoint.transform.localPosition = new Vector3(0.45f, 0f, 0f);

                // Serialized link cho PlayerCombat
                SerializedObject combatSo = new SerializedObject(pCombat);
                combatSo.FindProperty("currentWeapon").objectReferenceValue = weaponData;
                combatSo.FindProperty("weaponPivot").objectReferenceValue = weaponPivot.transform;
                combatSo.FindProperty("firePoint").objectReferenceValue = firePoint.transform;
                combatSo.FindProperty("weaponSpriteRenderer").objectReferenceValue = gSr;
                combatSo.ApplyModifiedProperties();
            }

            // 5. TẠO GAME MANAGER & OBJECT POOLER
            GameManager existingGm = Object.FindAnyObjectByType<GameManager>();
            if (existingGm == null)
            {
                GameObject gmObj = new GameObject("[Managers]");
                existingGm = gmObj.AddComponent<GameManager>();
                ObjectPooler pooler = gmObj.AddComponent<ObjectPooler>();

                SerializedObject poolSo = new SerializedObject(pooler);
                SerializedProperty poolsProp = poolSo.FindProperty("pools");
                poolsProp.ClearArray();
                poolsProp.InsertArrayElementAtIndex(0);
                SerializedProperty pool0 = poolsProp.GetArrayElementAtIndex(0);
                pool0.FindPropertyRelative("tag").stringValue = "PlayerBullet";
                pool0.FindPropertyRelative("prefab").objectReferenceValue = bulletPrefab;
                pool0.FindPropertyRelative("size").intValue = 30;
                poolSo.ApplyModifiedProperties();
            }

            // 6. GẮN CAMERA FOLLOW VÀO MAIN CAMERA
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                CameraFollow2D camFollow = mainCam.GetComponent<CameraFollow2D>();
                if (camFollow == null) camFollow = mainCam.gameObject.AddComponent<CameraFollow2D>();

                SerializedObject camSo = new SerializedObject(camFollow);
                camSo.FindProperty("target").objectReferenceValue = playerObj.transform;
                camSo.ApplyModifiedProperties();

                // Đổi màu nền background camera sang tối huyền bí (Dungeon background)
                mainCam.backgroundColor = new Color(0.12f, 0.13f, 0.17f, 1f);
            }

            // 7. TẠO 3 CON QUÁI DUMMY ĐỂ NGƯỜI CHƠI BẮN THỬ NGHIỆM LỰC & MÁU
            CreateDummyIfMissing("Dummy_Enemy_1", new Vector3(3.5f, 1.5f, 0f), enemySprite, 100f);
            CreateDummyIfMissing("Dummy_Enemy_2", new Vector3(4f, -1.8f, 0f), enemySprite, 120f);
            CreateDummyIfMissing("Dummy_Enemy_3", new Vector3(-3.8f, 0.5f, 0f), dummy2Sprite, 150f);

            // Lưu Scene tự động
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=#00FF66><b>[DungeonGame] THÀNH CÔNG 100%! Toàn bộ Nhân vật, Súng, Đạn và Quái mẫu đã được dựng xong. Hãy bấm nút PLAY (▶) để chơi ngay!</b></color>");
        }

        private static void CreateDummyIfMissing(string name, Vector3 pos, Sprite sprite, float hp)
        {
            if (GameObject.Find(name) != null) return;

            GameObject dummy = new GameObject(name);
            dummy.transform.position = pos;

            SpriteRenderer sr = dummy.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 9;

            Rigidbody2D rb = dummy.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.linearDamping = 4f; // Có độ ma sát để khi knockback sẽ từ từ dừng lại mượt mà

            BoxCollider2D col = dummy.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 0.8f);

            HealthComponent health = dummy.AddComponent<HealthComponent>();
            SerializedObject healthSo = new SerializedObject(health);
            healthSo.FindProperty("maxHealth").floatValue = hp;
            healthSo.ApplyModifiedProperties();

            dummy.AddComponent<HitFlash>();

            DamageOnContact dmg = dummy.AddComponent<DamageOnContact>();
            SerializedObject dmgSo = new SerializedObject(dmg);
            dmgSo.FindProperty("damage").floatValue = 10f;
            dmgSo.FindProperty("targetLayer").intValue = ~0;
            dmgSo.ApplyModifiedProperties();
        }

        private static Sprite CreateCircleSprite(int size, Color color)
        {
            Texture2D tex = new Texture2D(size, size);
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    tex.SetPixel(x, y, dist <= radius ? color : Color.clear);
                }
            }
            tex.filterMode = FilterMode.Bilinear;
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private static Sprite CreateSquareSprite(int width, int height, Color color)
        {
            Texture2D tex = new Texture2D(width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    tex.SetPixel(x, y, color);
                }
            }
            tex.filterMode = FilterMode.Point;
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), Mathf.Max(width, height));
        }
    }
}
#endif
