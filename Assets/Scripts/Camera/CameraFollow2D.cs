using DungeonGame.Core;
using UnityEngine;

namespace DungeonGame.Cameras
{
    /// <summary>
    /// Camera bám theo Player mượt mà (Smooth Damping) và hướng nhẹ về con trỏ chuột.
    /// An toàn 100% trước MissingReferenceException khi chuyển cảnh hoặc xóa đối tượng.
    /// </summary>
    public class CameraFollow2D : MonoBehaviour
    {
        [Header("Mục tiêu theo dõi")]
        [SerializeField] private Transform target;

        [Header("Độ trễ mượt mà")]
        [SerializeField] private float smoothTime = 0.2f;
        [SerializeField] private Vector3 offset = new Vector3(0, 0, -10f);

        [Header("Camera Look-Ahead (Nhìn về phía chuột)")]
        [SerializeField] private bool enableLookAhead = true;
        [SerializeField] private float lookAheadWeight = 0.15f;
        [SerializeField] private float maxLookAheadDistance = 2.5f;

        [Header("Giới hạn khung hình Map (Tùy chọn)")]
        [SerializeField] private bool useBounds = false;
        [SerializeField] private Vector2 minBounds;
        [SerializeField] private Vector2 maxBounds;

        private Vector3 currentVelocity = Vector3.zero;
        private Camera cam;

        private void Awake()
        {
            cam = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            try
            {
                if (target == null)
                {
                    var p = UnityEngine.Object.FindAnyObjectByType<DungeonGame.Player.PlayerController>();
                    if (p != null) target = p.transform;
                    else return;
                }

                Vector3 targetPosition = target.position + offset;

                if (enableLookAhead)
                {
                    Vector3 mouseWorldPos = GameInput.GetMouseWorldPosition(cam);
                    Vector3 toMouse = mouseWorldPos - target.position;
                    toMouse.z = 0;

                    Vector3 lookAhead = Vector3.ClampMagnitude(toMouse * lookAheadWeight, maxLookAheadDistance);
                    targetPosition += lookAhead;
                }

                if (useBounds)
                {
                    targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
                    targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
                }

                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
            }
            catch (System.Exception)
            {
                target = null;
            }
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}
