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
    /// Hệ thống Spawner sinh quái vật với nhịp độ cân bằng (Balanced Pacing):
    /// - 2.5 giây đầu cho người chơi chuẩn bị và di chuyển
    /// - Quái xuất hiện từ tốn (cứ 2s/con) ở cự ly ngoài màn hình (9 - 12 mét)
    /// - Tốc độ vừa phải để người chơi ngắm bắn thoải mái
    /// - Độ khó và số lượng quái tăng dần theo thời gian
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

        [Header("Cài đặt Spawn nhịp độ hợp lý")]
        [Tooltip("Thời gian chờ ban đầu trước khi con quái đầu tiên xuất hiện (giây)")]
        [SerializeField] private float initialDelay = 2.5f;

        [Tooltip("Khoảng cách tối thiểu từ Player (ngoài mép màn hình một khoảng vừa đủ)")]
        [SerializeField] private float minSpawnRadius = 9.0f;

        [Tooltip("Khoảng cách tối đa từ Player")]
        [SerializeField] private float maxSpawnRadius = 12.0f;

        [Tooltip("Thời gian chờ giữa 2 lần spawn ban đầu (giây)")]
        [SerializeField] private float baseSpawnInterval = 2.0f;

        [Tooltip("Thời gian chờ tối thiểu khi quái spawn nhanh nhất về sau")]
        [SerializeField] private float minSpawnInterval = 0.6f;

        [Tooltip("Số lượng quái tối đa cùng lúc trên màn hình")]
        [SerializeField] private int maxEnemiesAlive = 50;

        private Transform playerTarget;
        private float nextSpawnTime = 0f;
        private float currentSpawnInterval;

        private void Start()
        {
            currentSpawnInterval = baseSpawnInterval;
            // Cho người chơi 2.5 giây đầu để chuẩn bị và định hình
            nextSpawnTime = Time.time + initialDelay;

            FindPlayer();
            ValidatePrefabs();
        }

        private void ValidatePrefabs()
        {
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
                if (skeleton != null) enemyConfigs.Add(new EnemyConfig { enemyName = "Skeleton", prefab = skeleton, spawnWeight = 30, minSpawnTime = 15f });
                if (demon != null) enemyConfigs.Add(new EnemyConfig { enemyName = "Demon", prefab = demon, spawnWeight = 15, minSpawnTime = 30f });
#endif
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

            // Nhịp độ tăng dần từ tốn theo thời gian sống sót
            float time = GameManager.Instance != null ? GameManager.Instance.SurvivalTime : Time.time;
            currentSpawnInterval = Mathf.Max(minSpawnInterval, baseSpawnInterval - (time * 0.01f));

            if (Time.time >= nextSpawnTime)
            {
                int currentEnemyCount = UnityEngine.Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None).Length;
                if (currentEnemyCount < maxEnemiesAlive)
                {
                    // Ban đầu spawn từng con một, sau 45s mới bắt đầu spawn cụm 2 con
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
            if (selectedPrefab == null)
            {
                ValidatePrefabs();
                selectedPrefab = PickRandomEnemyPrefab();
                if (selectedPrefab == null) return;
            }

            // Spawn ngoài mép màn hình một khoảng an toàn
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
