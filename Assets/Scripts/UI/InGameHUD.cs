using DungeonGame.Combat;
using DungeonGame.Core;
using DungeonGame.Player;
using UnityEngine;

namespace DungeonGame.UI
{
    /// <summary>
    /// Script bổ trợ logic. Đã chuyển toàn bộ việc hiển thị sang Canvas UI (GameUIManager)
    /// để loại bỏ 100% hiện tượng trùng lặp chữ trên màn hình.
    /// </summary>
    public class InGameHUD : MonoBehaviour
    {
        // OnGUI đã được vô hiệu hóa để Canvas UI hiển thị sắc nét độc quyền, không bị lặp text.
    }
}
