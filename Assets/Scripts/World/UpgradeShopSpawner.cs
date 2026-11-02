using DungeonGame.Core;
using DungeonGame.Player;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonGame.World
{
    /// <summary>
    /// Tự động sinh ra Gian Hàng Ma Thuật (ShopStall) trên bản đồ khi người chơi đạt các mốc điểm nhất định:
    /// Ví dụ: 500, 1200, 2500, 4000, 6000, 9000, 13000 điểm...
    /// </summary>
    public class UpgradeShopSpawner : MonoBehaviour
    {
        [Header("Prefab Gian Hàng")]
        [SerializeField] private GameObject shopPrefab;

        [Header("Các mốc điểm xuất hiện gian hàng")]
        [SerializeField] private int[] scoreMilestones = new int[] { 300, 700, 1500, 2500, 4000, 6000, 8500, 12000 };

        [Header("Khoảng cách xuất hiện so với Player")]
        [SerializeField] private float spawnDistance = 5f;

        private HashSet<int> triggeredMilestones = new HashSet<int>();
        private Transform playerTransform;

        private string bannerMessage = "";
        private float bannerTimer = 0f;
        private GUIStyle bannerStyle;

        private void Start()
        {
            FindPlayer();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged += HandleScoreChanged;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged -= HandleScoreChanged;
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

        private void Update()
        {
            if (bannerTimer > 0)
            {
                bannerTimer -= Time.deltaTime;
                if (bannerTimer <= 0) bannerMessage = "";
            }

            // Fallback kiểm tra điểm liên tục nếu sự kiện không kích hoạt
            if (GameManager.Instance != null)
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
            foreach (int milestone in scoreMilestones)
            {
                if (score >= milestone && !triggeredMilestones.Contains(milestone))
                {
                    triggeredMilestones.Add(milestone);
                    SpawnShop(milestone);
                    break;
                }
            }
        }

        private void SpawnShop(int milestone)
        {
            if (playerTransform == null) FindPlayer();
            if (playerTransform == null) return;

            // Tính vị trí ngẫu nhiên quanh người chơi ~4 đến 6 mét
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
                // Fallback nếu chưa gán prefab
                var loaded = Resources.Load<GameObject>("Shop_Stall");
                if (loaded != null)
                {
                    shopInstance = Instantiate(loaded, spawnPos, Quaternion.identity);
                }
            }

            // Hiển thị Banner thông báo hoàng gia
            bannerMessage = $"⚡ MỐC {milestone} ĐIỂM! GIAN HÀNG MA THUẬT ĐÃ XUẤT HIỆN GẦN BẠN! ⚡";
            bannerTimer = 4.5f;

            Debug.Log($"[UpgradeShopSpawner] Gian hàng đã xuất hiện tại {spawnPos} (Mốc {milestone} điểm)");
        }

        private void OnGUI()
        {
            if (string.IsNullOrEmpty(bannerMessage)) return;

            if (bannerStyle == null)
            {
                bannerStyle = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 20,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                bannerStyle.normal.textColor = new Color(1f, 0.9f, 0.2f);
            }

            float width = 600f;
            float height = 50f;
            float x = (Screen.width - width) / 2f;
            float y = 80f;

            GUI.Box(new Rect(x, y, width, height), bannerMessage, bannerStyle);
        }
    }
}
