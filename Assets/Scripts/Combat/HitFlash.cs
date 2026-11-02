using System.Collections;
using UnityEngine;

namespace DungeonGame.Combat
{
    /// <summary>
    /// Hiệu ứng nhấp nháy đỏ/trắng trên SpriteRenderer khi đối tượng nhận sát thương (Game Feel).
    /// Tự động lắng nghe event OnDamageTaken từ HealthComponent.
    /// </summary>
    [RequireComponent(typeof(HealthComponent))]
    public class HitFlash : MonoBehaviour
    {
        [Header("Cài đặt Flash")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color flashColor = Color.red;
        [SerializeField] private float flashDuration = 0.1f;

        private HealthComponent health;
        private Color originalColor;
        private Coroutine flashCoroutine;

        private void Awake()
        {
            health = GetComponent<HealthComponent>();
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.OnDamageTaken += TriggerFlash;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.OnDamageTaken -= TriggerFlash;
            }
        }

        private void TriggerFlash(float damage)
        {
            if (spriteRenderer == null) return;

            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
            flashCoroutine = null;
        }
    }
}
