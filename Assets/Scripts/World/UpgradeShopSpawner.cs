using DungeonGame.Core;
using DungeonGame.Player;
using UnityEngine;

namespace DungeonGame.World
{
    /// <summary>
    /// Quản lý việc sinh Gian Hàng Ma Thuật (ShopStall) theo cơ chế tuần tự:
    /// - Mỗi thời điểm chỉ tồn tại DUY NHẤT 1 gian hàng trên bản đồ.
    /// - Nếu người chơi chưa có tiền, có thể bấm Space/ESC để tiếp tục đánh quái gom coin và quay lại shop bất cứ lúc nào.
    /// - Chỉ khi người chơi THỰC SỰ CHỌN NÂNG CẤP tại shop hiện tại, shop đó mới hoàn thành nhiệm vụ và biến mất.
    /// - Khi shop đã nâng cấp xong, gian hàng tiếp theo MỚI ĐƯỢC PHÉP XUẤT HIỆN tại mốc điểm kế tiếp!
    /// </summary>
    public class UpgradeShopSpawner : MonoBehaviour
    {
        public static UpgradeShopSpawner Instance { get; private set; }

        [Header("Prefab Gian Hàng")]
        [SerializeField] private GameObject shopPrefab;

        [Header("Các mốc điểm xuất hiện gian hàng tuần tự")]
        [SerializeField] private int[] scoreMilestones = new int[] { 300, 700, 1500, 2500, 4000, 6000, 8500, 12000, 16000, 20000 };

        [Header("Khoảng cách xuất hiện so với Player")]
        [SerializeField] private float spawnDistance = 5.5f;

        private int currentMilestoneIndex = 0;
        private GameObject currentActiveShop = null;
        private bool isWaitingForUpgrade = false;
        private float nextShopCooldown = 0f;
        private Transform playerTransform;

        private string bannerMessage = "";
        private float bannerTimer = 0f;
        private GUIStyle bannerStyle;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            FindPlayer();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged += HandleScoreChanged;
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
                GameManager.Instance.OnStageCompletedEvent += HandleStageCompleted;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged -= HandleScoreChanged;
                GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
                GameManager.Instance.OnStageCompletedEvent -= HandleStageCompleted;
            }
        }

        private void FindPlayer()
        {
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state == GameState.MainMenu)
            {
                ResetSpawner();
            }
        }

        private void HandleStageCompleted(int completedStage)
        {
            // Khi vượt qua màn chơi, dọn dẹp shop cũ của màn trước nếu chưa dùng để sẵn sàng cho màn mới
            if (currentActiveShop != null)
            {
                Destroy(currentActiveShop);
                currentActiveShop = null;
            }
            isWaitingForUpgrade = false;
            nextShopCooldown = 2.0f;
        }

        public void ResetSpawner()
        {
            if (currentActiveShop != null)
            {
                Destroy(currentActiveShop);
                currentActiveShop = null;
            }
            isWaitingForUpgrade = false;
            currentMilestoneIndex = 0;
            nextShopCooldown = 0f;
            bannerMessage = "";
            bannerTimer = 0f;
        }

        private void Update()
        {
            if (nextShopCooldown > 0f)
            {
                nextShopCooldown -= Time.deltaTime;
            }

            if (bannerTimer > 0f)
            {
                bannerTimer -= Time.deltaTime;
                if (bannerTimer <= 0f) bannerMessage = "";
            }

            // Đồng bộ kiểm tra điểm nếu đang trong trạng thái Playing
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
            {
                CheckScore(GameManager.Instance.Score);
            }
        }

        private void HandleScoreChanged(int newScore)
        {
            CheckScore(newScore);
        }

        public void CheckScore(int score)
        {
            // 1. Nếu đang có 1 gian hàng tồn tại và chưa được nâng cấp: KHÔNG BAO GIỜ SPAWN THÊM GIAN HÀNG KHÁC!
            if (isWaitingForUpgrade || currentActiveShop != null)
            {
                return;
            }

            // 2. Nếu đang trong thời gian giãn cách sau khi nâng cấp shop trước:
            if (nextShopCooldown > 0f)
            {
                return;
            }

            // 3. Nếu đã hết danh sách mốc điểm:
            if (currentMilestoneIndex >= scoreMilestones.Length)
            {
                return;
            }

            // 4. Kiểm tra xem điểm số đã đạt mốc của shop hiện tại chưa
            int targetMilestone = scoreMilestones[currentMilestoneIndex];
            if (score >= targetMilestone)
            {
                SpawnShop(targetMilestone, currentMilestoneIndex + 1);
            }
        }

        private void SpawnShop(int milestone, int shopNumber)
        {
            if (playerTransform == null) FindPlayer();
            if (playerTransform == null) return;

            // Đảm bảo không trùng lặp
            if (currentActiveShop != null) return;

            // Tính vị trí ngẫu nhiên quanh người chơi ~4.5 đến 5.5 mét
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            if (randomDir.sqrMagnitude < 0.1f) randomDir = Vector2.up;
            Vector3 spawnPos = playerTransform.position + (Vector3)(randomDir * spawnDistance);

            GameObject shopInstance = null;

            if (shopPrefab != null)
            {
                shopInstance = Instantiate(shopPrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                var loaded = Resources.Load<GameObject>("Shop_Stall");
#if UNITY_EDITOR
                if (loaded == null) loaded = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Shop_Stall.prefab");
#endif
                if (loaded != null)
                {
                    shopInstance = Instantiate(loaded, spawnPos, Quaternion.identity);
                }
            }

            if (shopInstance != null)
            {
                currentActiveShop = shopInstance;
                isWaitingForUpgrade = true;

                // Hiển thị Banner thông báo hoàng gia
                bannerMessage = $"⚡ MỐC {milestone:N0} ĐIỂM! GIAN HÀNG MA THUẬT (SỐ {shopNumber}) ĐÃ XUẤT HIỆN! ⚡";
                bannerTimer = 4.5f;

                Debug.Log($"<color=cyan><b>[UpgradeShopSpawner] Gian hàng số {shopNumber} đã xuất hiện tại {spawnPos} (Mốc {milestone} điểm). Chờ nâng cấp để mở shop tiếp theo!</b></color>");
            }
        }

        /// <summary>
        /// Được gọi từ ShopStall khi người chơi chọn và thanh toán nâng cấp thành công
        /// </summary>
        public void NotifyShopUpgraded(ShopStall shop)
        {
            if (currentActiveShop == null || currentActiveShop == shop.gameObject)
            {
                currentActiveShop = null;
            }
            isWaitingForUpgrade = false;
            currentMilestoneIndex++;
            nextShopCooldown = 3.5f; // Giãn cách 3.5 giây trước khi shop tiếp theo có thể xuất hiện

            bannerMessage = $"✨ NÂNG CẤP THÀNH CÔNG! GIAN HÀNG TIẾP THEO SẼ MỞ RA Ở MỐC ĐIỂM KẾ TIẾP! ✨";
            bannerTimer = 4.0f;
        }

        private void OnGUI()
        {
            if (string.IsNullOrEmpty(bannerMessage)) return;

            if (bannerStyle == null)
            {
                bannerStyle = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 18,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                bannerStyle.normal.textColor = new Color(1f, 0.9f, 0.2f);
            }

            float width = 680f;
            float height = 50f;
            float x = (Screen.width - width) / 2f;
            float y = 82f;

            GUI.Box(new Rect(x, y, width, height), bannerMessage, bannerStyle);
        }
    }
}
