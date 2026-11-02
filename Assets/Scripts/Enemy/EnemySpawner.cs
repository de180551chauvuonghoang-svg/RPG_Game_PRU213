using System;
using System.Collections.Generic;
using DungeonGame.Core;
using DungeonGame.Player;
using UnityEngine;

namespace DungeonGame.Enemy
{
    /// <summary>
    /// Hệ thống Spawner sinh quái vật xung quanh người chơi ở thế giới mở.
    /// Tần suất và số lượng quái tăng dần theo thời gian sinh tồn (Difficulty Scaling).
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Serializable]
        public class EnemyConfig
        {
            public string enemyName;
            public GameObject prefab;
            public int spawnWeight = 10; // Tỉ lệ xuất hiện
            public float minSpawnTime = 0f; // Thời gian tối thiểu (giây) để bắt đầu xuất hiện
        }

        [Header("Danh sách các loại quái")]
        [SerializeField] private List<EnemyConfig> enemyConfigs = new List<EnemyConfig>();

        [Header("Cài đặt Spawn")]
        [Tooltip("Khoảng cách tối thiểu từ Player (ngoài viền màn hình)")]
        [SerializeField] private float minSpawnRadius = 14f;

        [Tooltip("Khoảng cách tối đa từ Player")]
        [SerializeField] private float maxSpawnRadius = 20f;

        [Tooltip("Thời gian chờ giữa 2 lần spawn ban đầu (giây)")]
        [SerializeField] private float baseSpawnInterval = 1.5f;

        [Tooltip("Thời gian chờ tối thiểu khi quái spawn dồn dập nhất")]
        [SerializeField] private float minSpawnInterval = 0.4f;

        [Tooltip("Số lượng quái tối đa cùng lúc trên màn hình")]
        [SerializeField] private int maxEnemiesAlive = 70;

        private Transform playerTarget;
        private float nextSpawnTime = 0f;
        private float currentSpawnInterval;

        private void Start()
        {
            currentSpawnInterval = baseSpawnInterval;
            FindPlayer();
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
                return;

            if (playerTarget == null)
            {
                FindPlayer();
                return;
            }

            // Tăng dần độ khó: Càng sống lâu quái spawn càng nhanh
            float time = GameManager.Instance != null ? GameManager.Instance.SurvivalTime : Time.time;
            currentSpawnInterval = Mathf.Max(minSpawnInterval, baseSpawnInterval - (time * 0.015f));

            if (Time.time >= nextSpawnTime)
            {
                // Kiểm tra số lượng quái hiện tại
                int currentEnemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
                if (currentEnemyCount < maxEnemiesAlive)
                {
                    // Khi sống lâu hơn, mỗi đợt có thể spawn từ 1 đến 3 con cùng lúc
                    int batchSize = 1 + Mathf.FloorToInt(time / 45f);
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
            if (selectedPrefab == null) return;

            // Tính vị trí spawn ngẫu nhiên trên đường tròn ngoài tầm nhìn Camera
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

            if (eligible.Count == 0) return null;

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
