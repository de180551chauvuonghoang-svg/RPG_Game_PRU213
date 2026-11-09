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
        private bool requireStepAway = false;
        private float closeCooldownTimer = 0f;

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

        private GUIStyle coinWalletStyle;
        private GUIStyle cardLevelBadgeStyle;

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

            coinWalletStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false
            };
            coinWalletStyle.normal.textColor = new Color(1f, 0.90f, 0.2f);

            cardTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = false,
                clipping = TextClipping.Clip
            };
            cardTitleStyle.normal.textColor = new Color(1f, 0.88f, 0.25f);

            cardLevelBadgeStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = false
            };
            cardLevelBadgeStyle.normal.textColor = new Color(0.3f, 0.9f, 1f);

            cardStatStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = false,
                clipping = TextClipping.Clip
            };
            cardStatStyle.normal.textColor = new Color(0.5f, 1f, 0.5f);

            cardDescStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
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
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            upgradeBtnStyle.normal.textColor = new Color(1f, 0.95f, 0.3f);

            closeBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            closeBtnStyle.normal.textColor = Color.white;
        }

        private void Update()
        {
            if (isUsed) return;

            if (closeCooldownTimer > 0f)
            {
                closeCooldownTimer -= Time.unscaledDeltaTime;
            }

            if (playerTransform == null)
            {
                FindPlayer();
                if (playerTransform == null) return;
            }

            float dist = Vector2.Distance(transform.position, playerTransform.position);
            isPlayerNearby = dist <= interactRadius;

            // Nếu người chơi bước ra ngoài phạm vi tương tác thì xóa cờ requireStepAway
            if (dist > 2.4f)
            {
                requireStepAway = false;
            }

            if (isPlayerNearby && !isShopOpen)
            {
                bool pressE = false;
#if ENABLE_INPUT_SYSTEM
                if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) pressE = true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
                try { if (Input.GetKeyDown(KeyCode.E)) pressE = true; } catch {}
#endif
                // Tự động mở khi bước vào lần đầu hoặc khi bấm 'E'
                // Nếu vừa đóng bằng Space/ESC thì không tự động bung lại trừ phi bước ra xa rồi quay lại hoặc bấm 'E'
                if (pressE || (dist <= 1.8f && !requireStepAway && closeCooldownTimer <= 0f))
                {
                    OpenShop();
                }
            }
            else if (!isPlayerNearby && isShopOpen)
            {
                CloseShop();
            }

            // Hỗ trợ phím tắt 1, 2, 3, 4, Space, Escape khi mở shop
            if (isShopOpen)
            {
#if ENABLE_INPUT_SYSTEM
                if (Keyboard.current != null)
                {
                    if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
                    {
                        CloseShop();
                        return;
                    }
                    if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame) ApplyUpgrade(1);
                    else if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame) ApplyUpgrade(2);
                    else if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame) ApplyUpgrade(3);
                    else if (Keyboard.current.digit4Key.wasPressedThisFrame || Keyboard.current.numpad4Key.wasPressedThisFrame) ApplyUpgrade(4);
                }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
                try
                {
                    if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
                    {
                        CloseShop();
                        return;
                    }
                    if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) ApplyUpgrade(1);
                    else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) ApplyUpgrade(2);
                    else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) ApplyUpgrade(3);
                    else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) ApplyUpgrade(4);
                }
                catch {}
#endif
            }
        }

        private void OpenShop()
        {
            if (isShopOpen) return;
            isShopOpen = true;
            Time.timeScale = 0f; // DỪNG GAME ĐỂ QUÁI VẬT BỊ ĐÓNG BĂNG HOÀN TOÀN
            FindPlayer();
        }

        private void CloseShop()
        {
            if (!isShopOpen) return;
            isShopOpen = false;
            requireStepAway = true;
            closeCooldownTimer = 1.2f;
            Time.timeScale = 1f; // TIẾP TỤC CHƠI BÌNH THƯỜNG
            Debug.Log("<color=cyan><b>[SHOP] Đã đóng gian hàng. Bạn có thể tiếp tục chiến đấu và ghé lại bất cứ lúc nào!</b></color>");
        }

        private void OnGUI()
        {
            if (isUsed) return;

            InitStyles();

            // Bắt phím Space / Escape ngay trong OnGUI để đóng shop siêu nhạy và mượt mà
            if (isShopOpen && Event.current != null && Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.Space || Event.current.keyCode == KeyCode.Escape)
                {
                    CloseShop();
                    Event.current.Use();
                    return;
                }
            }

            // 1. HIỆN GỢI Ý KHI PLAYER ĐẾN GẦN GIAN HÀNG
            if (isPlayerNearby && !isShopOpen)
            {
                float pw = 540f;
                float ph = 48f;
                float px = (Screen.width - pw) / 2f;
                float py = Screen.height - 120f;

                int curCoins = GameManager.Instance != null ? GameManager.Instance.Coins : 0;
                string promptMsg = requireStepAway
                    ? $"⚡ NHẤN 'E' HOẶC BƯỚC VÀO ĐỂ MỞ LẠI GIAN HÀNG (BẠN CÓ: {curCoins} COIN) ⚡"
                    : $"⚡ NHẤN 'E' HOẶC BƯỚC VÀO ĐỂ NÂNG CẤP (BẠN CÓ: {curCoins} COIN) ⚡";
                GUI.Box(new Rect(px, py, pw, ph), promptMsg, promptStyle);
            }

            // 2. BẢNG NÂNG CẤP VŨ KHÍ RỘNG RÃI
            if (isShopOpen)
            {
                float sw = Mathf.Min(Screen.width - 40f, 1060f);
                float sh = Mathf.Min(Screen.height - 30f, 650f);
                float sx = (Screen.width - sw) / 2f;
                float sy = (Screen.height - sh) / 2f;

                // Nền và viền vàng kim
                GUI.DrawTexture(new Rect(sx - 4, sy - 4, sw + 8, sh + 8), borderTex);
                GUI.DrawTexture(new Rect(sx, sy, sw, sh), shopBgTex);

                int playerCoins = GameManager.Instance != null ? GameManager.Instance.Coins : 0;

                // Tiêu đề & Thông báo túi tiền vàng
                GUI.Label(new Rect(sx, sy + 10f, sw, 30f), "🏪 GIAN HÀNG MA THUẬT - NÂNG CẤP VŨ KHÍ RPG", shopTitleStyle);
                GUI.Label(new Rect(sx, sy + 38f, sw, 22f), $"⏸️ [GAME TẠM DỪNG]  •  TÚI TIỀN CỦA BẠN: {playerCoins:N0} COIN  •  (Tối đa Level 2)", coinWalletStyle);

                float cardX = sx + 20f;
                float cardW = sw - 40f;
                float cardH = 110f;
                float startY = sy + 66f;
                float gapY = 118f;

                // Dữ liệu chỉ số hiện tại
                int curShots = playerCombat != null ? playerCombat.MultiShotCount : 1;
                float curDmg = playerCombat != null ? playerCombat.CurrentDamage : 35f;
                float curRate = playerCombat != null ? playerCombat.CurrentFireRatePerSec : 6.2f;

                float curHp = playerHealth != null ? playerHealth.CurrentHealth : 100f;
                float maxHp = playerHealth != null ? playerHealth.MaxHealth : 100f;

                // ================= CARD 1: BẮN ĐA TIA (MULTI-SHOT) =================
                int multiLvl = playerCombat != null ? playerCombat.MultiShotLevel : 0;
                bool isMultiMaxed = playerCombat != null && playerCombat.IsMultiShotMaxed;
                int multiCost = playerCombat != null ? playerCombat.GetUpgradeCost(multiLvl) : 15;
                bool canAffordMulti = playerCoins >= multiCost && !isMultiMaxed;

                string multiBadge = isMultiMaxed ? "⭐ CẤP ĐỘ: TỐI ĐA (LEVEL 2/2 - MAX)" : $"⚡ CẤP ĐỘ: LEVEL {multiLvl}/2  ➔  NÂNG CẤP LÊN LEVEL {multiLvl + 1} (GIÁ: {multiCost} COIN)";
                string multiStats = isMultiMaxed 
                    ? $"👑 ĐÃ ĐẠT CỰC ĐẠI: [5 TIA CÁNH QUẠT]  |  Tổng sát thương: {curDmg:F0} x 5 = {curDmg * 5:F0} DMG" 
                    : $"⚡ Số tia đạn: [{curShots} tia] ➔ [{(multiLvl == 0 ? 3 : 5)} tia cánh quạt]  |  Tổng DMG: {curDmg:F0} x {(multiLvl == 0 ? 3 : 5)} = {curDmg * (multiLvl == 0 ? 3 : 5):F0}";
                string multiBtn = isMultiMaxed 
                    ? "👑 ĐÃ ĐẠT TỐI ĐA\n(MAX LEVEL)" 
                    : (canAffordMulti ? $"✨ NÂNG CẤP ({multiCost} COIN) ✨\n(Phím 1)" : $"❌ THIẾU COIN ({multiCost} COIN)\n(Phím 1)");

                DrawUpgradeCard(
                    new Rect(cardX, startY, cardW, cardH),
                    multiShotSprite,
                    "🏹 HỎA LONG PHÂN XẠ  •  [Trượng Thần Hỏa]",
                    multiBadge,
                    multiStats,
                    "Phủ hỏa lực cánh quạt diện rộng, càn quét toàn bộ đàn quái vật bao vây xung quanh!",
                    multiBtn,
                    canAffordMulti && !isMultiMaxed,
                    () => ApplyUpgrade(1)
                );

                // ================= CARD 2: CỰC ĐẠI SÁT THƯƠNG =================
                int dmgLvl = playerCombat != null ? playerCombat.DamageLevel : 0;
                bool isDmgMaxed = playerCombat != null && playerCombat.IsDamageMaxed;
                int dmgCost = playerCombat != null ? playerCombat.GetUpgradeCost(dmgLvl) : 15;
                bool canAffordDmg = playerCoins >= dmgCost && !isDmgMaxed;

                string dmgBadge = isDmgMaxed ? "⭐ CẤP ĐỘ: TỐI ĐA (LEVEL 2/2 - MAX)" : $"⚡ CẤP ĐỘ: LEVEL {dmgLvl}/2  ➔  NÂNG CẤP LÊN LEVEL {dmgLvl + 1} (GIÁ: {dmgCost} COIN)";
                float nextDmgVal = playerCombat != null ? playerCombat.NextDamage : curDmg * 1.6f;
                string dmgStats = isDmgMaxed 
                    ? $"👑 ĐÃ ĐẠT CỰC ĐẠI: +120% DMG [{curDmg:F0} DMG/phát]  |  Đạn Titan bộc phá x1.7" 
                    : $"⚡ Sát thương mỗi phát: [{curDmg:F0} DMG] ➔ [{nextDmgVal:F0} DMG] (+60% DMG)  |  Đạn to hơn";
                string dmgBtn = isDmgMaxed 
                    ? "👑 ĐÃ ĐẠT TỐI ĐA\n(MAX LEVEL)" 
                    : (canAffordDmg ? $"✨ NÂNG CẤP ({dmgCost} COIN) ✨\n(Phím 2)" : $"❌ THIẾU COIN ({dmgCost} COIN)\n(Phím 2)");

                DrawUpgradeCard(
                    new Rect(cardX, startY + gapY, cardW, cardH),
                    damageSprite,
                    "💥 CỰC ĐẠI SÁT THƯƠNG  •  [Đại Trượng Hủy Diệt Titan]",
                    dmgBadge,
                    dmgStats,
                    "Thân hắc thạch titan to lớn, bắn đại hỏa cầu khổng lồ thổi bay mọi kẻ địch!",
                    dmgBtn,
                    canAffordDmg && !isDmgMaxed,
                    () => ApplyUpgrade(2)
                );

                // ================= CARD 3: TỐC ĐỘ BẮN LIÊN HỒI =================
                int spdLvl = playerCombat != null ? playerCombat.FireRateLevel : 0;
                bool isSpdMaxed = playerCombat != null && playerCombat.IsFireRateMaxed;
                int spdCost = playerCombat != null ? playerCombat.GetUpgradeCost(spdLvl) : 15;
                bool canAffordSpd = playerCoins >= spdCost && !isSpdMaxed;

                string spdBadge = isSpdMaxed ? "⭐ CẤP ĐỘ: TỐI ĐA (LEVEL 2/2 - MAX)" : $"⚡ CẤP ĐỘ: LEVEL {spdLvl}/2  ➔  NÂNG CẤP LÊN LEVEL {spdLvl + 1} (GIÁ: {spdCost} COIN)";
                float nextRateVal = playerCombat != null ? playerCombat.NextFireRatePerSec : curRate * 1.5f;
                string spdStats = isSpdMaxed 
                    ? $"👑 ĐÃ ĐẠT CỰC ĐẠI: +100% TỐC ĐỘ [{curRate:F1} viên/s]  |  Xả đạn bão táp liên hoàn" 
                    : $"⚡ Tốc độ xả đạn: [{curRate:F1} viên/s] ➔ [{nextRateVal:F1} viên/s] (+50% Tốc độ bắn)";
                string spdBtn = isSpdMaxed 
                    ? "👑 ĐÃ ĐẠT TỐI ĐA\n(MAX LEVEL)" 
                    : (canAffordSpd ? $"✨ NÂNG CẤP ({spdCost} COIN) ✨\n(Phím 3)" : $"❌ THIẾU COIN ({spdCost} COIN)\n(Phím 3)");

                DrawUpgradeCard(
                    new Rect(cardX, startY + gapY * 2, cardW, cardH),
                    speedSprite,
                    "⚡ PHONG BẠO TỐC ĐỘ  •  [Trượng Thần Tốc Phong Lôi]",
                    spdBadge,
                    spdStats,
                    "Cánh thiên thần mạ vàng thanh thoát, xả đạn như bão táp quét sạch kẻ địch trong chớp mắt!",
                    spdBtn,
                    canAffordSpd && !isSpdMaxed,
                    () => ApplyUpgrade(3)
                );

                // ================= CARD 4: HỒI PHỤC ĐẦY MÁU =================
                int potionCost = 10;
                bool isHpFull = curHp >= maxHp;
                bool canAffordPotion = playerCoins >= potionCost && !isHpFull;

                string potionBadge = isHpFull ? "💚 TRẠNG THÁI: SINH LỰC ĐẦY ĐỦ 100%" : $"💖 TIÊN DƯỢC CỨU SINH (GIÁ: {potionCost} COIN)";
                string potionStats = $"⚡ Sinh mệnh hiệp sĩ: [{curHp:F0}/{maxHp:F0} HP] ➔ [HỒI PHỤC 100% ĐẦY BÌNH MÁU]";
                string potionBtn = isHpFull 
                    ? "💚 MÁU ĐÃ ĐẦY 100%\n(Không cần hồi)" 
                    : (canAffordPotion ? $"💖 HỒI MÁU ({potionCost} COIN)\n(Phím 4)" : $"❌ THIẾU COIN ({potionCost} COIN)\n(Phím 4)");

                DrawUpgradeCard(
                    new Rect(cardX, startY + gapY * 3, cardW, cardH),
                    holyPotionSprite,
                    "💖 THÁNH THỦY HỒI PHỤC  •  [Tiên Dược Thánh Hoàng Gia]",
                    potionBadge,
                    potionStats,
                    "Bình tiên dược cứu sinh khẩn cấp, lập tức hồi phục đầy đủ 100% lượng máu của hiệp sĩ!",
                    potionBtn,
                    canAffordPotion && !isHpFull,
                    () => ApplyUpgrade(4)
                );

                // Nút hoàn tất mua sắm / Tiếp tục chiến đấu ở đáy popup
                float finishBtnW = 380f;
                float finishBtnH = 40f;
                float finishBtnX = sx + (sw - finishBtnW) / 2f;
                float finishBtnY = sy + sh - 48f;

                if (GUI.Button(new Rect(finishBtnX, finishBtnY, finishBtnW, finishBtnH), "⚔️ TIẾP TỤC CHIẾN ĐẤU (SPACE / ESC) ⚔️", closeBtnStyle))
                {
                    CloseShop();
                }

                // Nút đóng 'X' góc trên bên phải
                if (GUI.Button(new Rect(sx + sw - 44f, sy + 10f, 34f, 30f), "X", closeBtnStyle))
                {
                    CloseShop();
                }
            }
        }

        private void DrawUpgradeCard(Rect rect, Sprite iconSprite, string headerLine, string levelBadge, string statsLine, string descLine, string btnText, bool isEnabled, System.Action onSelect)
        {
            // 1. Nền Card & Viền
            GUI.DrawTexture(new Rect(rect.x - 1, rect.y - 1, rect.width + 2, rect.height + 2), cardBorderTex);
            GUI.DrawTexture(rect, cardBgTex);

            // 2. Khung Icon Vũ Khí (Bên trái) - Khung 106x86 px
            Rect iconBorder = new Rect(rect.x + 12f, rect.y + 12f, 106f, 86f);
            GUI.DrawTexture(iconBorder, iconBorderTex);
            Rect iconBox = new Rect(rect.x + 14f, rect.y + 14f, 102f, 82f);
            GUI.DrawTexture(iconBox, iconBgTex);

            if (iconSprite != null && iconSprite.texture != null)
            {
                Rect spriteRect = new Rect(iconBox.x + 6f, iconBox.y + 6f, iconBox.width - 12f, iconBox.height - 12f);
                GUI.DrawTexture(spriteRect, iconSprite.texture, ScaleMode.ScaleToFit);
            }

            // 3. Nút Chọn Nâng Cấp (Bên phải)
            float btnW = 180f;
            float btnH = 68f;
            float btnX = rect.x + rect.width - btnW - 14f;
            float btnY = rect.y + 21f;

            GUI.enabled = isEnabled;
            if (GUI.Button(new Rect(btnX, btnY, btnW, btnH), btnText, upgradeBtnStyle))
            {
                onSelect?.Invoke();
            }
            GUI.enabled = true;

            // 4. Khối Thông Tin & Chỉ Số (Ở giữa)
            float infoX = rect.x + 130f;
            float infoW = btnX - infoX - 12f;

            // Dòng 1: Tiêu đề vũ khí
            GUI.Label(new Rect(infoX, rect.y + 8f, infoW, 24f), headerLine, cardTitleStyle);

            // Dòng 2: Cấp độ nâng cấp & Giá
            GUI.Label(new Rect(infoX, rect.y + 32f, infoW, 22f), levelBadge, cardLevelBadgeStyle);

            // Dòng 3: Chỉ số Thuộc tính So sánh (Trước ➔ Sau)
            GUI.Label(new Rect(infoX, rect.y + 55f, infoW, 22f), statsLine, cardStatStyle);

            // Dòng 4: Mô tả Hiệu Quả Chiến Đấu
            GUI.Label(new Rect(infoX, rect.y + 78f, infoW, 22f), descLine, cardDescStyle);
        }

        private void ApplyUpgrade(int option)
        {
            if (isUsed) return;
            if (playerCombat == null || playerHealth == null) FindPlayer();
            if (GameManager.Instance == null) return;

            bool upgradeSuccess = false;

            switch (option)
            {
                case 1:
                    if (playerCombat != null && !playerCombat.IsMultiShotMaxed)
                    {
                        int cost = playerCombat.GetUpgradeCost(playerCombat.MultiShotLevel);
                        if (GameManager.Instance.SpendCoins(cost))
                        {
                            playerCombat.UpgradeMultiShot(multiShotSprite);
                            upgradeSuccess = true;
                        }
                    }
                    break;

                case 2:
                    if (playerCombat != null && !playerCombat.IsDamageMaxed)
                    {
                        int cost = playerCombat.GetUpgradeCost(playerCombat.DamageLevel);
                        if (GameManager.Instance.SpendCoins(cost))
                        {
                            playerCombat.UpgradeDamage(damageSprite);
                            upgradeSuccess = true;
                        }
                    }
                    break;

                case 3:
                    if (playerCombat != null && !playerCombat.IsFireRateMaxed)
                    {
                        int cost = playerCombat.GetUpgradeCost(playerCombat.FireRateLevel);
                        if (GameManager.Instance.SpendCoins(cost))
                        {
                            playerCombat.UpgradeFireRate(speedSprite);
                            upgradeSuccess = true;
                        }
                    }
                    break;

                case 4:
                    if (playerHealth != null && playerHealth.CurrentHealth < playerHealth.MaxHealth)
                    {
                        if (GameManager.Instance.SpendCoins(10))
                        {
                            playerHealth.Heal(100f);
                            upgradeSuccess = true;
                        }
                    }
                    break;
            }

            if (upgradeSuccess)
            {
                FinishShopAfterUpgrade();
            }
        }

        private void FinishShopAfterUpgrade()
        {
            isUsed = true;
            CloseShop();

            if (UpgradeShopSpawner.Instance != null)
            {
                UpgradeShopSpawner.Instance.NotifyShopUpgraded(this);
            }

            Debug.Log("<color=green><b>[SHOP] Nâng cấp thành công! Gian hàng đã hoàn thành nhiệm vụ và rời đi. Gian hàng tiếp theo sẽ xuất hiện ở mốc điểm kế tiếp!</b></color>");

            Destroy(gameObject, 0.25f);
        }
    }
}
