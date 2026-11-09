using DungeonGame.Core;
using DungeonGame.Player;
using UnityEngine;

namespace DungeonGame.World
{
    /// <summary>
    /// Đồng Tiền Vàng (Coin Pickup):
    /// - Rơi ra khi tiêu diệt quái vật hoặc Boss.
    /// - Hiệu ứng lấp lánh và lực hút nam châm bay về phía Hiệp sĩ khi đến gần.
    /// - Nhặt được sẽ cộng trực tiếp vào túi tiền vàng trong GameManager để nâng cấp vũ khí tại Shop!
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class CoinPickup : MonoBehaviour
    {
        [Header("Giá trị Coin")]
        [SerializeField] private int coinValue = 1;

        [Header("Lực hút nam châm về người chơi")]
        [SerializeField] private float magnetRadius = 4.2f;
        [SerializeField] private float baseMagnetSpeed = 7.5f;
        [SerializeField] private float collectRadius = 0.65f;

        [Header("Thời gian tồn tại")]
        [SerializeField] private float lifetime = 50f;

        private Transform playerTarget;
        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private bool isCollected = false;
        private float spawnTime;
        private float currentSpeed;
        private static Sprite cachedCoinSprite;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            var col = GetComponent<CircleCollider2D>();
            if (col != null) col.isTrigger = true;
        }

        private void Start()
        {
            spawnTime = Time.time;
            currentSpeed = baseMagnetSpeed;
            FindPlayer();

            // Nếu chưa có Sprite thì tự động nạp hoặc tạo Sprite vàng kim
            if (spriteRenderer != null && spriteRenderer.sprite == null)
            {
                spriteRenderer.sprite = GetOrCreateCoinSprite();
            }
        }

        private void FindPlayer()
        {
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            if (player != null)
            {
                playerTarget = player.transform;
            }
        }

        private void Update()
        {
            if (isCollected) return;

            // Kiểm tra thời gian tồn tại
            if (Time.time - spawnTime > lifetime)
            {
                Destroy(gameObject);
                return;
            }

            // Nhấp nháy cảnh báo trước khi biến mất (5 giây cuối)
            if (Time.time - spawnTime > lifetime - 5f && spriteRenderer != null)
            {
                float alpha = (Mathf.Sin(Time.time * 12f) + 1f) * 0.4f + 0.2f;
                Color c = spriteRenderer.color;
                c.a = alpha;
                spriteRenderer.color = c;
            }

            if (playerTarget == null)
            {
                FindPlayer();
                if (playerTarget == null) return;
            }

            float dist = Vector2.Distance(transform.position, playerTarget.position);

            // 1. Nếu người chơi trong vùng hút nam châm: Lao nhanh về phía người chơi
            if (dist <= magnetRadius)
            {
                currentSpeed += Time.deltaTime * 14f; // Càng gần càng tăng tốc
                Vector3 targetPos = playerTarget.position;
                transform.position = Vector3.MoveTowards(transform.position, targetPos, currentSpeed * Time.deltaTime);

                if (dist <= collectRadius)
                {
                    Collect();
                }
            }
            else
            {
                // Hiệu ứng nhấp nhô nhẹ khi nằm trên mặt đất
                float bob = Mathf.Sin((Time.time + spawnTime) * 6f) * 0.002f;
                transform.position += new Vector3(0, bob, 0);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (isCollected) return;

            bool isPlayer = collision.CompareTag("Player") ||
                            collision.GetComponent<PlayerController>() != null ||
                            collision.GetComponentInParent<PlayerController>() != null;

            if (isPlayer)
            {
                Collect();
            }
        }

        public void Collect()
        {
            if (isCollected) return;
            isCollected = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddCoins(coinValue);
            }

            // Hiệu ứng thu nhỏ nhanh khi được nhặt
            Destroy(gameObject);
        }

        public void SetCoinValue(int val)
        {
            coinValue = Mathf.Max(1, val);
        }

        /// <summary>
        /// Hàm triệu hồi đồng tiền vàng tại vị trí với lực bung tự nhiên
        /// </summary>
        public static CoinPickup Spawn(Vector3 position, int value = 1)
        {
            GameObject coinObj = new GameObject("Coin_Pickup");
            coinObj.transform.position = position;
            coinObj.transform.localScale = Vector3.one * 1.1f;

            // Thêm Sprite Renderer
            var sr = coinObj.AddComponent<SpriteRenderer>();
            sr.sprite = GetOrCreateCoinSprite();
            sr.sortingOrder = 7; // Hiển thị trên nền sàn nhưng dưới Player

            // Thêm Collider
            var col = coinObj.AddComponent<CircleCollider2D>();
            col.radius = 0.35f;
            col.isTrigger = true;

            // Thêm Rigidbody để bung nhẹ ngẫu nhiên
            var rb = coinObj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            Vector2 randomBurst = Random.insideUnitCircle.normalized * Random.Range(1.2f, 2.8f);
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = randomBurst;
            rb.linearDamping = 4.5f;
#else
            rb.velocity = randomBurst;
            rb.drag = 4.5f;
#endif

            var pickup = coinObj.AddComponent<CoinPickup>();
            pickup.SetCoinValue(value);

            return pickup;
        }

        private static Sprite GetOrCreateCoinSprite()
        {
            if (cachedCoinSprite != null) return cachedCoinSprite;

            // 1. Thử tải từ Resources / Assets
            cachedCoinSprite = Resources.Load<Sprite>("Item_Coin");
#if UNITY_EDITOR
            if (cachedCoinSprite == null)
            {
                cachedCoinSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Item_Coin.png");
            }
#endif
            if (cachedCoinSprite != null) return cachedCoinSprite;

            // 2. Tạo Sprite đồng tiền vàng kim dự phòng nếu file chưa kịp sinh
            int size = 28;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color clear = new Color(0, 0, 0, 0);
            Color goldDark = new Color(0.6f, 0.4f, 0.05f);
            Color goldBase = new Color(1f, 0.8f, 0.1f);
            Color goldBright = new Color(1f, 0.95f, 0.4f);

            float radius = size / 2f - 1f;
            Vector2 center = new Vector2(size / 2f, size / 2f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    if (d > radius) tex.SetPixel(x, y, clear);
                    else if (d > radius - 1.8f) tex.SetPixel(x, y, goldDark);
                    else if (d > radius - 3.5f) tex.SetPixel(x, y, goldBright);
                    else tex.SetPixel(x, y, goldBase);
                }
            }

            // Dấu sao sáng ở giữa
            int cx = size / 2;
            int cy = size / 2;
            tex.SetPixel(cx, cy, Color.white);
            tex.SetPixel(cx + 1, cy, goldBright);
            tex.SetPixel(cx - 1, cy, goldBright);
            tex.SetPixel(cx, cy + 1, goldBright);
            tex.SetPixel(cx, cy - 1, goldBright);

            tex.Apply();
            cachedCoinSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32);
            return cachedCoinSprite;
        }
    }
}
