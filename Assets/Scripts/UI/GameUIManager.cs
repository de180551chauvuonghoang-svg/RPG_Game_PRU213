using System;
using DungeonGame.Combat;
using DungeonGame.Core;
using DungeonGame.Enemy;
using DungeonGame.Player;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace DungeonGame.UI
{
    /// <summary>
    /// Quản lý toàn bộ giao diện trò chơi:
    /// 1. Màn hình trước khi vào game (Title Screen / Chọn Màn 1 & Màn 2 / Xem màn đã vượt qua)
    /// 2. HUD in-game (Thanh Máu, Điểm, Kills, Đồng hồ)
    /// 3. Thanh Máu Boss Cuối hoành tráng khi đạt mốc điểm triệu hồi
    /// 4. Thông báo Cổng Dịch Chuyển & Bảng Chiến Thắng Vượt Màn sang Màn 2
    /// 5. Màn hình Game Over khi hiệp sĩ hy sinh
    /// </summary>
    public class GameUIManager : MonoBehaviour
    {
        public static GameUIManager Instance { get; private set; }

        private HealthComponent playerHealth;

        // Textures
        private Texture2D hpFillTex;
        private Texture2D hpBgTex;
        private Texture2D borderTex;
        private Texture2D popupBgTex;
        private Texture2D bossFillTex;
        private Texture2D bossBorderTex;
        private Texture2D stageCardBgTex;
        private Texture2D stageCardBorderTex;
        private Texture2D stageBorderTex1;
        private Texture2D stageBorderTex2;
        private Texture2D stageBorderTex3;
        private Texture2D stageBorderTex4;
        private Texture2D flashTex;
        private Texture2D fullScreenMenuBgTex;
        private Texture2D menuHeaderBgTex;
        private Texture2D goldLineTex;

        // Transition Effect Variables
        private float transitionFlashAlpha = 0f;
        private string stageTransitionBanner = "";
        private float stageTransitionTimer = 0f;
        private GUIStyle stageBannerStyle;
        private GUIStyle stageBannerSubStyle;
        private GUIStyle menuGuideStyle;
        private GUIStyle coinHudStyle;

        // GUIStyles
        private GUIStyle titleStyle;
        private GUIStyle scoreStyle;
        private GUIStyle killsStyle;
        private GUIStyle hpTextStyle;
        private GUIStyle gameOverHeaderStyle;
        private GUIStyle gameOverStatsStyle;
        private GUIStyle restartButtonStyle;

        // Menu & Boss & Victory Styles
        private GUIStyle menuHeaderStyle;
        private GUIStyle menuAuthorStyle;
        private GUIStyle stageTitleStyle;
        private GUIStyle stageStatusStyle;
        private GUIStyle stageDescStyle;
        private GUIStyle menuActionBtnStyle;
        private GUIStyle bossTitleStyle;
        private GUIStyle bossNoticeStyle;

        private bool isGameOver = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            CreateTextures();
        }

        private void Start()
        {
            FindPlayerHealth();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
            }
        }

        private void FindPlayerHealth()
        {
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            if (player != null)
            {
                playerHealth = player.GetComponent<HealthComponent>();
            }
        }

        private void CreateTextures()
        {
            hpFillTex = MakeTex(new Color(0.95f, 0.2f, 0.25f, 1f));
            hpBgTex = MakeTex(new Color(0.12f, 0.12f, 0.16f, 0.95f));
            borderTex = MakeTex(new Color(1f, 0.85f, 0.2f, 1f));
            popupBgTex = MakeTex(new Color(0.08f, 0.08f, 0.12f, 0.98f));
            bossFillTex = MakeTex(new Color(0.85f, 0.1f, 0.2f, 1f));
            bossBorderTex = MakeTex(new Color(0.9f, 0.2f, 0.9f, 1f));
            stageCardBgTex = MakeTex(new Color(0.12f, 0.13f, 0.20f, 0.95f));
            stageCardBorderTex = MakeTex(new Color(0.4f, 0.5f, 0.7f, 0.8f));
            stageBorderTex1 = MakeTex(new Color(0.25f, 0.7f, 1f, 0.95f)); // Lam Thạch Màn 1
            stageBorderTex2 = MakeTex(new Color(1f, 0.38f, 0.12f, 0.95f)); // Hỏa Diệm Màn 2
            stageBorderTex3 = MakeTex(new Color(0.4f, 0.95f, 1f, 0.95f)); // Băng Giá Màn 3
            stageBorderTex4 = MakeTex(new Color(1f, 0.82f, 0.2f, 0.95f)); // Lôi Thần Màn 4
            flashTex = MakeTex(new Color(1f, 1f, 1f, 1f));
            fullScreenMenuBgTex = MakeTex(new Color(0.06f, 0.07f, 0.11f, 1f));
            menuHeaderBgTex = MakeTex(new Color(0.09f, 0.10f, 0.16f, 1f));
            goldLineTex = MakeTex(new Color(1f, 0.82f, 0.2f, 1f));
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
            if (scoreStyle != null) return;

            scoreStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 25,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight,
                wordWrap = false
            };
            scoreStyle.normal.textColor = new Color(1f, 0.88f, 0.2f);

            killsStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight,
                wordWrap = false
            };
            killsStyle.normal.textColor = new Color(1f, 0.4f, 0.4f);

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false
            };
            titleStyle.normal.textColor = Color.white;

            coinHudStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = false
            };
            coinHudStyle.normal.textColor = new Color(1f, 0.88f, 0.2f);

            hpTextStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false
            };
            hpTextStyle.normal.textColor = Color.white;

            gameOverHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false
            };
            gameOverHeaderStyle.normal.textColor = new Color(1f, 0.25f, 0.25f);

            gameOverStatsStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 19,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false
            };
            gameOverStatsStyle.normal.textColor = Color.white;

            restartButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 19,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false
            };
            restartButtonStyle.normal.textColor = Color.yellow;

            // Menu Styles
            menuHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 32,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false
            };
            menuHeaderStyle.normal.textColor = new Color(1f, 0.85f, 0.25f);

            menuAuthorStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false
            };
            menuAuthorStyle.normal.textColor = new Color(0.2f, 0.95f, 1f);

            menuGuideStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false
            };
            menuGuideStyle.normal.textColor = new Color(0.75f, 0.85f, 0.95f);

            stageTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = false
            };
            stageTitleStyle.normal.textColor = new Color(1f, 0.9f, 0.35f);

            stageStatusStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = false
            };
            stageStatusStyle.normal.textColor = new Color(0.4f, 1f, 0.6f);

            stageDescStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Italic,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = false
            };
            stageDescStyle.normal.textColor = new Color(0.85f, 0.9f, 0.98f);

            menuActionBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false
            };
            menuActionBtnStyle.normal.textColor = Color.yellow;

            bossTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false
            };
            bossTitleStyle.normal.textColor = Color.white;

            bossNoticeStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false
            };
            bossNoticeStyle.normal.textColor = new Color(1f, 0.35f, 0.35f);

            stageBannerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false
            };
            stageBannerStyle.normal.textColor = new Color(1f, 0.75f, 0.2f);

            stageBannerSubStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false
            };
            stageBannerSubStyle.normal.textColor = new Color(1f, 0.35f, 0.35f);
        }

        public void TriggerStageTransitionEffect(int newStage)
        {
            transitionFlashAlpha = 1.0f;
            string stageTitle = newStage switch
            {
                2 => "THÀNH TRÌ DUNG NHAM",
                3 => "VỰC BĂNG VĨNH CỬU",
                4 => "ĐỀN THỜ SẤM SÉT (MÀN CUỐI)",
                _ => "HẦM NGỤC HẮC ÁM"
            };
            stageTransitionBanner = $"🔥 BẠN ĐÃ TIẾN VÀO MÀN {newStage}: {stageTitle}! 🔥";
            stageTransitionTimer = 5.5f;
        }

        private void Update()
        {
            if (playerHealth == null)
            {
                FindPlayerHealth();
            }

            // Kiểm tra nếu máu về 0 thì kích hoạt Game Over
            if (playerHealth != null && playerHealth.IsDead && !isGameOver && GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
            {
                isGameOver = true;
                GameManager.Instance.SetState(GameState.GameOver);
            }

            // Phím tắt bàn phím
            HandleInputShortcuts();
        }

        private void HandleInputShortcuts()
        {
            if (GameManager.Instance == null) return;

            GameState state = GameManager.Instance.CurrentState;

            // 1. Phím tắt tại Màn Hình Chính (MainMenu)
            if (state == GameState.MainMenu)
            {
#if ENABLE_INPUT_SYSTEM
                if (Keyboard.current != null)
                {
                    if (Keyboard.current.digit1Key.wasPressedThisFrame)
                        GameManager.Instance.StartGame(1);
                    else if (Keyboard.current.digit2Key.wasPressedThisFrame && GameManager.Instance.HighestStageCleared >= 1)
                        GameManager.Instance.StartGame(2);
                    else if (Keyboard.current.digit3Key.wasPressedThisFrame && GameManager.Instance.HighestStageCleared >= 2)
                        GameManager.Instance.StartGame(3);
                    else if (Keyboard.current.digit4Key.wasPressedThisFrame && GameManager.Instance.HighestStageCleared >= 3)
                        GameManager.Instance.StartGame(4);
                    else if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)
                    {
                        int targetStage = Mathf.Clamp(GameManager.Instance.HighestStageCleared + 1, 1, 4);
                        GameManager.Instance.StartGame(targetStage);
                    }
                }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
                try
                {
                    if (Input.GetKeyDown(KeyCode.Alpha1))
                        GameManager.Instance.StartGame(1);
                    else if (Input.GetKeyDown(KeyCode.Alpha2) && GameManager.Instance.HighestStageCleared >= 1)
                        GameManager.Instance.StartGame(2);
                    else if (Input.GetKeyDown(KeyCode.Alpha3) && GameManager.Instance.HighestStageCleared >= 2)
                        GameManager.Instance.StartGame(3);
                    else if (Input.GetKeyDown(KeyCode.Alpha4) && GameManager.Instance.HighestStageCleared >= 3)
                        GameManager.Instance.StartGame(4);
                    else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                    {
                        int targetStage = Mathf.Clamp(GameManager.Instance.HighestStageCleared + 1, 1, 4);
                        GameManager.Instance.StartGame(targetStage);
                    }
                }
                catch {}
#endif
            }

            // 2. Phím tắt tại Màn Hình GameOver
            if (state == GameState.GameOver || isGameOver)
            {
                bool wantsRestart = false;
#if ENABLE_INPUT_SYSTEM
                if (Keyboard.current != null && (Keyboard.current.rKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame))
                    wantsRestart = true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
                try { if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Space)) wantsRestart = true; } catch {}
#endif
                if (wantsRestart) RestartGame();
            }

            // 3. Phím tắt khi Victory qua màn hoặc Phá Đảo Game
            if (state == GameState.Victory)
            {
                int curStage = GameManager.Instance != null ? GameManager.Instance.CurrentStage : 1;
                bool isFinalClear = curStage >= 4;

                bool pressSpace = false;
                bool pressR = false;
                bool pressM = false;
                bool pressEsc = false;

#if ENABLE_INPUT_SYSTEM
                if (Keyboard.current != null)
                {
                    if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame) pressSpace = true;
                    if (Keyboard.current.rKey.wasPressedThisFrame) pressR = true;
                    if (Keyboard.current.mKey.wasPressedThisFrame) pressM = true;
                    if (Keyboard.current.escapeKey.wasPressedThisFrame) pressEsc = true;
                }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
                try
                {
                    if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) pressSpace = true;
                    if (Input.GetKeyDown(KeyCode.R)) pressR = true;
                    if (Input.GetKeyDown(KeyCode.M)) pressM = true;
                    if (Input.GetKeyDown(KeyCode.Escape)) pressEsc = true;
                }
                catch {}
#endif

                if (isFinalClear)
                {
                    if (pressR || pressSpace)
                    {
                        RestartGame();
                    }
                    else if (pressM || pressEsc)
                    {
                        GameManager.Instance.SetState(GameState.MainMenu);
                    }
                }
                else
                {
                    if (pressSpace)
                    {
                        GameManager.Instance.AdvanceToStage(curStage + 1);
                    }
                    else if (pressM || pressEsc)
                    {
                        GameManager.Instance.SetState(GameState.MainMenu);
                    }
                }
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state == GameState.GameOver)
            {
                isGameOver = true;
            }
            else if (state == GameState.Playing)
            {
                isGameOver = false;
            }
        }

        private void OnGUI()
        {
            InitStyles();

            if (GameManager.Instance == null) return;

            GameState state = GameManager.Instance.CurrentState;

            // ================= 1. MÀN HÌNH CHỌN MÀN CHƠI TRƯỚC KHI VÀO GAME (MAIN MENU) =================
            if (state == GameState.MainMenu)
            {
                DrawMainMenuScreen();
                return;
            }

            // ================= 2. HUD CHIẾN ĐẤU THỜI GIAN THỰC =================
            DrawInGameHUD();

            // ================= 3. THANH MÁU BOSS CUỐI (KHI BOSS ĐANG XUẤT HIỆN) =================
            if (GameManager.Instance.IsBossActive)
            {
                DrawBossHealthBar();
            }

            // ================= 4. BANNER THÔNG BÁO CỔNG DỊCH CHUYỂN KHI DIỆT XONG BOSS =================
            if (state == GameState.StageClear)
            {
                DrawStageClearBanner();
            }

            // ================= 5. BẢNG CHIẾN THẮNG VƯỢT MÀN KHI BƯỚC VÀO CỔNG =================
            if (state == GameState.Victory)
            {
                DrawVictoryModal();
            }

            // ================= 6. BẢNG GAME OVER KHI HY SINH =================
            if (state == GameState.GameOver || isGameOver)
            {
                DrawGameOverModal();
            }

            // ================= 7. HIỆU ỨNG CHUYỂN MÀN & BANNER TIẾN VÀO MÀN 2 =================
            DrawStageTransitionEffect();
        }

        // ================= VẼ MÀN HÌNH CHÍNH & CHỌN MÀN TOÀN DIỆN (FULL-SCREEN TITLE SCENE) =================
        private void DrawMainMenuScreen()
        {
            float sw = Screen.width;
            float sh = Screen.height;

            // 1. Phủ kín 100% toàn bộ màn hình (Che hoàn toàn sàn hầm ngục và quái vật phía sau)
            GUI.DrawTexture(new Rect(0, 0, sw, sh), fullScreenMenuBgTex);

            // 2. Thanh Tiêu Đề (Header Bar) trên đỉnh màn hình
            float headerH = 88f;
            GUI.DrawTexture(new Rect(0, 0, sw, headerH), menuHeaderBgTex);
            GUI.DrawTexture(new Rect(0, headerH - 3, sw, 3), goldLineTex); // Đường viền chỉ vàng kim óng ánh

            GUI.Label(new Rect(0, 24f, sw, 40f), "⚔️ CHIẾN BINH RPG: HẦM NGỤC HUYỀN THOẠI ⚔️", menuHeaderStyle);

            // 3. Khung Lưới Chọn Màn Chơi 2 Hàng x 2 Cột (2x2 Grid Layout) - Phủ đều màn hình
            float gridMarginX = 36f;
            float gridW = Mathf.Clamp(sw - (gridMarginX * 2f), 740f, 1220f);
            float gridX = (sw - gridW) / 2f;
            int cleared = GameManager.Instance != null ? GameManager.Instance.HighestStageCleared : 0;

            float bottomH = 92f;
            float availableH = sh - headerH - bottomH - 32f;
            float rowSpacing = 14f;
            float colSpacing = 16f;
            float cardH = Mathf.Clamp((availableH - rowSpacing) / 2f, 140f, 195f);
            float cardW = (gridW - colSpacing) / 2f;

            float gridStartY = headerH + 16f;

            // Tọa độ 2 Cột x 2 Hàng
            float col0X = gridX;
            float col1X = gridX + cardW + colSpacing;
            float row0Y = gridStartY;
            float row1Y = gridStartY + cardH + rowSpacing;

            // ===== MÀN 1 (Hàng 0, Cột 0) =====
            DrawGridStageCard(
                new Rect(col0X, row0Y, cardW, cardH),
                stageBorderTex1,
                "🏰 MÀN 1: HẦM NGỤC HẮC ÁM",
                cleared >= 1 ? "🏆 TRẠNG THÁI: ĐÃ VƯỢT QUA ★★★" : "⚡ SẴN SÀNG CHIẾN ĐẤU",
                new Color(0.3f, 0.8f, 1f),
                "• Mục tiêu: 1,000 Điểm ➔ Chúa Tể Goliath xuất hiện\n• Đặc điểm Trùm: Đòn đánh trừ đúng 10 Máu người chơi\n• Phần thưởng: Diệt Boss triệu hồi Cổng Dịch Chuyển qua Màn 2",
                "▶ CHIẾN ĐẤU MÀN 1 (Phím 1)",
                true,
                () => GameManager.Instance.StartGame(1)
            );

            // ===== MÀN 2 (Hàng 0, Cột 1) =====
            bool isStage2Unlocked = cleared >= 1;
            DrawGridStageCard(
                new Rect(col1X, row0Y, cardW, cardH),
                stageBorderTex2,
                "🌋 MÀN 2: THÀNH TRÌ DUNG NHAM",
                isStage2Unlocked ? (cleared >= 2 ? "🏆 TRẠNG THÁI: ĐÃ VƯỢT QUA ★★★" : "⚡ SẴN SÀNG CHIẾN ĐẤU") : "🔒 ĐANG KHÓA (Cần qua Màn 1)",
                new Color(1f, 0.45f, 0.2f),
                "• Mục tiêu: 2,500 Điểm ➔ Chúa Tể Dung Nham Ifrit (2,500 HP)\n• Kỹ năng Trùm: Bắn Cầu Lửa từ xa & Cuồng Nộ đạn chùm 3 hướng\n• Địa hình: Sàn nham thạch vô tận rực lửa và quỷ dung nham",
                isStage2Unlocked ? "▶ CHIẾN ĐẤU MÀN 2 (Phím 2)" : "🔒 CẦN TIÊU DIỆT BOSS MÀN 1",
                isStage2Unlocked,
                () => { if (isStage2Unlocked) GameManager.Instance.StartGame(2); }
            );

            // ===== MÀN 3 (Hàng 1, Cột 0) =====
            bool isStage3Unlocked = cleared >= 2;
            DrawGridStageCard(
                new Rect(col0X, row1Y, cardW, cardH),
                stageBorderTex3,
                "❄️ MÀN 3: VỰC BĂNG VĨNH CỬU",
                isStage3Unlocked ? (cleared >= 3 ? "🏆 TRẠNG THÁI: ĐÃ VƯỢT QUA ★★★" : "⚡ SẴN SÀNG CHIẾN ĐẤU") : "🔒 ĐANG KHÓA (Cần qua Màn 2)",
                new Color(0.4f, 0.95f, 1f),
                "• Mục tiêu: 4,000 Điểm ➔ Vu Yêu Băng Frost Lich (4,000 HP)\n• Kỹ năng Trùm: Phép Băng Giá diện rộng & Băng tiễn dồn dập\n• Địa hình: Hầm ngục băng giá cổ đại trơn trượt",
                isStage3Unlocked ? "▶ CHIẾN ĐẤU MÀN 3 (Phím 3)" : "🔒 CẦN TIÊU DIỆT BOSS MÀN 2",
                isStage3Unlocked,
                () => { if (isStage3Unlocked) GameManager.Instance.StartGame(3); }
            );

            // ===== MÀN 4 (Hàng 1, Cột 1) =====
            bool isStage4Unlocked = cleared >= 3;
            DrawGridStageCard(
                new Rect(col1X, row1Y, cardW, cardH),
                stageBorderTex4,
                "⚡ MÀN 4: ĐỀN THỜ SẤM SÉT (MÀN CUỐI)",
                isStage4Unlocked ? (cleared >= 4 ? "👑 ĐÃ TOÀN THẮNG TRÒ CHƠI ★★★" : "⚡ SẴN SÀNG CHIẾN ĐẤU") : "🔒 ĐANG KHÓA (Cần qua Màn 3)",
                new Color(1f, 0.85f, 0.25f),
                "• Mục tiêu: 6,000 Điểm ➔ Thần Sấm Sét Zeus Titan (6,000 HP)\n• Kỹ năng Trùm: Lôi điện hủy diệt toàn màn hình\n• Vinh quang: Toàn thắng RPG - Phá đảo Hầm Ngục Huyền Thoại!",
                isStage4Unlocked ? "▶ QUYẾT ĐẤU MÀN 4 (Phím 4)" : "🔒 CẦN TIÊU DIỆT BOSS MÀN 3",
                isStage4Unlocked,
                () => { if (isStage4Unlocked) GameManager.Instance.StartGame(4); }
            );

            // 4. Thanh Đáy & Nút Bắt Đầu Lớn (Footer Action Bar)
            int targetStage = Mathf.Clamp(cleared + 1, 1, 4);
            float playBtnW = Mathf.Clamp(gridW * 0.65f, 480f, 680f);
            float playBtnH = 50f;
            float playBtnX = (sw - playBtnW) / 2f;
            float playBtnY = row1Y + cardH + 16f;

            if (playBtnY + playBtnH + 32f > sh)
            {
                playBtnY = sh - playBtnH - 32f;
            }

            string playBtnText = $"⚔️ BẮT ĐẦU CHIẾN ĐẤU MÀN {targetStage} (SPACE / ENTER) ⚔️";
            if (GUI.Button(new Rect(playBtnX, playBtnY, playBtnW, playBtnH), playBtnText, restartButtonStyle))
            {
                GameManager.Instance.StartGame(targetStage);
            }

            // Hướng dẫn phím tắt ở sát mép đáy
            float guideY = playBtnY + playBtnH + 6f;
            GUI.Label(new Rect(0, guideY, sw, 22f), "🎮 Phím tắt: [1, 2, 3, 4] Chọn nhanh màn chơi  •  [SPACE / ENTER] Bắt đầu màn cao nhất  •  [W, A, S, D] Di chuyển  •  [Chuột Trái] Tấn công  •  [R] Chơi lại", menuGuideStyle);
        }

        private void DrawGridStageCard(Rect rect, Texture2D borderTex, string title, string status, Color titleColor, string desc, string btnText, bool isEnabled, Action onClick)
        {
            // Viền phát sáng theo màu chủ đề của từng màn chơi
            Texture2D bTex = borderTex != null ? borderTex : (stageCardBorderTex != null ? stageCardBorderTex : Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.x - 2, rect.y - 2, rect.width + 4, rect.height + 4), bTex);

            // Nền thẻ
            Texture2D bgTex = (isEnabled && stageCardBgTex != null) ? stageCardBgTex : (popupBgTex != null ? popupBgTex : Texture2D.blackTexture);
            GUI.DrawTexture(rect, bgTex);

            float padX = 16f;
            float contentW = rect.width - (padX * 2f);

            // 1. Tiêu đề Màn chơi
            GUIStyle curTitleStyle = new GUIStyle(stageTitleStyle);
            curTitleStyle.fontSize = Mathf.Clamp(Mathf.RoundToInt(rect.height * 0.11f), 15, 18);
            curTitleStyle.normal.textColor = titleColor;
            GUI.Label(new Rect(rect.x + padX, rect.y + 10f, contentW, 24f), title, curTitleStyle);

            // 2. Trạng thái (Đã vượt qua / Sẵn sàng / Đang khóa)
            GUIStyle curStatusStyle = new GUIStyle(stageStatusStyle);
            curStatusStyle.fontSize = Mathf.Clamp(Mathf.RoundToInt(rect.height * 0.085f), 12, 14);
            curStatusStyle.normal.textColor = isEnabled ? (status.Contains("ĐÃ VƯỢT") || status.Contains("TOÀN THẮNG") ? new Color(1f, 0.85f, 0.2f) : new Color(0.4f, 1f, 0.5f)) : new Color(0.9f, 0.4f, 0.4f);
            GUI.Label(new Rect(rect.x + padX, rect.y + 35f, contentW, 20f), status, curStatusStyle);

            // 3. Mô tả chi tiết
            GUIStyle curDescStyle = new GUIStyle(stageDescStyle);
            curDescStyle.fontSize = Mathf.Clamp(Mathf.RoundToInt(rect.height * 0.075f), 11, 13);
            curDescStyle.wordWrap = true;
            float descH = rect.height - 110f;
            GUI.Label(new Rect(rect.x + padX, rect.y + 57f, contentW, Mathf.Max(descH, 40f)), desc, curDescStyle);

            // 4. Nút hành động trực tiếp ở đáy thẻ
            float btnH = Mathf.Clamp(rect.height * 0.22f, 34f, 44f);
            float btnY = rect.y + rect.height - btnH - 10f;
            Rect btnRect = new Rect(rect.x + padX, btnY, contentW, btnH);

            GUI.enabled = isEnabled;
            if (GUI.Button(btnRect, btnText, menuActionBtnStyle))
            {
                onClick?.Invoke();
            }
            GUI.enabled = true;
        }

        // ================= VẼ HUD CHIẾN ĐẤU =================
        private void DrawInGameHUD()
        {
            // 1. Thanh máu Player
            float curHp = playerHealth != null ? Mathf.Max(0, playerHealth.CurrentHealth) : 100f;
            float maxHp = playerHealth != null ? playerHealth.MaxHealth : 100f;
            float hpPct = Mathf.Clamp01(curHp / maxHp);

            float bx = 25f;
            float by = 25f;
            float bw = 260f;
            float bh = 32f;
            float border = 3f;

            GUI.DrawTexture(new Rect(bx - border, by - border, bw + border * 2, bh + border * 2), borderTex);
            GUI.DrawTexture(new Rect(bx, by, bw, bh), hpBgTex);
            if (hpPct > 0f)
            {
                GUI.DrawTexture(new Rect(bx, by, bw * hpPct, bh), hpFillTex);
            }
            GUI.Label(new Rect(bx, by, bw, bh), $"MÁU: {Mathf.CeilToInt(curHp)} / {Mathf.CeilToInt(maxHp)} ({Mathf.RoundToInt(hpPct * 100)}%)", hpTextStyle);

            // 1.5. Hiển thị Tiền Vàng (Coins)
            int coins = GameManager.Instance.Coins;
            GUI.Label(new Rect(bx, by + bh + 4f, 260f, 24f), $"★ VÀNG: {coins:N0} COIN", coinHudStyle);

            // 2. Điểm & Tiêu diệt
            int score = GameManager.Instance.Score;
            int kills = GameManager.Instance.KillCount;
            float time = GameManager.Instance.SurvivalTime;
            int stage = GameManager.Instance.CurrentStage;

            float rightEdge = Screen.width - 25f;
            string stageTitleText = stage == 2 ? "🌋 MÀN 2: DUNG NHAM" : (stage == 3 ? "❄️ MÀN 3: BĂNG GIÁ" : (stage == 4 ? "⚡ MÀN 4: SẤM SÉT" : "🏰 MÀN 1: HẦM NGỤC"));
            GUI.Label(new Rect(rightEdge - 420f, 15f, 420f, 32f), $"{stageTitleText}  |  ★ {score:N0} Điểm", scoreStyle);
            GUI.Label(new Rect(rightEdge - 420f, 48f, 420f, 28f), $"☠ TIÊU DIỆT: {kills} con", killsStyle);

            if (!GameManager.Instance.IsBossActive && !GameManager.Instance.IsBossDefeated)
            {
                GUI.Label(new Rect(rightEdge - 420f, 78f, 420f, 24f), $"👑 MỐC TRIỆU HỒI BOSS: {GameManager.Instance.BossScoreThreshold:N0} ĐIỂM", bossNoticeStyle);
            }

            if (stage >= 2)
            {
                string bannerSub = stage switch
                {
                    2 => "🔥 [MÀN 2: THÀNH TRÌ DUNG NHAM]",
                    3 => "❄️ [MÀN 3: VỰC BĂNG VĨNH CỬU]",
                    4 => "⚡ [MÀN 4: ĐỀN THỜ SẤM SÉT]",
                    _ => ""
                };
                GUI.Label(new Rect(bx, by + bh + 28f, 380f, 22f), bannerSub, stageBannerSubStyle);
            }

            // 3. Đồng hồ thời gian
            int min = Mathf.FloorToInt(time / 60f);
            int sec = Mathf.FloorToInt(time % 60f);
            GUI.Label(new Rect((Screen.width / 2f) - 150f, 18f, 300f, 32f), $"⏱ THỜI GIAN: {min:00}:{sec:00}", titleStyle);
        }

        // ================= VẼ THANH MÁU BOSS CUỐI =================
        private void DrawBossHealthBar()
        {
            var boss = GameManager.Instance.ActiveBoss;
            if (boss == null) return;

            float curHp = Mathf.Max(0, boss.CurrentHealth);
            float maxHp = boss.MaxHealth;
            float pct = Mathf.Clamp01(curHp / maxHp);

            float barW = Mathf.Min(Screen.width - 100f, 580f);
            float barH = 34f;
            float barX = (Screen.width - barW) / 2f;
            float barY = 58f;

            // Khung viền tím neon / đỏ cam
            GUI.DrawTexture(new Rect(barX - 3, barY - 3, barW + 6, barH + 6), bossBorderTex);
            GUI.DrawTexture(new Rect(barX, barY, barW, barH), hpBgTex);

            // Ruột máu đỏ thẫm
            if (pct > 0f)
            {
                GUI.DrawTexture(new Rect(barX, barY, barW * pct, barH), bossFillTex);
            }

            // Tiêu đề Boss & Cảnh báo chiêu thức
            GUI.Label(new Rect(barX, barY + 4f, barW, 26f), $"👑 {boss.BossName} : {Mathf.CeilToInt(curHp)} / {Mathf.CeilToInt(maxHp)}", bossTitleStyle);

            string warningMsg = (GameManager.Instance.CurrentStage == 2)
                ? "🔥 CẢNH BÁO: BOSS BẮN CẦU LỬA DUNG NHAM TỪ XA & SÁT THƯƠNG 14 MÁU!"
                : "⚠️ CẢNH BÁO: ĐÒN ĐÁNH CỦA BOSS TRỪ ĐÚNG 10 MÁU NGƯỜI CHƠI!";
            GUI.Label(new Rect(barX, barY + 36f, barW, 20f), warningMsg, bossNoticeStyle);
        }

        // ================= VẼ BANNER CỔNG DỊCH CHUYỂN =================
        private void DrawStageClearBanner()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentStage >= 4) return;

            int curStage = GameManager.Instance != null ? GameManager.Instance.CurrentStage : 1;
            string bossName = curStage switch
            {
                1 => "GOLIATH",
                2 => "IFRIT",
                3 => "FROST LICH",
                _ => $"MÀN {curStage}"
            };

            float bw = Mathf.Min(Screen.width - 40f, 780f);
            float bh = 64f;
            float bx = (Screen.width - bw) / 2f;
            float by = 60f;

            GUI.DrawTexture(new Rect(bx - 3, by - 3, bw + 6, bh + 6), borderTex);
            GUI.DrawTexture(new Rect(bx, by, bw, bh), popupBgTex);

            GUI.Label(new Rect(bx, by + 6f, bw, 26f), $"🎉 BOSS {bossName} ĐÃ BỊ HẠ GỤC! QUÁI VẬT ĐÃ NGỪNG XUẤT HIỆN! 🎉", menuHeaderStyle);
            GUI.Label(new Rect(bx, by + 34f, bw, 22f), $"🌀 HÃY BƯỚC VÀO CỔNG DỊCH CHUYỂN PHÁT SÁNG ĐỂ TIẾN VÀO MÀN {curStage + 1}! 🌀", stageStatusStyle);
        }

        // ================= VẼ BẢNG CHIẾN THẮNG VƯỢT MÀN / PHÁ ĐẢO GAME =================
        private void DrawVictoryModal()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentStage >= 4)
            {
                DrawGameCompleteVictoryModal();
            }
            else
            {
                DrawStageVictoryModal();
            }
        }

        // ================= VẼ BẢNG CHÚC MỪNG PHÁ ĐẢO GAME (MÀN 4 HOÀN THÀNH) =================
        private void DrawGameCompleteVictoryModal()
        {
            float vw = Mathf.Min(Screen.width - 40f, 860f);
            float vh = Mathf.Min(Screen.height - 30f, 630f);
            float vx = (Screen.width - vw) / 2f;
            float vy = (Screen.height - vh) / 2f;

            // Viền hoàng kim lấp lánh và nền xanh thẫm
            GUI.DrawTexture(new Rect(vx - 5, vy - 5, vw + 10, vh + 10), borderTex);
            GUI.DrawTexture(new Rect(vx, vy, vw, vh), popupBgTex);

            // 1. Tiêu đề Phá Đảo Game rực rỡ
            GUI.Label(new Rect(vx, vy + 22f, vw, 40f), "👑 CHÚC MỪNG BẠN ĐÃ PHÁ ĐẢO TRÒ CHƠI! 👑", menuHeaderStyle);
            GUI.Label(new Rect(vx, vy + 68f, vw, 26f), "✨ ĐẠI TOÀN THẮNG: ĐẬP TAN THẦN SẤM ZEUS TITAN & GIẢI CỨU THẾ GIỚI RPG! ✨", stageTitleStyle);

            // 2. Bảng thống kê toàn bộ hành trình
            int score = GameManager.Instance.Score;
            int kills = GameManager.Instance.KillCount;
            int coins = GameManager.Instance.Coins;
            float time = GameManager.Instance.SurvivalTime;
            int min = Mathf.FloorToInt(time / 60f);
            int sec = Mathf.FloorToInt(time % 60f);

            var player = UnityEngine.Object.FindAnyObjectByType<Player.PlayerCombat>();
            int multiLvl = player != null ? player.MultiShotLevel : 0;
            int dmgLvl = player != null ? player.DamageLevel : 0;
            int spdLvl = player != null ? player.FireRateLevel : 0;

            float statsY = vy + 120f;
            float rowH = 34f;

            GUI.Label(new Rect(vx + 40f, statsY, vw - 80f, rowH), $"★ TỔNG ĐIỂM CHIẾN DỊCH HOÀN THÀNH: {score:N0} ĐIỂM", gameOverStatsStyle);
            GUI.Label(new Rect(vx + 40f, statsY + rowH, vw - 80f, rowH), $"☠ TỔNG QUÁI VẬT & 4 TRÙM CUỐI TIÊU DIỆT: {kills} KẺ ĐỊCH", gameOverStatsStyle);
            GUI.Label(new Rect(vx + 40f, statsY + rowH * 2, vw - 80f, rowH), $"★ TỔNG TIỀN VÀNG TÍCH LŨY: {coins:N0} COIN", gameOverStatsStyle);
            GUI.Label(new Rect(vx + 40f, statsY + rowH * 3, vw - 80f, rowH), $"⏱ TỔNG THỜI GIAN VƯỢT QUA 4 MÀN: {min:00} PHÚT {sec:00} GIÂY", gameOverStatsStyle);
            GUI.Label(new Rect(vx + 40f, statsY + rowH * 4, vw - 80f, rowH), $"⚔ CẤP VŨ KHÍ: Đa Tia [Cấp {multiLvl}/2]  |  Sát Thương [Cấp {dmgLvl}/2]  |  Tốc Độ [Cấp {spdLvl}/2]", stageStatusStyle);
            GUI.Label(new Rect(vx + 40f, statsY + rowH * 5, vw - 80f, 38f), "🎖 DANH HIỆU: ★★★ HUYỀN THOẠI CHIẾN BINH RPG BẤT TỬ ★★★", stageBannerStyle);

            // 3. Hai nút tương tác lớn
            float btnW = vw - 120f;
            float btnH = 58f;
            float btnX = vx + 60f;
            float btnY1 = vy + vh - 140f;
            float btnY2 = vy + vh - 72f;

            if (GUI.Button(new Rect(btnX, btnY1, btnW, btnH), "🔄 CHƠI LẠI TỪ ĐẦU (PHÍM 'R' / SPACE)", restartButtonStyle))
            {
                RestartGame();
            }

            if (GUI.Button(new Rect(btnX, btnY2, btnW, 50f), "🏠 VỀ MENU CHỌN MÀN CHƠI (PHÍM 'M' / ESC)", menuActionBtnStyle))
            {
                GameManager.Instance.SetState(GameState.MainMenu);
            }
        }

        // ================= VẼ BẢNG CHIẾN THẮNG TỪNG MÀN 1, 2, 3 =================
        private void DrawStageVictoryModal()
        {
            int curStage = GameManager.Instance != null ? GameManager.Instance.CurrentStage : 1;
            int nextStage = curStage + 1;

            string curTitle = curStage switch
            {
                1 => "MÀN 1: HẦM NGỤC HẮC ÁM",
                2 => "MÀN 2: THÀNH TRÌ DUNG NHAM",
                3 => "MÀN 3: VỰC BĂNG VĨNH CỬU",
                _ => $"MÀN {curStage}"
            };

            string nextTitle = nextStage switch
            {
                2 => "MÀN 2: THÀNH TRÌ DUNG NHAM",
                3 => "MÀN 3: VỰC BĂNG VĨNH CỬU",
                4 => "MÀN 4: ĐỀN THỜ SẤM SÉT (MÀN CUỐI)",
                _ => $"MÀN {nextStage}"
            };

            float vw = Mathf.Min(Screen.width - 40f, 720f);
            float vh = 440f;
            float vx = (Screen.width - vw) / 2f;
            float vy = (Screen.height - vh) / 2f;

            GUI.DrawTexture(new Rect(vx - 4, vy - 4, vw + 8, vh + 8), borderTex);
            GUI.DrawTexture(new Rect(vx, vy, vw, vh), popupBgTex);

            GUI.Label(new Rect(vx, vy + 20f, vw, 36f), $"🏆 CHIẾN THẮNG {curTitle} RỰC RỠ! 🏆", menuHeaderStyle);
            GUI.Label(new Rect(vx, vy + 60f, vw, 26f), $"Bạn đã đánh bại Trùm Cuối & Mở Khóa {nextTitle}!", menuAuthorStyle);

            GUI.Label(new Rect(vx, vy + 105f, vw, 28f), $"★ Tổng Điểm Đạt Được: {GameManager.Instance.Score:N0} Điểm", gameOverStatsStyle);
            GUI.Label(new Rect(vx, vy + 138f, vw, 28f), $"☠ Số Quái Vật Hạ Gục: {GameManager.Instance.KillCount} Con", gameOverStatsStyle);
            GUI.Label(new Rect(vx, vy + 170f, vw, 28f), $"★ Tiền Vàng Tích Lũy: {GameManager.Instance.Coins:N0} COIN", gameOverStatsStyle);
            GUI.Label(new Rect(vx, vy + 202f, vw, 28f), $"🔥 Đã Mở Khóa: {nextTitle}", stageTitleStyle);

            // Nút Tiến Vào Màn Kế Tiếp
            if (GUI.Button(new Rect(vx + 60f, vy + 250f, vw - 120f, 62f), $"▶ TIẾN VÀO {nextTitle} (SPACE / '{nextStage}')", restartButtonStyle))
            {
                GameManager.Instance.AdvanceToStage(nextStage);
            }

            // Nút Về Menu Chính
            if (GUI.Button(new Rect(vx + 60f, vy + 325f, vw - 120f, 50f), "🏠 VỀ MENU CHỌN MÀN CHƠI (Phím 'M' / ESC)", menuActionBtnStyle))
            {
                GameManager.Instance.SetState(GameState.MainMenu);
            }
        }

        // ================= VẼ BẢNG GAME OVER =================
        private void DrawGameOverModal()
        {
            float pw = 540f;
            float ph = 340f;
            float px = (Screen.width - pw) / 2f;
            float py = (Screen.height - ph) / 2f;

            GUI.DrawTexture(new Rect(px - 4, py - 4, pw + 8, ph + 8), borderTex);
            GUI.DrawTexture(new Rect(px, py, pw, ph), popupBgTex);

            GUI.Label(new Rect(px, py + 20f, pw, 40f), "⚔️ HIỆP SĨ ĐÃ HY SINH! ⚔️", gameOverHeaderStyle);

            int score = GameManager.Instance.Score;
            int kills = GameManager.Instance.KillCount;
            float time = GameManager.Instance.SurvivalTime;
            int min = Mathf.FloorToInt(time / 60f);
            int sec = Mathf.FloorToInt(time % 60f);

            GUI.Label(new Rect(px, py + 80f, pw, 30f), $"★ Tổng Điểm Đạt Được: {score:N0}", gameOverStatsStyle);
            GUI.Label(new Rect(px, py + 115f, pw, 30f), $"☠ Số Quái Tiêu Diệt: {kills} con", gameOverStatsStyle);
            GUI.Label(new Rect(px, py + 150f, pw, 30f), $"⏱ Thời Gian Sống Sót: {min:00} phút {sec:00} giây", gameOverStatsStyle);

            if (GUI.Button(new Rect(px + 60f, py + 225f, pw - 120f, 60f), "🔄 CHƠI LẠI (BẤM PHÍM 'R')", restartButtonStyle))
            {
                RestartGame();
            }
        }

        // ================= VẼ HIỆU ỨNG CHUYỂN MÀN & BANNER MÀN 2 =================
        private void DrawStageTransitionEffect()
        {
            // 1. Banner chào đón màn chơi mới khi vừa bước qua cổng
            if (stageTransitionTimer > 0f)
            {
                stageTransitionTimer -= Time.unscaledDeltaTime;

                float bw = Mathf.Min(Screen.width - 40f, 860f);
                float bh = 95f;
                float bx = (Screen.width - bw) / 2f;
                float by = 65f;

                // Nền dung nham với viền vàng cam rực cháy
                GUI.DrawTexture(new Rect(bx - 3, by - 3, bw + 6, bh + 6), borderTex);
                GUI.DrawTexture(new Rect(bx, by, bw, bh), popupBgTex);

                GUI.Label(new Rect(bx, by + 12f, bw, 34f), stageTransitionBanner, stageBannerStyle);
                GUI.Label(new Rect(bx, by + 50f, bw, 26f), "⚡ Sàn hầm ngục đã biến thành nham thạch nóng chảy! Quái vật trở nên cuồng bạo hơn!", stageBannerSubStyle);
            }

            // 2. Chớp sáng chuyển màn (Flash Transition) khi bước vào cổng dịch chuyển
            if (transitionFlashAlpha > 0.01f)
            {
                transitionFlashAlpha = Mathf.MoveTowards(transitionFlashAlpha, 0f, Time.unscaledDeltaTime * 1.4f);
                Color oldColor = GUI.color;
                GUI.color = new Color(1f, 0.95f, 0.85f, transitionFlashAlpha);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), flashTex);
                GUI.color = oldColor;
            }
        }

        public void RestartGame()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartCurrentScene();
            }
        }
    }
}
