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
    /// Quản lý trạng thái chung của Game (Game Loop, Điểm số, Pause/Resume, Game Over).
    /// Tương thích cả Unity 6 New Input System lẫn Legacy Input.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Trạng thái hiện tại")]
        [SerializeField] private GameState currentState = GameState.Playing;

        [Header("Thông số thống kê")]
        [SerializeField] private int killCount = 0;
        [SerializeField] private float survivalTime = 0f;

        public event Action<GameState> OnGameStateChanged;
        public event Action<int> OnKillCountChanged;

        public GameState CurrentState => currentState;
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
            }

            // Phím tắt Pause nhanh (Escape / P) qua GameInput
            if (GameInput.IsPausePressed())
            {
                TogglePause();
            }
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

        public void RegisterKill()
        {
            killCount++;
            OnKillCountChanged?.Invoke(killCount);
        }

        public void RestartCurrentScene()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
