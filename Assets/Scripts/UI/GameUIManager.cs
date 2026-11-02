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
        private Texture2D flashTex;

        // Transition Effect Variables
        private float transitionFlashAlpha = 0f;
        private string stageTransitionBanner = "";
        private float stageTransitionTimer = 0f;
        private GUIStyle stageBannerStyle;
        private GUIStyle stageBannerSubStyle;

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
            stageCardBgTex = MakeTex(new Color(0.12f, 0.12f, 0.18f, 0.95f));
            stageCardBorderTex = MakeTex(new Color(0.4f, 0.5f, 0.7f, 0.8f));
            flashTex = MakeTex(new Color(1f, 1f, 1f, 1f));
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
                fontSize = 21,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false
            };
            restartButtonStyle.normal.textColor = Color.yellow;

            // Menu Styles
            menuHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false
            };
            menuHeaderStyle.normal.textColor = new Color(1f, 0.85f, 0.25f);

            menuAuthorStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false
            };
            menuAuthorStyle.normal.textColor = new Color(0.2f, 0.95f, 1f);

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
                fontSize = 17,
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
            stageTransitionBanner = $"🔥 BẠN ĐÃ TIẾN VÀO MÀN {newStage}: THÀNH TRÌ DUNG NHAM! 🔥";
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
                    if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.digit1Key.wasPressedThisFrame)
                        GameManager.Instance.StartGame(1);
                    else if (Keyboard.current.digit2Key.wasPressedThisFrame && GameManager.Instance.HighestStageCleared >= 1)
                        GameManager.Instance.StartGame(2);
                }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
                try
                {
                    if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Alpha1))
                        GameManager.Instance.StartGame(1);
                    else if (Input.GetKeyDown(KeyCode.Alpha2) && GameManager.Instance.HighestStageCleared >= 1)
                        GameManager.Instance.StartGame(2);
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

            // 3. Phím tắt khi Victory qua màn
            if (state == GameState.Victory)
            {
#if ENABLE_INPUT_SYSTEM
                if (Keyboard.current != null)
                {
                    if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.digit2Key.wasPressedThisFrame)
                        GameManager.Instance.AdvanceToStage(2);
                    else if (Keyboard.current.mKey.wasPressedThisFrame)
                        GameManager.Instance.SetState(GameState.MainMenu);
                }
#endif
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

        // ================= VẼ MÀN HÌNH CHÍNH & CHỌN MÀN =================
        private void DrawMainMenuScreen()
        {
            float mw = Mathf.Min(Screen.width - 40f, 960f);
            float mh = Mathf.Min(Screen.height - 40f, 620f);
            float mx = (Screen.width - mw) / 2f;
            float my = (Screen.height - mh) / 2f;

            // Nền tối và viền vàng kim
            GUI.DrawTexture(new Rect(mx - 4, my - 4, mw + 8, mh + 8), borderTex);
            GUI.DrawTexture(new Rect(mx, my, mw, mh), popupBgTex);

            // Tiêu đề & Thông tin sinh viên
            GUI.Label(new Rect(mx, my + 18f, mw, 36f), "⚔️ CHIẾN BINH RPG: HẦM NGỤC HUYỀN THOẠI ⚔️", menuHeaderStyle);
            GUI.Label(new Rect(mx, my + 54f, mw, 24f), "Đồ Án PRU213 - Sinh viên: Châu Vương Hoàng (MSSV: DE180551)", menuAuthorStyle);

            int cleared = GameManager.Instance.HighestStageCleared;

            // Khung Màn 1
            float cardX = mx + 25f;
            float cardW = mw - 50f;
            float cardH = 150f;
            float card1Y = my + 95f;

            DrawStageCard(
                new Rect(cardX, card1Y, cardW, cardH),
                "🏰 MÀN 1: HẦM NGỤC HẮC ÁM (Dark Dungeon)",
                cleared >= 1 ? "🏆 TRẠNG THÁI: ĐÃ VƯỢT QUA ★★★" : "⚡ TRẠNG THÁI: ĐÃ MỞ KHÓA - SẴN SÀNG CHIẾN ĐẤU",
                "• Mục tiêu: Tiêu diệt quái vật ➔ Đạt 1,000 Điểm ➔ Chúa Tể Goliath xuất hiện.\n• Cảnh báo Trùm: Mỗi đòn đánh của Boss Goliath trừ đúng 10 Máu!\n• Phần thưởng: Tiêu diệt Boss để ngưng quái và triệu hồi Cổng Dịch Chuyển qua Màn 2.",
                "▶️ CHIẾN ĐẤU MÀN 1 (Phím 1)",
                true,
                () => GameManager.Instance.StartGame(1)
            );

            // Khung Màn 2
            float card2Y = my + 260f;
            bool isStage2Unlocked = cleared >= 1;

            DrawStageCard(
                new Rect(cardX, card2Y, cardW, cardH),
                "🌋 MÀN 2: THÀNH TRÌ DUNG NHAM (Molten Citadel)",
                isStage2Unlocked ? (cleared >= 2 ? "🏆 TRẠNG THÁI: ĐÃ VƯỢT QUA ★★★" : "⚡ TRẠNG THÁI: ĐÃ MỞ KHÓA - SẴN SÀNG CHIẾN ĐẤU") : "🔒 TRẠNG THÁI: ĐANG KHÓA (Cần tiêu diệt Boss Goliath ở Màn 1)",
                "• Môi trường: Bản đồ nham thạch rực lửa vô tận với quái vật dung nham hung hãn.\n• Mục tiêu: Đạt 2,500 Điểm ➔ Tiêu Diệt Chúa Tể Lửa Ifrit.\n• Giữ nguyên toàn bộ vũ khí thần thoại đã nâng cấp từ Màn 1!",
                isStage2Unlocked ? "▶️ CHIẾN ĐẤU MÀN 2 (Phím 2)" : "🔒 CHƯA MỞ KHÓA",
                isStage2Unlocked,
                () => { if (isStage2Unlocked) GameManager.Instance.StartGame(2); }
            );

            // Nút To Ở Đáy: Bắt Đầu Chơi
            float playBtnW = 420f;
            float playBtnH = 65f;
            float playBtnX = mx + (mw - playBtnW) / 2f;
            float playBtnY = my + mh - playBtnH - 25f;

            if (GUI.Button(new Rect(playBtnX, playBtnY, playBtnW, playBtnH), "⚔️ BẮT ĐẦU CHIẾN ĐẤU (SPACE / ENTER) ⚔️", restartButtonStyle))
            {
                GameManager.Instance.StartGame(1);
            }
        }

        private void DrawStageCard(Rect rect, string title, string status, string desc, string btnText, bool isEnabled, Action onClick)
        {
            GUI.DrawTexture(new Rect(rect.x - 1, rect.y - 1, rect.width + 2, rect.height + 2), stageCardBorderTex);
            GUI.DrawTexture(rect, stageCardBgTex);

            float infoX = rect.x + 20f;
            float btnW = 230f;
            float infoW = rect.width - btnW - 40f;

            GUI.Label(new Rect(infoX, rect.y + 12f, infoW, 28f), title, stageTitleStyle);
            GUI.Label(new Rect(infoX, rect.y + 40f, infoW, 24f), status, stageStatusStyle);
            GUI.Label(new Rect(infoX, rect.y + 68f, infoW, 70f), desc, stageDescStyle);

            float btnX = rect.x + rect.width - btnW - 15f;
            float btnY = rect.y + (rect.height - 60f) / 2f;

            GUI.enabled = isEnabled;
            if (GUI.Button(new Rect(btnX, btnY, btnW, 60f), btnText, menuActionBtnStyle))
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

            // 2. Điểm & Tiêu diệt
            int score = GameManager.Instance.Score;
            int kills = GameManager.Instance.KillCount;
            float time = GameManager.Instance.SurvivalTime;
            int stage = GameManager.Instance.CurrentStage;

            float rightEdge = Screen.width - 25f;
            string stageTitleText = stage == 2 ? "🌋 MÀN 2: DUNG NHAM" : "🏰 MÀN 1: HẦM NGỤC";
            GUI.Label(new Rect(rightEdge - 420f, 15f, 420f, 32f), $"{stageTitleText}  |  ★ {score:N0} Điểm", scoreStyle);
            GUI.Label(new Rect(rightEdge - 420f, 48f, 420f, 28f), $"☠ TIÊU DIỆT: {kills} con", killsStyle);

            if (stage == 2)
            {
                GUI.Label(new Rect(bx, by + bh + 4f, 360f, 22f), "🔥 [MÀN 2: THÀNH TRÌ DUNG NHAM]", stageBannerSubStyle);
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

            float barW = Mathf.Min(Screen.width - 100f, 560f);
            float barH = 34f;
            float barX = (Screen.width - barW) / 2f;
            float barY = 58f;

            // Khung viền tím neon
            GUI.DrawTexture(new Rect(barX - 3, barY - 3, barW + 6, barH + 6), bossBorderTex);
            GUI.DrawTexture(new Rect(barX, barY, barW, barH), hpBgTex);

            // Ruột máu đỏ thẫm
            if (pct > 0f)
            {
                GUI.DrawTexture(new Rect(barX, barY, barW * pct, barH), bossFillTex);
            }

            // Tiêu đề Boss & Cảnh báo trừ 10 máu
            GUI.Label(new Rect(barX, barY + 4f, barW, 26f), $"👑 {boss.BossName} : {Mathf.CeilToInt(curHp)} / {Mathf.CeilToInt(maxHp)}", bossTitleStyle);
            GUI.Label(new Rect(barX, barY + 36f, barW, 20f), "⚠️ ĐÒN ĐÁNH CỦA BOSS TRỪ ĐÚNG 10 MÁU NGƯỜI CHƠI!", bossNoticeStyle);
        }

        // ================= VẼ BANNER CỔNG DỊCH CHUYỂN =================
        private void DrawStageClearBanner()
        {
            float bw = Mathf.Min(Screen.width - 40f, 760f);
            float bh = 60f;
            float bx = (Screen.width - bw) / 2f;
            float by = 60f;

            GUI.DrawTexture(new Rect(bx - 3, by - 3, bw + 6, bh + 6), borderTex);
            GUI.DrawTexture(new Rect(bx, by, bw, bh), popupBgTex);

            GUI.Label(new Rect(bx, by + 6f, bw, 26f), "🎉 BOSS GOLIATH ĐÃ BỊ HẠ GỤC! QUÁI VẬT ĐÃ NGỪNG XUẤT HIỆN! 🎉", menuHeaderStyle);
            GUI.Label(new Rect(bx, by + 32f, bw, 22f), "🌀 HÃY BƯỚC VÀO CỔNG DỊCH CHUYỂN PHÁT SÁNG ĐỂ TIẾN VÀO MÀN 2! 🌀", stageStatusStyle);
        }

        // ================= VẼ BẢNG CHIẾN THẮNG VƯỢT MÀN =================
        private void DrawVictoryModal()
        {
            float vw = Mathf.Min(Screen.width - 40f, 680f);
            float vh = 420f;
            float vx = (Screen.width - vw) / 2f;
            float vy = (Screen.height - vh) / 2f;

            GUI.DrawTexture(new Rect(vx - 4, vy - 4, vw + 8, vh + 8), borderTex);
            GUI.DrawTexture(new Rect(vx, vy, vw, vh), popupBgTex);

            GUI.Label(new Rect(vx, vy + 20f, vw, 36f), "🏆 CHIẾN THẮNG MÀN 1 RỰC RỠ! 🏆", menuHeaderStyle);
            GUI.Label(new Rect(vx, vy + 60f, vw, 26f), "Bạn đã đánh bại Chúa Tể Hầm Ngục & Mở Khóa Màn 2!", menuAuthorStyle);

            GUI.Label(new Rect(vx, vy + 110f, vw, 28f), $"★ Tổng Điểm Đạt Được: {GameManager.Instance.Score:N0} Điểm", gameOverStatsStyle);
            GUI.Label(new Rect(vx, vy + 145f, vw, 28f), $"☠ Số Quái Vật Hạ Gục: {GameManager.Instance.KillCount} Con", gameOverStatsStyle);
            GUI.Label(new Rect(vx, vy + 180f, vw, 28f), "🔥 Đã mở khóa: MÀN 2 - THÀNH TRÌ DUNG NHAM", stageTitleStyle);

            // Nút Tiến Vào Màn 2
            if (GUI.Button(new Rect(vx + 60f, vy + 240f, vw - 120f, 62f), "🌋 TIẾN VÀO MÀN 2 NGAY (SPACE / '2') 🌋", restartButtonStyle))
            {
                GameManager.Instance.AdvanceToStage(2);
            }

            // Nút Về Menu Chính
            if (GUI.Button(new Rect(vx + 60f, vy + 315f, vw - 120f, 48f), "🏠 VỀ MENU CHỌN MÀN CHƠI (Phím 'M')", menuActionBtnStyle))
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
