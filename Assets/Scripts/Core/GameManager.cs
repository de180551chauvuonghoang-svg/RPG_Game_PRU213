using System;
using DungeonGame.Combat;
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

        [Header("Prefab Boss & Cổng Dịch Chuyển & Sàn Màn 2")]
        [SerializeField] private GameObject bossPrefab;
        [SerializeField] private GameObject bossStage2Prefab;
        [SerializeField] private GameObject portalPrefab;
        [SerializeField] private Sprite stage2LavaSprite;

        [Header("Thống kê trận đánh")]
        [SerializeField] private int score = 0;
        [SerializeField] private int killCount = 0;
        [SerializeField] private float survivalTime = 0f;
        [SerializeField] private int coins = 0;

        private BossController activeBoss;
        private bool isBossSpawned = false;
        private bool isBossDefeated = false;

        // Events
        public event Action<int> OnScoreChanged;
        public event Action<int> OnKillCountChanged;
        public event Action<int> OnCoinsChanged;
        public event Action<float> OnSurvivalTimeChanged;
        public event Action<GameState> OnGameStateChanged;
        public event Action<BossController> OnBossSpawned;
        public event Action OnBossDefeatedEvent;
        public event Action<int> OnStageCompletedEvent;

        public GameState CurrentState => currentState;
        public int Score => score;
        public int KillCount => killCount;
        public int Coins => coins;
        public float SurvivalTime => survivalTime;
        public int CurrentStage => currentStage;
        public bool IsBossDefeated => isBossDefeated;
        public BossController ActiveBoss => activeBoss;
        public bool IsBossActive => activeBoss != null && !activeBoss.IsDead;
        public int HighestStageCleared => PlayerPrefs.GetInt("HighestStageCleared", 0);
        public int BossScoreThreshold => bossScoreThreshold;

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
            if (stage == 1)
            {
                coins = 0;
            }
            isBossSpawned = false;
            isBossDefeated = false;
            activeBoss = null;

            switch (currentStage)
            {
                case 1: bossScoreThreshold = 1000; break;
                case 2: bossScoreThreshold = 2500; break;
                case 3: bossScoreThreshold = 4000; break;
                case 4: bossScoreThreshold = 6000; break;
                default: bossScoreThreshold = 1000; break;
            }

            SetState(GameState.Playing);

            // Bắt đầu sinh quái
            var spawner = UnityEngine.Object.FindAnyObjectByType<EnemySpawner>();
            if (spawner != null)
            {
                spawner.ResumeSpawning();
            }

            // Cấu hình môi trường tương ứng cho từng màn chơi
            SetupStageEnvironment(currentStage);

            OnScoreChanged?.Invoke(score);
            OnKillCountChanged?.Invoke(killCount);
            OnCoinsChanged?.Invoke(coins);

            Debug.Log($"<color=green><b>[GameManager] BẮT ĐẦU MÀN {currentStage}! Mốc triệu hồi Boss: {bossScoreThreshold} điểm.</b></color>");
        }

        private void SetupStageEnvironment(int stage)
        {
            var worldMgr = UnityEngine.Object.FindAnyObjectByType<InfiniteWorldManager>();
            if (worldMgr == null) return;

            Sprite targetSprite = null;
            Color bgColor = new Color(0.06f, 0.06f, 0.10f, 1f);

            switch (stage)
            {
                case 1:
                    targetSprite = Resources.Load<Sprite>("Dungeon_Floor");
#if UNITY_EDITOR
                    if (targetSprite == null) targetSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Dungeon_Floor.png");
#endif
                    bgColor = new Color(0.06f, 0.06f, 0.10f, 1f);
                    break;

                case 2:
                    targetSprite = stage2LavaSprite;
                    if (targetSprite == null) targetSprite = Resources.Load<Sprite>("Dungeon_Floor_Lava");
#if UNITY_EDITOR
                    if (targetSprite == null) targetSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Dungeon_Floor_Lava.png");
#endif
                    bgColor = new Color(0.16f, 0.04f, 0.04f, 1f);
                    break;

                case 3:
                    targetSprite = Resources.Load<Sprite>("Dungeon_Floor_Ice");
#if UNITY_EDITOR
                    if (targetSprite == null) targetSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Dungeon_Floor_Ice.png");
                    if (targetSprite == null) targetSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Dungeon_Floor.png");
#endif
                    bgColor = new Color(0.04f, 0.08f, 0.18f, 1f);
                    break;

                case 4:
                    targetSprite = Resources.Load<Sprite>("Dungeon_Floor_Thunder");
#if UNITY_EDITOR
                    if (targetSprite == null) targetSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Dungeon_Floor_Thunder.png");
                    if (targetSprite == null) targetSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Dungeon_Floor_Lava.png");
#endif
                    bgColor = new Color(0.12f, 0.04f, 0.20f, 1f);
                    break;
            }

            if (targetSprite != null)
            {
                worldMgr.SwitchToStage(targetSprite, bgColor);
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

        public void AddCoins(int amount)
        {
            if (amount <= 0) return;
            coins += amount;
            OnCoinsChanged?.Invoke(coins);
            Debug.Log($"<color=yellow><b>★ [NHẶT COIN +{amount}] Bạn nhận được {amount} tiền vàng! Hiện có: {coins} Coin.</b></color>");
        }

        public bool SpendCoins(int amount)
        {
            if (amount <= 0) return true;
            if (coins < amount) return false;

            coins -= amount;
            OnCoinsChanged?.Invoke(coins);
            Debug.Log($"<color=yellow><b>★ [TIÊU COIN -{amount}] Đã thanh toán {amount} tiền vàng! Còn lại: {coins} Coin.</b></color>");
            return true;
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
            GameObject prefabToSpawn = (currentStage >= 2 && bossStage2Prefab != null) ? bossStage2Prefab : bossPrefab;

            if (prefabToSpawn != null)
            {
                bossObj = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
            }
            else
            {
                string targetPrefabName = (currentStage >= 2) ? "Boss_InfernalIfrit" : "Boss_Goliath";
                var loaded = Resources.Load<GameObject>(targetPrefabName);
#if UNITY_EDITOR
                if (loaded == null) loaded = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/{targetPrefabName}.prefab");
#endif
                if (loaded != null) bossObj = Instantiate(loaded, spawnPos, Quaternion.identity);
            }

            if (bossObj != null)
            {
                activeBoss = bossObj.GetComponent<BossController>();

                // Tùy biến chỉ số và màu sắc theo từng màn chơi
                if (currentStage == 3)
                {
                    var hp = bossObj.GetComponent<HealthComponent>();
                    if (hp != null) hp.SetMaxHealth(4000f, true);
                    var sr = bossObj.GetComponentInChildren<SpriteRenderer>();
                    if (sr != null) sr.color = new Color(0.4f, 0.95f, 1f, 1f); // Băng Tuyết Uy Dũng
                }
                else if (currentStage == 4)
                {
                    var hp = bossObj.GetComponent<HealthComponent>();
                    if (hp != null) hp.SetMaxHealth(6000f, true);
                    var sr = bossObj.GetComponentInChildren<SpriteRenderer>();
                    if (sr != null) sr.color = new Color(1f, 0.85f, 0.25f, 1f); // Kim Sắc Thần Sấm
                }

                OnBossSpawned?.Invoke(activeBoss);
            }

            string bossTitle = currentStage switch
            {
                1 => "Chúa Tể Goliath (Màn 1)",
                2 => "Chúa Tể Dung Nham Ifrit (Màn 2)",
                3 => "Vu Yêu Băng Frost Lich (Màn 3)",
                4 => "Thần Sấm Sét Zeus Titan (Màn 4)",
                _ => $"Trùm Cuối Màn {currentStage}"
            };
            Debug.Log($"<color=red><b>🚨 [BOSS SPAWNED] {bossTitle} đã xuất hiện tại {spawnPos}!</b></color>");
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
            currentStage = nextStage;
            switch (currentStage)
            {
                case 2: bossScoreThreshold = score + 2500; break;
                case 3: bossScoreThreshold = score + 4000; break;
                case 4: bossScoreThreshold = score + 6000; break;
                default: bossScoreThreshold = score + 1000; break;
            }
            isBossSpawned = false;
            isBossDefeated = false;
            activeBoss = null;

            // 1. Lưu tiến độ màn chơi đã vượt qua
            int highest = PlayerPrefs.GetInt("HighestStageCleared", 0);
            if (currentStage - 1 > highest)
            {
                PlayerPrefs.SetInt("HighestStageCleared", currentStage - 1);
                PlayerPrefs.Save();
            }

            // 2. Chuyển đổi môi trường sàn và màu nền
            SetupStageEnvironment(currentStage);

            // 3. Rung lắc camera tạo hiệu ứng dịch chuyển không gian bùng nổ
            var camFollow = Camera.main != null ? Camera.main.GetComponent<DungeonGame.Cameras.CameraFollow2D>() : null;
            if (camFollow != null)
            {
                camFollow.TriggerShake(0.8f, 0.5f);
            }

            // 4. Hồi 100% máu cho Hiệp sĩ
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            if (player != null)
            {
                var hp = player.GetComponent<HealthComponent>();
                if (hp != null) hp.Heal(100f);
            }

            // 5. Kích hoạt hiệu ứng Chuyển Màn & Banner trong UI
            if (DungeonGame.UI.GameUIManager.Instance != null)
            {
                DungeonGame.UI.GameUIManager.Instance.TriggerStageTransitionEffect(currentStage);
            }

            // 6. Spawner tiếp tục sinh quái
            var spawner = UnityEngine.Object.FindAnyObjectByType<EnemySpawner>();
            if (spawner != null)
            {
                spawner.ResumeSpawning();
            }

            SetState(GameState.Playing);

            Debug.Log($"<color=#FF6600><b>[GameManager] CHUYỂN QUA MÀN {currentStage} THÀNH CÔNG!</b></color>");
        }

        public void TransitionToStage2()
        {
            AdvanceToStage(2);
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
