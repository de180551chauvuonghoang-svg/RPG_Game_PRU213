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

            bool isPlayer = collision.CompareTag("Player") ||
                            collision.GetComponent<PlayerController>() != null ||
                            collision.GetComponentInParent<PlayerController>() != null;

            if (isPlayer)
            {
                hasTriggered = true;
                int nextStage = GameManager.Instance != null ? GameManager.Instance.CurrentStage + 1 : 2;
                Debug.Log($"<color=cyan><b>[PORTAL] Người chơi đã bước vào Cổng Dịch Chuyển! Khởi động chuyển dịch không gian sang Màn {nextStage}...</b></color>");
                
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AdvanceToStage(nextStage);
                }
                
                Destroy(gameObject, 0.2f);
            }
        }
    }
}
