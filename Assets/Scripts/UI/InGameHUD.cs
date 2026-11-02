using DungeonGame.Combat;
using DungeonGame.Core;
using DungeonGame.Player;
using UnityEngine;

namespace DungeonGame.UI
{
    /// <summary>
    /// Giao diện HUD hiển thị trực tiếp lên màn hình game (OnGUI + Screen overlay).
    /// Đảm bảo luôn luôn hiển thị 100% không phụ thuộc Canvas hay Font ngoài:
    /// - Thanh Máu 100% của Hiệp Sĩ (Mất đúng 5 máu khi quái chạm vào)
    /// - Điểm số nhảy màu vàng kim
    /// - Số quái tiêu diệt
    /// - Thời gian sống sót
    /// - Màn hình Game Over khi hết máu
    /// </summary>
    public class InGameHUD : MonoBehaviour
    {
        private HealthComponent playerHealth;
        private GUIStyle titleStyle;
        private GUIStyle scoreStyle;
        private GUIStyle killsStyle;
        private GUIStyle hpTextStyle;
        private GUIStyle gameOverStyle;
        private GUIStyle buttonStyle;
        private Texture2D hpFillTex;
        private Texture2D hpBgTex;
        private Texture2D borderTex;

        private void Awake()
        {
            CreateTextures();
        }

        private void Start()
        {
            FindPlayerHealth();
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
            hpFillTex = MakeColorTex(new Color(0.9f, 0.2f, 0.25f, 1f)); // Đỏ tươi
            hpBgTex = MakeColorTex(new Color(0.15f, 0.15f, 0.2f, 0.9f));  // Nền tối
            borderTex = MakeColorTex(new Color(0.85f, 0.75f, 0.25f, 1f)); // Viền vàng kim
        }

        private Texture2D MakeColorTex(Color col)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, col);
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
            scoreStyle.normal.textColor = new Color(1f, 0.88f, 0.2f); // Vàng kim

            killsStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight
            };
            killsStyle.normal.textColor = new Color(1f, 0.45f, 0.45f); // Đỏ cam

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            titleStyle.normal.textColor = Color.white;

            hpTextStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            hpTextStyle.normal.textColor = Color.white;

            gameOverStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 32,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            gameOverStyle.normal.textColor = Color.red;

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold
            };
            buttonStyle.normal.textColor = Color.white;
        }

        private void OnGUI()
        {
            InitStyles();

            if (playerHealth == null)
            {
                FindPlayerHealth();
            }

            // 1. VẼ THANH MÁU HIỆP SĨ (100% HP) Ở GÓC TRÊN BÊN TRÁI
            float currentHp = playerHealth != null ? playerHealth.CurrentHealth : 100f;
            float maxHp = playerHealth != null ? playerHealth.MaxHealth : 100f;
            float hpPercent = Mathf.Clamp01(currentHp / maxHp);

            float barX = 25f;
            float barY = 25f;
            float barW = 280f;
            float barH = 34f;
            float border = 3f;

            // Viền vàng kim ngoài
            GUI.DrawTexture(new Rect(barX - border, barY - border, barW + border * 2, barH + border * 2), borderTex);
            // Nền đen bên trong
            GUI.DrawTexture(new Rect(barX, barY, barW, barH), hpBgTex);
            // Ruột đỏ thể hiện % máu còn lại
            GUI.DrawTexture(new Rect(barX, barY, barW * hpPercent, barH), hpFillTex);
            // Text hiển thị máu (HP: 100 / 100)
            GUI.Label(new Rect(barX, barY, barW, barH), $"MÁU: {Mathf.CeilToInt(currentHp)} / {Mathf.CeilToInt(maxHp)} ({Mathf.RoundToInt(hpPercent * 100)}%)", hpTextStyle);

            // 2. VẼ ĐIỂM SỐ VÀ TIÊU DIỆT Ở GÓC TRÊN BÊN PHẢI
            int score = GameManager.Instance != null ? GameManager.Instance.Score : 0;
            int kills = GameManager.Instance != null ? GameManager.Instance.KillCount : 0;
            float time = GameManager.Instance != null ? GameManager.Instance.SurvivalTime : Time.time;

            float rightMargin = Screen.width - 25f;
            GUI.Label(new Rect(rightMargin - 350f, 20f, 350f, 40f), $"★ ĐIỂM: {score:N0}", scoreStyle);
            GUI.Label(new Rect(rightMargin - 350f, 60f, 350f, 35f), $"☠ TIÊU DIỆT: {kills}", killsStyle);

            // 3. VẼ ĐỒNG HỒ THỜI GIAN Ở CHÍNH GIỮA TRÊN ĐỈNH
            int min = Mathf.FloorToInt(time / 60f);
            int sec = Mathf.FloorToInt(time % 60f);
            GUI.Label(new Rect((Screen.width / 2f) - 150f, 20f, 300f, 40f), $"⏱ THỜI GIAN: {min:00}:{sec:00}", titleStyle);

            // 4. MÀN HÌNH GAME OVER KHI MÁU = 0
            if (playerHealth != null && playerHealth.IsDead)
            {
                float panelW = 450f;
                float panelH = 300f;
                float px = (Screen.width - panelW) / 2f;
                float py = (Screen.height - panelH) / 2f;

                GUI.Box(new Rect(px, py, panelW, panelH), "HIỆP SĨ ĐÃ HY SINH!\n(GAME OVER)", gameOverStyle);

                GUI.Label(new Rect(px, py + 90f, panelW, 40f), $"Tổng Điểm: {score:N0}", titleStyle);
                GUI.Label(new Rect(px, py + 130f, panelW, 40f), $"Quái Hạ Gục: {kills}", titleStyle);

                if (GUI.Button(new Rect(px + 100f, py + 200f, panelW - 200f, 50f), "CHƠI LẠI (RESTART)", buttonStyle))
                {
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.RestartCurrentScene();
                    }
                }
            }
        }
    }
}
