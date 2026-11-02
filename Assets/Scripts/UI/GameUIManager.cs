using DungeonGame.Combat;
using DungeonGame.Core;
using DungeonGame.Player;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonGame.UI
{
    /// <summary>
    /// Quản lý toàn bộ giao diện HUD trên màn hình:
    /// - Điểm số (Score)
    /// - Số quái tiêu diệt (Kills)
    /// - Thời gian sống sót (Survival Time)
    /// - Thanh máu của Hiệp Sĩ (Health Bar)
    /// - Bảng Game Over khi tử trận
    /// </summary>
    public class GameUIManager : MonoBehaviour
    {
        public static GameUIManager Instance { get; private set; }

        [Header("Tham chiếu Text HUD")]
        [SerializeField] private Text scoreText;
        [SerializeField] private Text killsText;
        [SerializeField] private Text timeText;

        [Header("Thanh Máu Hiệp Sĩ")]
        [SerializeField] private Image healthBarFill;
        [SerializeField] private Text healthText;

        [Header("Bảng Game Over")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Text finalScoreText;
        [SerializeField] private Button restartButton;

        private HealthComponent playerHealth;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartClicked);
            }

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
        }

        private void Start()
        {
            // Kết nối với GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged += UpdateScoreUI;
                GameManager.Instance.OnKillCountChanged += UpdateKillsUI;
                GameManager.Instance.OnSurvivalTimeChanged += UpdateTimeUI;
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;

                UpdateScoreUI(GameManager.Instance.Score);
                UpdateKillsUI(GameManager.Instance.KillCount);
                UpdateTimeUI(GameManager.Instance.SurvivalTime);
            }

            // Kết nối với HealthComponent của Player
            var player = Object.FindAnyObjectByType<PlayerController>();
            if (player != null)
            {
                playerHealth = player.GetComponent<HealthComponent>();
                if (playerHealth != null)
                {
                    playerHealth.OnHealthChanged += UpdateHealthUI;
                    playerHealth.OnDeath += HandlePlayerDeath;
                    UpdateHealthUI(playerHealth.CurrentHealth, playerHealth.MaxHealth);
                }
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged -= UpdateScoreUI;
                GameManager.Instance.OnKillCountChanged -= UpdateKillsUI;
                GameManager.Instance.OnSurvivalTimeChanged -= UpdateTimeUI;
                GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
            }

            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged -= UpdateHealthUI;
                playerHealth.OnDeath -= HandlePlayerDeath;
            }
        }

        public void UpdateScoreUI(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"ĐIỂM: {score:N0}";
            }
        }

        public void UpdateKillsUI(int kills)
        {
            if (killsText != null)
            {
                killsText.text = $"TIÊU DIỆT: {kills}";
            }
        }

        public void UpdateTimeUI(float seconds)
        {
            if (timeText != null)
            {
                int min = Mathf.FloorToInt(seconds / 60f);
                int sec = Mathf.FloorToInt(seconds % 60f);
                timeText.text = $"THỜI GIAN: {min:00}:{sec:00}";
            }
        }

        public void UpdateHealthUI(float current, float max)
        {
            if (healthBarFill != null)
            {
                healthBarFill.fillAmount = Mathf.Clamp01(current / max);
            }

            if (healthText != null)
            {
                healthText.text = $"HP: {Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
            }
        }

        private void HandlePlayerDeath()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver();
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state == GameState.GameOver)
            {
                if (gameOverPanel != null)
                {
                    gameOverPanel.SetActive(true);
                    if (finalScoreText != null && GameManager.Instance != null)
                    {
                        finalScoreText.text = $"TỔNG ĐIỂM: {GameManager.Instance.Score:N0}\nQUÁI DIỆT ĐƯỢC: {GameManager.Instance.KillCount}";
                    }
                }
            }
        }

        public void OnRestartClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartCurrentScene();
            }
        }
    }
}
