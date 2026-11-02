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
    /// - Giao diện Card Roguelike cỡ lớn, rộng rãi, tuyệt đối KHÔNG BỊ ĐÈ CHỮ / RỚT DÒNG (wordWrap = false).
    /// - Hỗ trợ phím tắt 1, 2, 3, 4 hoặc click chuột để chọn nâng cấp.
    /// - Đổi trực tiếp ngoại hình vũ khí trên tay nhân vật khi người chơi bấm chọn.
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
        private Texture2D cardBorderTex;
        private Texture2D iconBgTex;
        private Texture2D iconBorderTex;

        // Styles
        private GUIStyle shopTitleStyle;
        private GUIStyle shopPauseNoticeStyle;
        private GUIStyle cardTitleStyle;
        private GUIStyle cardStatStyle;
        private GUIStyle cardDescStyle;
        private GUIStyle upgradeBtnStyle;
        private GUIStyle promptStyle;
        private GUIStyle closeBtnStyle;

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
            shopBgTex = MakeTex(new Color(0.07f, 0.06f, 0.11f, 0.98f));
            borderTex = MakeTex(new Color(1f, 0.82f, 0.2f, 1f));
            cardBgTex = MakeTex(new Color(0.13f, 0.13f, 0.20f, 0.97f));
            cardBorderTex = MakeTex(new Color(0.4f, 0.35f, 0.55f, 0.8f));
            iconBgTex = MakeTex(new Color(0.05f, 0.05f, 0.09f, 1f));
            iconBorderTex = MakeTex(new Color(1f, 0.75f, 0.2f, 0.7f));
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
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false
            };
            shopTitleStyle.normal.textColor = new Color(1f, 0.85f, 0.25f);

            shopPauseNoticeStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false
            };
            shopPauseNoticeStyle.normal.textColor = new Color(0.35f, 1f, 0.65f);

            cardTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = false,
                clipping = TextClipping.Clip
            };
            cardTitleStyle.normal.textColor = new Color(1f, 0.88f, 0.25f);

            cardStatStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = false,
                clipping = TextClipping.Clip
            };
            cardStatStyle.normal.textColor = new Color(0.5f, 1f, 0.5f);

            cardDescStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Italic,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = false,
                clipping = TextClipping.Clip
            };
            cardDescStyle.normal.textColor = new Color(0.80f, 0.86f, 0.95f);

            promptStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 19,
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
            upgradeBtnStyle.normal.textColor = new Color(1f, 0.95f, 0.3f);

            closeBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            closeBtnStyle.normal.textColor = Color.white;
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

            // Hỗ trợ phím tắt 1, 2, 3, 4 và ESC khi mở shop
            if (isShopOpen && !isUsed)
            {
#if ENABLE_INPUT_SYSTEM
                if (Keyboard.current != null)
                {
                    if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame) ApplyUpgrade(1);
                    else if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame) ApplyUpgrade(2);
                    else if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame) ApplyUpgrade(3);
                    else if (Keyboard.current.digit4Key.wasPressedThisFrame || Keyboard.current.numpad4Key.wasPressedThisFrame) ApplyUpgrade(4);
                    else if (Keyboard.current.escapeKey.wasPressedThisFrame) CloseShop();
                }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
                try
                {
                    if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) ApplyUpgrade(1);
                    else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) ApplyUpgrade(2);
                    else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) ApplyUpgrade(3);
                    else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) ApplyUpgrade(4);
                    else if (Input.GetKeyDown(KeyCode.Escape)) CloseShop();
                }
                catch {}
#endif
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
                float pw = 480f;
                float ph = 48f;
                float px = (Screen.width - pw) / 2f;
                float py = Screen.height - 120f;

                GUI.Box(new Rect(px, py, pw, ph), "⚡ BƯỚC VÀO HOẶC NHẤN 'E' ĐỂ NÂNG CẤP VŨ KHÍ ⚡", promptStyle);
            }

            // 2. BẢNG NÂNG CẤP VŨ KHÍ RỘNG RÃI, ĐẸP MẮT
            if (isShopOpen && !isUsed)
            {
                // Tự động căn chỉnh kích thước Popup cân đối với mọi độ phân giải màn hình
                float sw = Mathf.Min(Screen.width - 40f, 1040f);
                float sh = Mathf.Min(Screen.height - 40f, 620f);
                float sx = (Screen.width - sw) / 2f;
                float sy = (Screen.height - sh) / 2f;

                // Nền và viền vàng kim
                GUI.DrawTexture(new Rect(sx - 4, sy - 4, sw + 8, sh + 8), borderTex);
                GUI.DrawTexture(new Rect(sx, sy, sw, sh), shopBgTex);

                // Tiêu đề & Thông báo tạm dừng
                GUI.Label(new Rect(sx, sy + 10f, sw, 32f), "🏪 GIAN HÀNG MA THUẬT - CHỌN 1 VŨ KHÍ NÂNG CẤP", shopTitleStyle);
                GUI.Label(new Rect(sx, sy + 40f, sw, 22f), "⏸️ [TRÒ CHƠI ĐÃ TẠM DỪNG] Quái vật bị đóng băng 100% - Bạn an toàn tuyệt đối!", shopPauseNoticeStyle);

                float cardX = sx + 20f;
                float cardW = sw - 40f;
                float cardH = 112f;
                float startY = sy + 68f;
                float gapY = 124f;

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
                    "🏹 HỎA LONG TAM XẠ  •  [Trượng 3 Nhánh Thần Hỏa]",
                    $"⚡ Số tia đạn: [{curShots} tia] ➔ [{nextShots} tia cánh quạt]  |  Tổng sát thương: {curDmg:F0} x {nextShots} = {curDmg * nextShots:F0} DMG",
                    "Phủ hỏa lực cánh quạt diện rộng, càn quét toàn bộ đàn quái vật bao vây xung quanh!",
                    "CHỌN [1]",
                    () => ApplyUpgrade(1)
                );

                // ================= CARD 2: CỰC ĐẠI SÁT THƯƠNG =================
                DrawUpgradeCard(
                    new Rect(cardX, startY + gapY, cardW, cardH),
                    damageSprite,
                    "💥 CỰC ĐẠI SÁT THƯƠNG  •  [Đại Trượng Hủy Diệt Titan]",
                    $"⚡ Sát thương mỗi phát: [{curDmg:F0} DMG] ➔ [{nextDmg:F0} DMG] (+60% Dame)  |  Đạn to x1.5",
                    "Thân hắc thạch titan to lớn, bắn đại hỏa cầu khổng lồ thổi bay mọi kẻ địch!",
                    "CHỌN [2]",
                    () => ApplyUpgrade(2)
                );

                // ================= CARD 3: TỐC ĐỘ BẮN LIÊN HỒI =================
                DrawUpgradeCard(
                    new Rect(cardX, startY + gapY * 2, cardW, cardH),
                    speedSprite,
                    "⚡ PHONG BẠO TỐC ĐỘ  •  [Trượng Thần Tốc Phong Lôi]",
                    $"⚡ Tốc độ xả đạn: [{curRate:F1} viên/s] ➔ [{nextRate:F1} viên/s] (+50% Tốc độ bắn)",
                    "Cánh thiên thần mạ vàng thanh thoát, xả đạn như bão táp quét sạch kẻ địch trong chớp mắt!",
                    "CHỌN [3]",
                    () => ApplyUpgrade(3)
                );

                // ================= CARD 4: HỒI PHỤC ĐẦY MÁU =================
                DrawUpgradeCard(
                    new Rect(cardX, startY + gapY * 3, cardW, cardH),
                    holyPotionSprite,
                    "💖 THÁNH THỦY HỒI PHỤC  •  [Tiên Dược Thánh Hoàng Gia]",
                    $"⚡ Sinh mệnh hiệp sĩ: [{curHp:F0}/{maxHp:F0} HP] ➔ [HỒI PHỤC 100% ĐẦY BÌNH MÁU]",
                    "Bình tiên dược cứu sinh khẩn cấp, lập tức hồi phục đầy đủ 100% lượng máu của hiệp sĩ!",
                    "CHỌN [4]",
                    () => ApplyUpgrade(4)
                );

                // Nút đóng góc trên
                if (GUI.Button(new Rect(sx + sw - 46f, sy + 10f, 36f, 32f), "X", closeBtnStyle))
                {
                    CloseShop();
                }
            }
        }

        private void DrawUpgradeCard(Rect rect, Sprite iconSprite, string headerLine, string statsLine, string descLine, string shortcutKey, System.Action onSelect)
        {
            // 1. Nền Card & Viền
            GUI.DrawTexture(new Rect(rect.x - 1, rect.y - 1, rect.width + 2, rect.height + 2), cardBorderTex);
            GUI.DrawTexture(rect, cardBgTex);

            // 2. Khung Icon Vũ Khí (Bên trái) - Khung 106x86 px
            Rect iconBorder = new Rect(rect.x + 12f, rect.y + 13f, 108f, 86f);
            GUI.DrawTexture(iconBorder, iconBorderTex);
            Rect iconBox = new Rect(rect.x + 14f, rect.y + 15f, 104f, 82f);
            GUI.DrawTexture(iconBox, iconBgTex);

            if (iconSprite != null && iconSprite.texture != null)
            {
                Rect spriteRect = new Rect(iconBox.x + 6f, iconBox.y + 6f, iconBox.width - 12f, iconBox.height - 12f);
                GUI.DrawTexture(spriteRect, iconSprite.texture, ScaleMode.ScaleToFit);
            }

            // 3. Nút Chọn Nâng Cấp (Bên phải)
            float btnW = 150f;
            float btnH = 66f;
            float btnX = rect.x + rect.width - btnW - 14f;
            float btnY = rect.y + 23f;

            if (GUI.Button(new Rect(btnX, btnY, btnW, btnH), $"✨ NÂNG CẤP ✨\n({shortcutKey})", upgradeBtnStyle))
            {
                onSelect?.Invoke();
            }

            // 4. Khối Thông Tin & Chỉ Số (Ở giữa) - Chiều rộng tính tự động, không bao giờ đè chữ
            float infoX = rect.x + 132f;
            float infoW = btnX - infoX - 12f;

            // Dòng 1: Tiêu đề vũ khí & Tên ngoại hình mới (Riêng biệt, cao 26px)
            GUI.Label(new Rect(infoX, rect.y + 12f, infoW, 26f), headerLine, cardTitleStyle);

            // Dòng 2: Chỉ số Thuộc tính So sánh (Trước ➔ Sau) (Riêng biệt, cao 24px)
            GUI.Label(new Rect(infoX, rect.y + 42f, infoW, 24f), statsLine, cardStatStyle);

            // Dòng 3: Mô tả Hiệu Quả Chiến Đấu (Riêng biệt, cao 22px)
            GUI.Label(new Rect(infoX, rect.y + 70f, infoW, 22f), descLine, cardDescStyle);
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
