using DungeonGame.Core;
using DungeonGame.Player;
using UnityEngine;

namespace DungeonGame.World
{
    /// <summary>
    /// Cổng Dịch Chuyển Không Gian (Stage Portal):
    /// Xuất hiện ngay khi Boss Cuối bị tiêu diệt.
    /// Cho phép người chơi bước vào để qua Màn 2 (hoặc màn kế tiếp).
    /// </summary>
    public class StagePortal : MonoBehaviour
    {
        [Header("Hiệu ứng Portal")]
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float baseScale = 1.6f;

        private bool hasTriggered = false;

        private void Update()
        {
            // Hiệu ứng xoay tròn và nhấp nháy ma thuật
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * 0.12f;
            transform.localScale = Vector3.one * (baseScale * pulse);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (hasTriggered) return;

            if (collision.TryGetComponent<PlayerController>(out var player))
            {
                hasTriggered = true;
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.CompleteStage();
                }
            }
        }
    }
}
