using System.Collections.Generic;
using DungeonGame.Player;
using UnityEngine;

namespace DungeonGame.World
{
    /// <summary>
    /// Hệ thống Bản Đồ Thế Giới Mở Vô Tận (Chunk-based Infinite World Generation).
    /// Người chơi đi tới đâu, hầm ngục và môi trường tự động sinh ra tới đó.
    /// Tự động thu hồi các vùng đất ở quá xa để tối ưu hóa bộ nhớ RAM và duy trì FPS cao.
    /// </summary>
    public class InfiniteWorldManager : MonoBehaviour
    {
        [Header("Cài đặt Chunk")]
        [Tooltip("Kích thước mỗi ô chunk (mét)")]
        [SerializeField] private int chunkSize = 16;

        [Tooltip("Bán kính chunk hiển thị xung quanh người chơi (2 = 5x5 chunks)")]
        [SerializeField] private int viewDistanceChunks = 2;

        [Header("Sprite Sàn & Trang Trí")]
        [SerializeField] private Sprite floorSprite;

        private Transform playerTransform;
        private Vector2Int currentChunkCoord;
        private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();

        private void Start()
        {
            FindPlayer();
            UpdateVisibleChunks();
        }

        private void Update()
        {
            if (playerTransform == null)
            {
                FindPlayer();
                return;
            }

            Vector2Int newChunkCoord = GetChunkCoordFromPosition(playerTransform.position);
            if (newChunkCoord != currentChunkCoord)
            {
                currentChunkCoord = newChunkCoord;
                UpdateVisibleChunks();
            }
        }

        private void FindPlayer()
        {
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            if (player != null)
            {
                playerTransform = player.transform;
                currentChunkCoord = GetChunkCoordFromPosition(playerTransform.position);
            }
        }

        private Vector2Int GetChunkCoordFromPosition(Vector3 pos)
        {
            int cx = Mathf.FloorToInt(pos.x / chunkSize);
            int cy = Mathf.FloorToInt(pos.y / chunkSize);
            return new Vector2Int(cx, cy);
        }

        private void UpdateVisibleChunks()
        {
            HashSet<Vector2Int> chunksToKeep = new HashSet<Vector2Int>();

            // 1. Sinh các chunk mới trong tầm nhìn người chơi
            for (int xOffset = -viewDistanceChunks; xOffset <= viewDistanceChunks; xOffset++)
            {
                for (int yOffset = -viewDistanceChunks; yOffset <= viewDistanceChunks; yOffset++)
                {
                    Vector2Int chunkCoord = new Vector2Int(currentChunkCoord.x + xOffset, currentChunkCoord.y + yOffset);
                    chunksToKeep.Add(chunkCoord);

                    if (!activeChunks.ContainsKey(chunkCoord))
                    {
                        GameObject newChunk = CreateChunk(chunkCoord);
                        activeChunks.Add(chunkCoord, newChunk);
                    }
                }
            }

            // 2. Thu dọn các chunk ở quá xa khỏi người chơi để tối ưu bộ nhớ
            List<Vector2Int> chunksToRemove = new List<Vector2Int>();
            foreach (var kvp in activeChunks)
            {
                if (!chunksToKeep.Contains(kvp.Key))
                {
                    chunksToRemove.Add(kvp.Key);
                }
            }

            foreach (var coord in chunksToRemove)
            {
                Destroy(activeChunks[coord]);
                activeChunks.Remove(coord);
            }
        }

        private GameObject CreateChunk(Vector2Int coord)
        {
            GameObject chunkObj = new GameObject($"Chunk_{coord.x}_{coord.y}");
            chunkObj.transform.SetParent(this.transform);

            Vector3 chunkWorldPos = new Vector3(coord.x * chunkSize, coord.y * chunkSize, 0f);
            chunkObj.transform.position = chunkWorldPos;

            int tileSize = 2; // Mỗi tile sàn kích thước 2x2 đơn vị
            int tilesPerSide = chunkSize / tileSize;

            for (int x = 0; x < tilesPerSide; x++)
            {
                for (int y = 0; y < tilesPerSide; y++)
                {
                    GameObject tile = new GameObject($"Tile_{x}_{y}");
                    tile.transform.SetParent(chunkObj.transform, false);
                    tile.transform.localPosition = new Vector3(x * tileSize, y * tileSize, 0f);
                    float spriteWidth = (floorSprite != null && floorSprite.rect.width > 0) ? (floorSprite.rect.width / floorSprite.pixelsPerUnit) : 2f;
                    float scaleFactor = tileSize / spriteWidth;
                    tile.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);

                    SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
                    sr.sprite = floorSprite;
                    sr.sortingOrder = -10;
                }
            }

            return chunkObj;
        }

        public void SetFloorSprite(Sprite sprite)
        {
            floorSprite = sprite;
        }
    }
}
