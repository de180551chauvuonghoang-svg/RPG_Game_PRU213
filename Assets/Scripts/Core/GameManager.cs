using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DungeonGame.Core
{
    public enum GameState
    {
        Playing,
        Paused,
        GameOver,
        Victory
    }

    /// <summary>
    /// Quản lý trạng thái chung của Game (Game Loop, Điểm số, Kills, Thời gian sinh tồn).
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Trạng thái hiện tại")]
        [SerializeField] private GameState currentState = GameState.Playing;

        [Header("Thông số thống kê")]
        [SerializeField] private int score = 0;
        [SerializeField] private int killCount = 0;
        [SerializeField] private float survivalTime = 0f;

        // Events cho UI lắng nghe (Observer Pattern)
        public event Action<int> OnScoreChanged;
        public event Action<int> OnKillCountChanged;
        public event Action<float> OnSurvivalTimeChanged;
        public event Action<GameState> OnGameStateChanged;

        public GameState CurrentState => currentState;
        public int Score => score;
        public int KillCount => killCount;
        public float SurvivalTime => survivalTime;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (currentState == GameState.Playing)
            {
                survivalTime += Time.deltaTime;
                OnSurvivalTimeChanged?.Invoke(survivalTime);
            }

            // Phím tắt Pause nhanh (Escape / P) qua GameInput
            if (GameInput.IsPausePressed())
            {
                TogglePause();
            }
        }

        public void AddScore(int amount)
        {
            score += amount;
            OnScoreChanged?.Invoke(score);
        }

        public void RegisterKill()
        {
            killCount++;
            OnKillCountChanged?.Invoke(killCount);
        }

        public void SetState(GameState newState)
        {
            currentState = newState;

            switch (newState)
            {
                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                case GameState.GameOver:
                case GameState.Victory:
                    Time.timeScale = 0f;
                    break;
            }

            OnGameStateChanged?.Invoke(newState);
        }

        public void TogglePause()
        {
            if (currentState == GameState.Playing)
            {
                SetState(GameState.Paused);
            }
            else if (currentState == GameState.Paused)
            {
                SetState(GameState.Playing);
            }
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
