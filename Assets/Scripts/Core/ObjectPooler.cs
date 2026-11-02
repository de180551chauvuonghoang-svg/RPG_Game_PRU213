using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonGame.Core
{
    /// <summary>
    /// Design Pattern: Object Pooling (Singleton).
    /// Tối ưu hóa bộ nhớ, không tạo/hủy đối tượng liên tục để loại bỏ lag giật do Garbage Collector.
    /// </summary>
    public class ObjectPooler : MonoBehaviour
    {
        public static ObjectPooler Instance { get; private set; }

        [Serializable]
        public class Pool
        {
            public string tag;
            public GameObject prefab;
            public int size = 20;
        }

        [Header("Danh sách các Pool")]
        [SerializeField] private List<Pool> pools = new List<Pool>();

        private Dictionary<string, Queue<GameObject>> poolDictionary;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializePools();
        }

        private void InitializePools()
        {
            poolDictionary = new Dictionary<string, Queue<GameObject>>();

            foreach (var pool in pools)
            {
                Queue<GameObject> objectPool = new Queue<GameObject>();

                GameObject poolParent = new GameObject($"[Pool] {pool.tag}");
                poolParent.transform.SetParent(this.transform);

                for (int i = 0; i < pool.size; i++)
                {
                    GameObject obj = Instantiate(pool.prefab, poolParent.transform);
                    obj.SetActive(false);
                    objectPool.Enqueue(obj);
                }

                poolDictionary.Add(pool.tag, objectPool);
            }
        }

        /// <summary>
        /// Lấy đối tượng từ Pool theo tag và đặt vào vị trí, góc xoay chỉ định.
        /// </summary>
        public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"[ObjectPooler] Không tìm thấy pool có tag: {tag}");
                return null;
            }

            Queue<GameObject> objectQueue = poolDictionary[tag];

            if (objectQueue.Count == 0)
            {
                Debug.LogWarning($"[ObjectPooler] Hết đối tượng trong pool {tag}");
                return null;
            }

            GameObject objectToSpawn = objectQueue.Dequeue();

            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;
            objectToSpawn.SetActive(true);

            // Đưa lại vào cuối hàng đợi để xoay vòng
            objectQueue.Enqueue(objectToSpawn);

            return objectToSpawn;
        }

        /// <summary>
        /// Thu hồi đối tượng về lại Pool.
        /// </summary>
        public void ReturnToPool(GameObject obj)
        {
            obj.SetActive(false);
        }
    }
}
