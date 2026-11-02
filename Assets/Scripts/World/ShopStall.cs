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
    /// Cho phép người chơi nâng cấp vũ khí (Bắn 3 tia, Sát thương khủng, Bắn siêu nhanh, Hồi 100% máu).
    /// </summary>
    public class ShopStall : MonoBehaviour
    {
        [Header("Khoảng cách tương tác")]
        [SerializeField] private float interactRadius = 3.2f;

        private Transform playerTransform;
        private PlayerCombat playerCombat;
        private HealthComponent playerHealth;

        private bool isPlayerNearby = false;
        private bool isShopOpen = false;
        private bool isUsed = false;

        private GUIStyle shopBoxStyle;
        private GUIStyle shopTitleStyle;
        private GUIStyle shopDescStyle;
        private GUIStyle itemBtnStyle;
        private GUIStyle promptStyle;
        private Texture2D shopBgTex;
        private Texture2D borderTex;

        private void Start()
        {
            FindPlayer();
            CreateShopTextures();
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

        private void CreateShopTextures()
        {
            shopBgTex = new Texture2D(1, 1);
            shopBgTex.SetPixel(0, 0, new Color(0.1f, 0.08f, 0.15f, 0.98f));
            shopBgTex.Apply();

            borderTex = new Texture2D(1, 1);
            borderTex.SetPixel(0, 0, new Color(1f, 0.85f, 0.2f, 1f));
            borderTex.Apply();
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

            shopDescStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleCenter
            };
            shopDescStyle.normal.textColor = Color.white;

            promptStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            promptStyle.normal.textColor = Color.yellow;

            itemBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            itemBtnStyle.normal.textColor = Color.white;
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

            if (isPlayerNearby && !isUsed)
            {
                // Nhấn phím E hoặc tự động mở khi bước vào
                bool pressE = false;
#if ENABLE_INPUT_SYSTEM
                if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) pressE = true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
                try { if (Input.GetKeyDown(KeyCode.E)) pressE = true; } catch {}
#endif
                if (pressE || dist <= 1.8f)
                {
                    isShopOpen = true;
                }
            }
            else
            {
                isShopOpen = false;
            }
        }

        private void OnGUI()
        {
            InitStyles();

            // 1. HIỆN GỢI Ý KHI PLAYER ĐẾN GẦN GIAN HÀNG
            if (isPlayerNearby && !isShopOpen && !isUsed)
            {
                float pw = 420f;
                float ph = 45f;
                float px = (Screen.width - pw) / 2f;
                float py = Screen.height - 120f;

                GUI.Box(new Rect(px, py, pw, ph), "⚡ BƯỚC VÀO HOẶC NHẤN 'E' ĐỂ NÂNG CẤP VŨ KHÍ ⚡", promptStyle);
            }

            // 2. BẢNG NÂNG CẤP VŨ KHÍ KHI MỞ GIAN HÀNG
            if (isShopOpen && !isUsed)
            {
                float sw = 640f;
                float sh = 420f;
                float sx = (Screen.width - sw) / 2f;
                float sy = (Screen.height - sh) / 2f;

                // Nền và viền vàng kim
                GUI.DrawTexture(new Rect(sx - 4, sy - 4, sw + 8, sh + 8), borderTex);
                GUI.DrawTexture(new Rect(sx, sy, sw, sh), shopBgTex);

                // Tiêu đề
                GUI.Label(new Rect(sx, sy + 15f, sw, 35f), "🏪 GIAN HÀNG MA THUẬT - CHỌN 1 NÂNG CẤP VŨ KHÍ", shopTitleStyle);
                GUI.Label(new Rect(sx, sy + 50f, sw, 25f), "Phần thưởng đặc biệt khi đạt mốc điểm sinh tồn!", shopDescStyle);

                // Option 1: Bắn 3 tia
                if (GUI.Button(new Rect(sx + 40f, sy + 90f, sw - 80f, 60f), 
                    $"🏹 HỎA LONG TAM XẠ (BẮN 3 TIA CHÙM CÁNH QUẠT)\n{(playerCombat != null ? $"Hiện tại: {playerCombat.MultiShotCount} tia -> Nâng cấp lên: {playerCombat.MultiShotCount + 2} tia" : "")}", itemBtnStyle))
                {
                    ApplyUpgrade(1);
                }

                // Option 2: Sát thương khủng
                if (GUI.Button(new Rect(sx + 40f, sy + 165f, sw - 80f, 60f), 
                    $"💥 CỰC ĐẠI SÁT THƯƠNG (+60% DAME, ĐẠN TO GẤP ĐÔI)\n{(playerCombat != null ? $"Hệ số Dame hiện tại: x{playerCombat.DamageMultiplier:F1}" : "")}", itemBtnStyle))
                {
                    ApplyUpgrade(2);
                }

                // Option 3: Bắn siêu tốc
                if (GUI.Button(new Rect(sx + 40f, sy + 240f, sw - 80f, 60f), 
                    $"⚡ PHONG BẠO TỐC ĐỘ (+50% TỐC ĐỘ XẢ ĐẠN)\n{(playerCombat != null ? $"Tốc độ bắn: x{playerCombat.FireRateMultiplier:F1}" : "")}", itemBtnStyle))
                {
                    ApplyUpgrade(3);
                }

                // Option 4: Hồi 100% máu
                if (GUI.Button(new Rect(sx + 40f, sy + 315f, sw - 80f, 50f), 
                    "💖 THÁNH THỦY HỒI PHỤC (HỒI ĐẦY 100% MÁU NGAY LẬP TỨC)", itemBtnStyle))
                {
                    ApplyUpgrade(4);
                }

                // Nút đóng
                if (GUI.Button(new Rect(sx + sw - 45f, sy + 10f, 35f, 30f), "X", itemBtnStyle))
                {
                    isShopOpen = false;
                }
            }
        }

        private void ApplyUpgrade(int option)
        {
            if (playerCombat == null || playerHealth == null) FindPlayer();

            switch (option)
            {
                case 1:
                    if (playerCombat != null) playerCombat.UpgradeMultiShot();
                    break;
                case 2:
                    if (playerCombat != null) playerCombat.UpgradeDamage();
                    break;
                case 3:
                    if (playerCombat != null) playerCombat.UpgradeFireRate();
                    break;
                case 4:
                    if (playerHealth != null) playerHealth.Heal(100f);
                    break;
            }

            isUsed = true;
            isShopOpen = false;

            // Đổi màu gian hàng sang màu xám sau khi mua xong
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = new Color(0.5f, 0.5f, 0.6f, 0.7f);

            // Tự hủy gian hàng sau 5 giây để dọn map
            Destroy(gameObject, 5f);
        }
    }
}
