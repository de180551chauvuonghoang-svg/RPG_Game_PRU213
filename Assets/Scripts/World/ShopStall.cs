using DungeonGame.Combat;
using DungeonGame.Core;
using DungeonGame.Player;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace DungeonGame.World
{
    /// <summary>
    /// Gian Hàng Ma Thuật (Mystic Upgrade Shrine) xuất hiện trên Map khi đạt các mốc điểm.
    /// - Khi mở shop: TỰ ĐỘNG PAUSE GAME (Time.timeScale = 0), quái dừng lại hoàn toàn, không mất máu.
    /// - Hiển thị ảnh vũ khí mới xem trước, tên vũ khí mới và các thuộc tính so sánh trực quan.
    /// - Khi chọn: THAY ĐỔI NGOẠI HÌNH VŨ KHÍ trên tay nhân vật ngay lập tức và unpause game.
    /// </summary>
    public class ShopStall : MonoBehaviour
    {
        [Header("Khoảng cách tương tác")]
        [SerializeField] private float interactRadius = 3.2f;

        [Header("Sprites Vũ Khí Mới Nâng Cấp")]
        [SerializeField] private Sprite multiShotSprite;
        [SerializeField] private Sprite damageSprite;
        [SerializeField] private Sprite speedSprite;
        [SerializeField] private Sprite holyPotionSprite;

        private Transform playerTransform;
        private PlayerCombat playerCombat;
        private HealthComponent playerHealth;

        private bool isPlayerNearby = false;
        private bool isShopOpen = false;
        private bool isUsed = false;

        // Textures
        private Texture2D shopBgTex;
        private Texture2D borderTex;
        private Texture2D cardBgTex;
        private Texture2D bannerGlowTex;

        // Styles
        private GUIStyle shopTitleStyle;
        private GUIStyle shopPauseNoticeStyle;
        private GUIStyle cardTitleStyle;
        private GUIStyle cardStatStyle;
        private GUIStyle cardDescStyle;
        private GUIStyle upgradeBtnStyle;
        private GUIStyle promptStyle;

        private void Start()
        {
            FindPlayer();
            CreateShopTextures();
            AutoLoadSpritesIfMissing();
        }

        private void OnDisable()
        {
            if (isShopOpen)
            {
                CloseShop();
            }
        }

        private void OnDestroy()
        {
            if (isShopOpen)
            {
                CloseShop();
            }
        }

        private void FindPlayer()
        {
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            if (player != null)
            {
                playerTransform = player.transform;
                playerCombat = player.GetComponent<PlayerCombat>();
                playerHealth = player.GetComponent<HealthComponent>();
            }
        }

        private void AutoLoadSpritesIfMissing()
        {
#if UNITY_EDITOR
            if (multiShotSprite == null)
                multiShotSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Weapon_Staff_Multi.png");
            if (damageSprite == null)
                damageSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Weapon_Staff_Titan.png");
            if (speedSprite == null)
                speedSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Weapon_Staff_Speed.png");
            if (holyPotionSprite == null)
                holyPotionSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Item_HolyPotion.png");
#endif
        }

        public void SetUpgradeSprites(Sprite multi, Sprite damage, Sprite speed, Sprite potion)
        {
            multiShotSprite = multi;
            damageSprite = damage;
            speedSprite = speed;
            holyPotionSprite = potion;
        }

        private void CreateShopTextures()
        {
            shopBgTex = MakeTex(new Color(0.08f, 0.06f, 0.12f, 0.98f));
            borderTex = MakeTex(new Color(1f, 0.85f, 0.2f, 1f));
            cardBgTex = MakeTex(new Color(0.15f, 0.13f, 0.22f, 0.95f));
            bannerGlowTex = MakeTex(new Color(0.2f, 0.85f, 0.4f, 0.25f));
        }

        private Texture2D MakeTex(Color c)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, c);
            tex.Apply();
            return tex;
        }

        private void InitStyles()
        {
            if (shopTitleStyle != null) return;

            shopTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            shopTitleStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);

            shopPauseNoticeStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            shopPauseNoticeStyle.normal.textColor = new Color(0.4f, 1f, 0.6f);

            cardTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            cardTitleStyle.normal.textColor = new Color(1f, 0.9f, 0.3f);

            cardStatStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            cardStatStyle.normal.textColor = Color.white;

            cardDescStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Italic,
                alignment = TextAnchor.MiddleLeft
            };
            cardDescStyle.normal.textColor = new Color(0.8f, 0.85f, 0.95f);

            promptStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            promptStyle.normal.textColor = Color.yellow;

            upgradeBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            upgradeBtnStyle.normal.textColor = Color.yellow;
        }

        private void Update()
        {
            if (playerTransform == null)
            {
                FindPlayer();
                if (playerTransform == null) return;
            }

            float dist = Vector2.Distance(transform.position, playerTransform.position);
            isPlayerNearby = dist <= interactRadius;

            if (isPlayerNearby && !isUsed && !isShopOpen)
            {
                bool pressE = false;
#if ENABLE_INPUT_SYSTEM
                if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) pressE = true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
                try { if (Input.GetKeyDown(KeyCode.E)) pressE = true; } catch {}
#endif
                if (pressE || dist <= 1.8f)
                {
                    OpenShop();
                }
            }
            else if (!isPlayerNearby && isShopOpen)
            {
                CloseShop();
            }
        }

        private void OpenShop()
        {
            if (isShopOpen) return;
            isShopOpen = true;
            Time.timeScale = 0f; // DỪNG GAME NGAY LẬP TỨC ĐỂ QUÁI VẬT BỊ ĐÓNG BĂNG
            FindPlayer();
        }

        private void CloseShop()
        {
            if (!isShopOpen) return;
            isShopOpen = false;
            Time.timeScale = 1f; // TIẾP TỤC CHƠI BÌNH THƯỜNG
        }

        private void OnGUI()
        {
            InitStyles();

            // 1. HIỆN GỢI Ý KHI PLAYER ĐẾN GẦN GIAN HÀNG
            if (isPlayerNearby && !isShopOpen && !isUsed)
            {
                float pw = 450f;
                float ph = 45f;
                float px = (Screen.width - pw) / 2f;
                float py = Screen.height - 120f;

                GUI.Box(new Rect(px, py, pw, ph), "⚡ BƯỚC VÀO HOẶC NHẤN 'E' ĐỂ NÂNG CẤP VŨ KHÍ ⚡", promptStyle);
            }

            // 2. BẢNG NÂNG CẤP VŨ KHÍ CHI TIẾT (ĐÃ PAUSE QUÁI)
            if (isShopOpen && !isUsed)
            {
                float sw = 840f;
                float sh = 520f;
                float sx = (Screen.width - sw) / 2f;
                float sy = (Screen.height - sh) / 2f;

                // Nền và viền vàng kim
                GUI.DrawTexture(new Rect(sx - 4, sy - 4, sw + 8, sh + 8), borderTex);
                GUI.DrawTexture(new Rect(sx, sy, sw, sh), shopBgTex);

                // Tiêu đề & Thông báo tạm dừng
                GUI.Label(new Rect(sx, sy + 10f, sw, 32f), "🏪 GIAN HÀNG MA THUẬT - CHỌN 1 VŨ KHÍ NÂNG CẤP", shopTitleStyle);
                GUI.Label(new Rect(sx, sy + 40f, sw, 22f), "⏸️ [TRÒ CHƠI ĐÃ TẠM DỪNG] Quái vật bị đóng băng, người chơi an toàn tuyệt đối!", shopPauseNoticeStyle);

                float cardX = sx + 25f;
                float cardW = sw - 50f;
                float cardH = 92f;
                float startY = sy + 70f;
                float gapY = 104f;

                // Dữ liệu chỉ số hiện tại
                int curShots = playerCombat != null ? playerCombat.MultiShotCount : 1;
                int nextShots = playerCombat != null ? playerCombat.NextMultiShotCount : 3;

                float curDmg = playerCombat != null ? playerCombat.CurrentDamage : 35f;
                float nextDmg = playerCombat != null ? playerCombat.NextDamage : 56f;

                float curRate = playerCombat != null ? playerCombat.CurrentFireRatePerSec : 6.2f;
                float nextRate = playerCombat != null ? playerCombat.NextFireRatePerSec : 9.3f;

                float curHp = playerHealth != null ? playerHealth.CurrentHealth : 100f;
                float maxHp = playerHealth != null ? playerHealth.MaxHealth : 100f;

                // ================= CARD 1: BẮN 3 TIA / 5 TIA =================
                DrawUpgradeCard(
                    new Rect(cardX, startY, cardW, cardH),
                    multiShotSprite,
                    "🏹 HỎA LONG TAM XẠ (Ngoại hình: Trượng 3 Đầu Thần Lửa)",
                    $"Số tia đạn: [{curShots} tia] ➔ [{nextShots} tia cánh quạt] | Sát thương: {curDmg:F0} x {nextShots}",
                    "Bắn chùm cánh quạt phủ rộng toàn màn hình, càn quét toàn bộ đàn quái vật bao vây!",
                    () => ApplyUpgrade(1)
                );

                // ================= CARD 2: CỰC ĐẠI SÁT THƯƠNG =================
                DrawUpgradeCard(
                    new Rect(cardX, startY + gapY, cardW, cardH),
                    damageSprite,
                    "💥 CỰC ĐẠI SÁT THƯƠNG (Ngoại hình: Đại Trượng Hủy Diệt Titan)",
                    $"Sát thương mỗi phát: [{curDmg:F0} DMG] ➔ [{nextDmg:F0} DMG] (+60% Dame) | Đạn to x1.5",
                    "Thân trượng hắc thạch titan to lớn, bắn ra quả cầu lửa khổng lồ thổi bay quái vật!",
                    () => ApplyUpgrade(2)
                );

                // ================= CARD 3: TỐC ĐỘ BẮN LIÊN HỒI =================
                DrawUpgradeCard(
                    new Rect(cardX, startY + gapY * 2, cardW, cardH),
                    speedSprite,
                    "⚡ PHONG BẠO TỐC ĐỘ (Ngoại hình: Trượng Thần Tốc Phong Lôi)",
                    $"Tốc độ xả đạn: [{curRate:F1} viên/s] ➔ [{nextRate:F1} viên/s] (+50% Tốc độ bắn)",
                    "Trượng cánh thiên thần mạ vàng, xả đạn như bão táp quét sạch kẻ địch trong chớp mắt!",
                    () => ApplyUpgrade(3)
                );

                // ================= CARD 4: HỒI PHỤC ĐẦY MÁU =================
                DrawUpgradeCard(
                    new Rect(cardX, startY + gapY * 3, cardW, cardH),
                    holyPotionSprite,
                    "💖 THÁNH THỦY HỒI PHỤC (Ngoại hình: Tiên Dược Thánh Hoàng Gia)",
                    $"Sinh mệnh hiệp sĩ: [{curHp:F0}/{maxHp:F0} HP] ➔ [HỒI PHỤC ĐẦY ĐỦ 100% MÁU]",
                    "Bình tiên dược cứu sinh khẩn cấp, hồi phục toàn bộ lượng máu bị mất lập tức!",
                    () => ApplyUpgrade(4)
                );

                // Nút đóng góc trên
                if (GUI.Button(new Rect(sx + sw - 45f, sy + 10f, 35f, 30f), "X", upgradeBtnStyle))
                {
                    CloseShop();
                }
            }
        }

        private void DrawUpgradeCard(Rect rect, Sprite iconSprite, string title, string stats, string desc, System.Action onSelect)
        {
            // Nền Card
            GUI.DrawTexture(rect, cardBgTex);

            // Icon Vũ Khí (Bên trái)
            Rect iconBox = new Rect(rect.x + 10f, rect.y + 10f, 90f, 72f);
            GUI.Box(iconBox, GUIContent.none);
            if (iconSprite != null && iconSprite.texture != null)
            {
                GUI.DrawTexture(iconBox, iconSprite.texture, ScaleMode.ScaleToFit);
            }

            // Thông tin & Chỉ số so sánh (Ở giữa)
            float infoX = rect.x + 115f;
            float infoW = rect.width - 275f;

            GUI.Label(new Rect(infoX, rect.y + 8f, infoW, 26f), title, cardTitleStyle);
            GUI.Label(new Rect(infoX, rect.y + 35f, infoW, 24f), stats, cardStatStyle);
            GUI.Label(new Rect(infoX, rect.y + 60f, infoW, 22f), desc, cardDescStyle);

            // Nút Chọn Nâng Cấp (Bên phải)
            float btnW = 140f;
            float btnH = 60f;
            float btnX = rect.x + rect.width - btnW - 12f;
            float btnY = rect.y + 16f;

            if (GUI.Button(new Rect(btnX, btnY, btnW, btnH), "✨ NÂNG CẤP ✨", upgradeBtnStyle))
            {
                onSelect?.Invoke();
            }
        }

        private void ApplyUpgrade(int option)
        {
            if (playerCombat == null || playerHealth == null) FindPlayer();

            switch (option)
            {
                case 1:
                    if (playerCombat != null) playerCombat.UpgradeMultiShot(multiShotSprite);
                    break;
                case 2:
                    if (playerCombat != null) playerCombat.UpgradeDamage(damageSprite);
                    break;
                case 3:
                    if (playerCombat != null) playerCombat.UpgradeFireRate(speedSprite);
                    break;
                case 4:
                    if (playerHealth != null) playerHealth.Heal(100f);
                    break;
            }

            isUsed = true;
            CloseShop(); // UNPAUSE VÀ TIẾP TỤC CHƠI

            // Đổi màu gian hàng sang màu xám sau khi mua xong
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = new Color(0.5f, 0.5f, 0.6f, 0.7f);

            // Tự hủy gian hàng sau 5 giây để dọn map
            Destroy(gameObject, 5f);
        }
    }
}
