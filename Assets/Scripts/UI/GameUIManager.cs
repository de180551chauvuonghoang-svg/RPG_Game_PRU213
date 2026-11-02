using System;
using DungeonGame.Combat;
using DungeonGame.Core;
using DungeonGame.Player;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace DungeonGame.UI
{
    /// <summary>
    /// Quản lý giao diện HUD và Màn hình Chơi Lại (GameOver):
    /// - Thanh Máu 100% co ngắn lại khi bị quái trừ 5 máu
    /// - Khi máu về 0: Hiện ngay bảng Game Over ở giữa màn hình với nút CHƠI LẠI
    /// - Hỗ trợ bấm phím 'R' hoặc click chuột để Chơi Lại ngay lập tức
    /// - Điểm số vàng kim và số quái hạ gục thời gian thực
    /// </summary>
    public class GameUIManager : MonoBehaviour
    {
        public static GameUIManager Instance { get; private set; }

        private HealthComponent playerHealth;

        // Textures vẽ thanh máu
        private Texture2D hpFillTex;
        private Texture2D hpBgTex;
        private Texture2D borderTex;
        private Texture2D popupBgTex;

        // GUIStyles
        private GUIStyle titleStyle;
        private GUIStyle scoreStyle;
        private GUIStyle killsStyle;
        private GUIStyle hpTextStyle;
        private GUIStyle gameOverHeaderStyle;
        private GUIStyle gameOverStatsStyle;
        private GUIStyle restartButtonStyle;

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
            hpFillTex = MakeTex(new Color(0.95f, 0.2f, 0.25f, 1f));   // Đỏ tươi ruột máu
            hpBgTex = MakeTex(new Color(0.12f, 0.12f, 0.16f, 0.95f));  // Nền tối thanh máu
            borderTex = MakeTex(new Color(1f, 0.85f, 0.2f, 1f));       // Viền vàng kim
            popupBgTex = MakeTex(new Color(0.08f, 0.08f, 0.12f, 0.96f)); // Nền tối popup Game Over
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
                fontSize = 26,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight
            };
            scoreStyle.normal.textColor = new Color(1f, 0.88f, 0.2f);

            killsStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight
            };
            killsStyle.normal.textColor = new Color(1f, 0.4f, 0.4f);

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            titleStyle.normal.textColor = Color.white;

            hpTextStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            hpTextStyle.normal.textColor = Color.white;

            gameOverHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            gameOverHeaderStyle.normal.textColor = new Color(1f, 0.25f, 0.25f);

            gameOverStatsStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            gameOverStatsStyle.normal.textColor = Color.white;

            restartButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            restartButtonStyle.normal.textColor = Color.yellow;
        }

        private void Update()
        {
            if (playerHealth == null)
            {
                FindPlayerHealth();
            }

            // Kiểm tra nếu máu về 0 thì kích hoạt Game Over
            if (playerHealth != null && playerHealth.IsDead && !isGameOver)
            {
                isGameOver = true;
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetState(GameState.GameOver);
                }
            }

            // Khi đang Game Over, cho phép bấm phím R hoặc Space để Chơi Lại ngay
            if (isGameOver)
            {
                bool wantsRestart = false;

#if ENABLE_INPUT_SYSTEM
                if (Keyboard.current != null && (Keyboard.current.rKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
                {
                    wantsRestart = true;
                }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
                try
                {
                    if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                    {
                        wantsRestart = true;
                    }
                }
                catch {}
#endif

                if (wantsRestart)
                {
                    RestartGame();
                }
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state == GameState.GameOver)
            {
                isGameOver = true;
            }
        }

        private void OnGUI()
        {
            InitStyles();

            // 1. THANH MÁU HIỆP SĨ (GÓC TRÊN BÊN TRÁI) - CO NGẮN THEO ĐÚNG % MÁU
            float curHp = playerHealth != null ? Mathf.Max(0, playerHealth.CurrentHealth) : 100f;
            float maxHp = playerHealth != null ? playerHealth.MaxHealth : 100f;
            float hpPct = Mathf.Clamp01(curHp / maxHp);

            float bx = 25f;
            float by = 25f;
            float bw = 260f;
            float bh = 32f;
            float border = 3f;

            // Viền vàng kim
            GUI.DrawTexture(new Rect(bx - border, by - border, bw + border * 2, bh + border * 2), borderTex);
            // Nền đen bên trong
            GUI.DrawTexture(new Rect(bx, by, bw, bh), hpBgTex);
            // Ruột đỏ co ngắn theo % máu
            if (hpPct > 0f)
            {
                GUI.DrawTexture(new Rect(bx, by, bw * hpPct, bh), hpFillTex);
            }
            // Chữ hiển thị
            GUI.Label(new Rect(bx, by, bw, bh), $"MÁU: {Mathf.CeilToInt(curHp)} / {Mathf.CeilToInt(maxHp)} ({Mathf.RoundToInt(hpPct * 100)}%)", hpTextStyle);

            // 2. ĐIỂM SỐ VÀ TIÊU DIỆT (GÓC TRÊN BÊN PHẢI)
            int score = GameManager.Instance != null ? GameManager.Instance.Score : 0;
            int kills = GameManager.Instance != null ? GameManager.Instance.KillCount : 0;
            float time = GameManager.Instance != null ? GameManager.Instance.SurvivalTime : Time.time;

            float rightEdge = Screen.width - 25f;
            GUI.Label(new Rect(rightEdge - 320f, 20f, 320f, 35f), $"★ ĐIỂM: {score:N0}", scoreStyle);
            GUI.Label(new Rect(rightEdge - 320f, 55f, 320f, 30f), $"☠ TIÊU DIỆT: {kills}", killsStyle);

            // 3. ĐỒNG HỒ THỜI GIAN (CHÍNH GIỮA TRÊN ĐỈNH)
            int min = Mathf.FloorToInt(time / 60f);
            int sec = Mathf.FloorToInt(time % 60f);
            GUI.Label(new Rect((Screen.width / 2f) - 150f, 20f, 300f, 35f), $"⏱ THỜI GIAN: {min:00}:{sec:00}", titleStyle);

            // 4. BẢNG GAME OVER & NÚT CHƠI LẠI KHI MÁU VỀ 0
            if (isGameOver || curHp <= 0f)
            {
                float pw = 520f;
                float ph = 320f;
                float px = (Screen.width - pw) / 2f;
                float py = (Screen.height - ph) / 2f;

                // Khung popup tối viền vàng
                GUI.DrawTexture(new Rect(px - 4, py - 4, pw + 8, ph + 8), borderTex);
                GUI.DrawTexture(new Rect(px, py, pw, ph), popupBgTex);

                // Tiêu đề
                GUI.Label(new Rect(px, py + 20f, pw, 40f), "⚔️ HIỆP SĨ ĐÃ HY SINH! ⚔️", gameOverHeaderStyle);

                // Thống kê thành tích
                GUI.Label(new Rect(px, py + 80f, pw, 30f), $"★ Tổng Điểm Đạt Được: {score:N0}", gameOverStatsStyle);
                GUI.Label(new Rect(px, py + 115f, pw, 30f), $"☠ Số Quái Tiêu Diệt: {kills} con", gameOverStatsStyle);
                GUI.Label(new Rect(px, py + 150f, pw, 30f), $"⏱ Thời Gian Sống Sót: {min:00} phút {sec:00} giây", gameOverStatsStyle);

                // Nút bấm Chơi Lại
                if (GUI.Button(new Rect(px + 60f, py + 215f, pw - 120f, 60f), "🔄 CHƠI LẠI (BẤM PHÍM 'R')", restartButtonStyle))
                {
                    RestartGame();
                }
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
