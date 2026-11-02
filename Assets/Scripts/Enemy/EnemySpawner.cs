using System;
using System.Collections.Generic;
using DungeonGame.Core;
using DungeonGame.Player;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DungeonGame.Enemy
{
    /// <summary>
    /// Hệ thống Spawner sinh quái vật xung quanh người chơi ở thế giới mở.
    /// Quái xuất hiện dồn dập ngay từ giây đầu tiên và tăng tiến độ khó theo thời gian.
    /// Tự động nạp Prefabs dự phòng nếu bị mất liên kết.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Serializable]
        public class EnemyConfig
        {
            public string enemyName;
            public GameObject prefab;
            public int spawnWeight = 10;
            public float minSpawnTime = 0f;
        }

        [Header("Danh sách các loại quái")]
        [SerializeField] private List<EnemyConfig> enemyConfigs = new List<EnemyConfig>();

        [Header("Cài đặt Spawn cự ly gần & dồn dập")]
        [Tooltip("Khoảng cách tối thiểu từ Player (ngay sát mép viền màn hình)")]
        [SerializeField] private float minSpawnRadius = 6.5f;

        [Tooltip("Khoảng cách tối đa từ Player")]
        [SerializeField] private float maxSpawnRadius = 9.5f;

        [Tooltip("Thời gian chờ giữa 2 lần spawn ban đầu (giây)")]
        [SerializeField] private float baseSpawnInterval = 0.8f;

        [Tooltip("Thời gian chờ tối thiểu khi quái spawn dồn dập nhất")]
        [SerializeField] private float minSpawnInterval = 0.3f;

        [Tooltip("Số lượng quái tối đa cùng lúc trên màn hình")]
        [SerializeField] private int maxEnemiesAlive = 60;

        private Transform playerTarget;
        private float nextSpawnTime = 0f;
        private float currentSpawnInterval;

        private void Start()
        {
            currentSpawnInterval = baseSpawnInterval;
            FindPlayer();
            ValidatePrefabs();

            // Spawn ngay 4 con quái đầu tiên ở 4 hướng ngay giây số 0 để người chơi thấy quái lao tới ngay!
            SpawnInitialWave();
        }

        private void ValidatePrefabs()
        {
            // Kiểm tra xem các prefab có bị null không, nếu có thì tự nạp lại
            bool hasValidPrefab = false;
            foreach (var cfg in enemyConfigs)
            {
                if (cfg.prefab != null)
                {
                    hasValidPrefab = true;
                    break;
                }
            }

            if (!hasValidPrefab)
            {
#if UNITY_EDITOR
                GameObject slime = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Slime.prefab");
                GameObject skeleton = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Skeleton.prefab");
                GameObject demon = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Demon.prefab");

                enemyConfigs.Clear();
                if (slime != null) enemyConfigs.Add(new EnemyConfig { enemyName = "Slime", prefab = slime, spawnWeight = 60, minSpawnTime = 0f });
                if (skeleton != null) enemyConfigs.Add(new EnemyConfig { enemyName = "Skeleton", prefab = skeleton, spawnWeight = 30, minSpawnTime = 10f });
                if (demon != null) enemyConfigs.Add(new EnemyConfig { enemyName = "Demon", prefab = demon, spawnWeight = 15, minSpawnTime = 20f });
#endif
            }
        }

        private void SpawnInitialWave()
        {
            if (playerTarget == null) FindPlayer();
            if (playerTarget == null) return;

            Vector2[] directions = new Vector2[]
            {
                Vector2.up * 5.5f,
                Vector2.down * 5.5f,
                Vector2.left * 6f,
                Vector2.right * 6f
            };

            foreach (var offset in directions)
            {
                GameObject prefab = PickRandomEnemyPrefab();
                if (prefab != null)
                {
                    Instantiate(prefab, playerTarget.position + new Vector3(offset.x, offset.y, 0f), Quaternion.identity);
                }
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
                return;

            if (playerTarget == null)
            {
                FindPlayer();
                if (playerTarget == null) return;
            }

            // Càng sống lâu quái spawn càng nhanh
            float time = GameManager.Instance != null ? GameManager.Instance.SurvivalTime : Time.time;
            currentSpawnInterval = Mathf.Max(minSpawnInterval, baseSpawnInterval - (time * 0.012f));

            if (Time.time >= nextSpawnTime)
            {
                int currentEnemyCount = UnityEngine.Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None).Length;
                if (currentEnemyCount < maxEnemiesAlive)
                {
                    int batchSize = 1 + Mathf.FloorToInt(time / 30f);
                    for (int i = 0; i < batchSize; i++)
                    {
                        SpawnRandomEnemy();
                    }
                }

                nextSpawnTime = Time.time + currentSpawnInterval;
            }
        }

        private void FindPlayer()
        {
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            if (player != null) playerTarget = player.transform;
        }

        private void SpawnRandomEnemy()
        {
            GameObject selectedPrefab = PickRandomEnemyPrefab();
            if (selectedPrefab == null)
            {
                ValidatePrefabs();
                selectedPrefab = PickRandomEnemyPrefab();
                if (selectedPrefab == null) return;
            }

            // Spawn ngay rìa màn hình để quái chạy vào thấy ngay
            Vector2 randomDir = UnityEngine.Random.insideUnitCircle.normalized;
            float randomDist = UnityEngine.Random.Range(minSpawnRadius, maxSpawnRadius);
            Vector3 spawnPos = playerTarget.position + new Vector3(randomDir.x, randomDir.y, 0f) * randomDist;

            Instantiate(selectedPrefab, spawnPos, Quaternion.identity);
        }

        private GameObject PickRandomEnemyPrefab()
        {
            if (enemyConfigs == null || enemyConfigs.Count == 0) return null;

            float currentTime = GameManager.Instance != null ? GameManager.Instance.SurvivalTime : Time.time;
            List<EnemyConfig> eligible = new List<EnemyConfig>();
            int totalWeight = 0;

            foreach (var cfg in enemyConfigs)
            {
                if (cfg.prefab != null && currentTime >= cfg.minSpawnTime)
                {
                    eligible.Add(cfg);
                    totalWeight += cfg.spawnWeight;
                }
            }

            if (eligible.Count == 0)
            {
                // Fallback nếu chưa đến minSpawnTime của quái nào khác thì luôn cho quái đầu tiên spawn
                foreach (var cfg in enemyConfigs)
                {
                    if (cfg.prefab != null) return cfg.prefab;
                }
                return null;
            }

            int roll = UnityEngine.Random.Range(0, totalWeight);
            int accum = 0;

            foreach (var cfg in eligible)
            {
                accum += cfg.spawnWeight;
                if (roll < accum)
                {
                    return cfg.prefab;
                }
            }

            return eligible[0].prefab;
        }

        public void AddEnemyConfig(string name, GameObject prefab, int weight, float minTime)
        {
            enemyConfigs.Add(new EnemyConfig
            {
                enemyName = name,
                prefab = prefab,
                spawnWeight = weight,
                minSpawnTime = minTime
            });
        }

        public void ClearConfigs()
        {
            enemyConfigs.Clear();
        }
    }
}
