using System;
using DungeonGame.Enemy;
using DungeonGame.Player;
using DungeonGame.World;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DungeonGame.Core
{
    public enum GameState
    {
        MainMenu,   // Màn hình chọn màn / trước khi chơi
        Playing,    // Đang chiến đấu
        Paused,     // Tạm dừng
        StageClear, // Tiêu diệt Boss thành công, Cổng dịch chuyển đã mở
        GameOver,   // Hiệp sĩ hy sinh
        Victory     // Toàn thắng
    }

    /// <summary>
    /// Quản lý trạng thái chung của Game:
    /// - Quản lý Màn 1, Màn 2 và Tiến độ các màn đã vượt qua (PlayerPrefs)
    /// - Quản lý mốc điểm triệu hồi Boss Cuối (Màn 1: 1,000 điểm; Màn 2: 2,500 điểm)
    /// - Khi Boss chết: Ngừng sinh quái, triệu hồi Cổng Dịch Chuyển qua màn kế tiếp
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Trạng thái Game")]
        [SerializeField] private GameState currentState = GameState.MainMenu;

        [Header("Quản lý Màn chơi")]
        [SerializeField] private int currentStage = 1;
        [SerializeField] private int bossScoreThreshold = 1000;

        [Header("Prefab Boss & Cổng Dịch Chuyển")]
        [SerializeField] private GameObject bossPrefab;
        [SerializeField] private GameObject portalPrefab;

        [Header("Thống kê trận đánh")]
        [SerializeField] private int score = 0;
        [SerializeField] private int killCount = 0;
        [SerializeField] private float survivalTime = 0f;

        private BossController activeBoss;
        private bool isBossSpawned = false;
        private bool isBossDefeated = false;

        // Events
        public event Action<int> OnScoreChanged;
        public event Action<int> OnKillCountChanged;
        public event Action<float> OnSurvivalTimeChanged;
        public event Action<GameState> OnGameStateChanged;
        public event Action<BossController> OnBossSpawned;
        public event Action OnBossDefeatedEvent;
        public event Action<int> OnStageCompletedEvent;

        public GameState CurrentState => currentState;
        public int Score => score;
        public int KillCount => killCount;
        public float SurvivalTime => survivalTime;
        public int CurrentStage => currentStage;
        public BossController ActiveBoss => activeBoss;
        public bool IsBossActive => activeBoss != null && !activeBoss.IsDead;
        public int HighestStageCleared => PlayerPrefs.GetInt("HighestStageCleared", 0);

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
            // Ban đầu hiển thị Màn hình trước khi vào chơi (MainMenu)
            SetState(GameState.MainMenu);
        }

        private void Update()
        {
            if (currentState == GameState.Playing)
            {
                survivalTime += Time.deltaTime;
                OnSurvivalTimeChanged?.Invoke(survivalTime);
            }

            if (GameInput.IsPausePressed() && (currentState == GameState.Playing || currentState == GameState.Paused))
            {
                TogglePause();
            }
        }

        public void StartGame(int stage = 1)
        {
            currentStage = stage;
            score = 0;
            killCount = 0;
            survivalTime = 0f;
            isBossSpawned = false;
            isBossDefeated = false;
            activeBoss = null;

            bossScoreThreshold = (currentStage == 1) ? 1000 : 2500;

            SetState(GameState.Playing);

            // Bắt đầu sinh quái
            var spawner = UnityEngine.Object.FindAnyObjectByType<EnemySpawner>();
            if (spawner != null)
            {
                spawner.ResumeSpawning();
            }

            // Nếu là Màn 2, đổi màu sàn hoặc cấu hình sang Dung Nham
            if (currentStage == 2)
            {
                SetupStage2Environment();
            }

            OnScoreChanged?.Invoke(score);
            OnKillCountChanged?.Invoke(killCount);

            Debug.Log($"<color=green><b>[GameManager] BẮT ĐẦU MÀN {currentStage}! Mốc triệu hồi Boss: {bossScoreThreshold} điểm.</b></color>");
        }

        private void SetupStage2Environment()
        {
            var worldMgr = UnityEngine.Object.FindAnyObjectByType<InfiniteWorldManager>();
            if (worldMgr != null)
            {
                var lavaSprite = Resources.Load<Sprite>("Dungeon_Floor_Lava");
#if UNITY_EDITOR
                if (lavaSprite == null) lavaSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Dungeon_Floor_Lava.png");
#endif
                if (lavaSprite != null) worldMgr.SetFloorSprite(lavaSprite);
            }
        }

        public void AddScore(int amount)
        {
            score += amount;
            OnScoreChanged?.Invoke(score);

            // Kiểm tra mốc triệu hồi Boss Cuối
            if (!isBossSpawned && score >= bossScoreThreshold)
            {
                SpawnBoss();
            }
        }

        public void RegisterKill()
        {
            killCount++;
            OnKillCountChanged?.Invoke(killCount);
        }

        public void SpawnBoss()
        {
            if (isBossSpawned) return;
            isBossSpawned = true;

            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            Vector3 spawnPos = Vector3.zero;
            if (player != null)
            {
                Vector2 dir = UnityEngine.Random.insideUnitCircle.normalized;
                if (dir.sqrMagnitude < 0.1f) dir = Vector2.up;
                spawnPos = player.transform.position + (Vector3)(dir * 9.5f);
            }

            GameObject bossObj = null;
            if (bossPrefab != null)
            {
                bossObj = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                var loaded = Resources.Load<GameObject>("Boss_Goliath");
#if UNITY_EDITOR
                if (loaded == null) loaded = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Boss_Goliath.prefab");
#endif
                if (loaded != null) bossObj = Instantiate(loaded, spawnPos, Quaternion.identity);
            }

            if (bossObj != null)
            {
                activeBoss = bossObj.GetComponent<BossController>();
                OnBossSpawned?.Invoke(activeBoss);
            }

            Debug.Log($"<color=red><b>🚨 [BOSS SPAWNED] Boss Cuối Màn {currentStage} đã xuất hiện tại {spawnPos}!</b></color>");
        }

        public void RegisterActiveBoss(BossController boss)
        {
            activeBoss = boss;
            OnBossSpawned?.Invoke(activeBoss);
        }

        public void NotifyBossDefeated(Vector3 deathPos)
        {
            isBossDefeated = true;
            activeBoss = null;
            currentState = GameState.StageClear;

            OnBossDefeatedEvent?.Invoke();

            Debug.Log($"<color=yellow><b>🏆 [STAGE CLEAR] Boss Màn {currentStage} đã bị tiêu diệt! Cổng dịch chuyển đã mở!</b></color>");
        }

        public void CompleteStage()
        {
            // Lưu tiến độ màn chơi cao nhất đã vượt qua
            int highest = PlayerPrefs.GetInt("HighestStageCleared", 0);
            if (currentStage > highest)
            {
                PlayerPrefs.SetInt("HighestStageCleared", currentStage);
                PlayerPrefs.Save();
            }

            OnStageCompletedEvent?.Invoke(currentStage);

            // Mở modal chiến thắng màn & chuẩn bị sang màn 2
            SetState(GameState.Victory);
        }

        public void AdvanceToStage(int nextStage)
        {
            StartGame(nextStage);
        }

        public void SetState(GameState newState)
        {
            currentState = newState;

            switch (newState)
            {
                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;
                case GameState.MainMenu:
                case GameState.Paused:
                case GameState.GameOver:
                case GameState.Victory:
                    Time.timeScale = 0f;
                    break;
                case GameState.StageClear:
                    Time.timeScale = 1f; // Vẫn cho player di chuyển để bước vào cổng dịch chuyển
                    break;
            }

            OnGameStateChanged?.Invoke(newState);
        }

        public void TogglePause()
        {
            if (currentState == GameState.Playing) SetState(GameState.Paused);
            else if (currentState == GameState.Paused) SetState(GameState.Playing);
        }

        public void TriggerGameOver()
        {
            SetState(GameState.GameOver);
        }

        public void RestartCurrentScene()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
