# 🎮 HƯỚNG DẪN THIẾT LẬP VÀ TEST GIAI ĐOẠN 1 TRONG UNITY (5 PHÚT)

Toàn bộ các Script cho **Giai đoạn 1** đã được lập trình sẵn và chuẩn hóa theo kiến trúc sạch (Clean Architecture & SOLID).
Bạn chỉ cần làm theo các bước dưới đây để dựng cảnh chơi thử ngay lập tức:

---

## 🛠 BƯỚC 1: MỞ DỰ ÁN TRONG UNITY
1. Mở **Unity Hub** -> Chọn **Add project from disk** -> Trỏ tới thư mục `c:\Users\RinHeo\Desktop\gameproject`.
   *(Nếu bạn vừa tạo mới project 2D từ Unity Hub, chỉ cần copy toàn bộ thư mục `Assets/Scripts` vào project của bạn).*
2. Mở Scene mặc định (`SampleScene`).

---

## 🧍 BƯỚC 2: TẠO NHÂN VẬT CHÍNH (PLAYER)
1. Trong Hierarchy: Chuột phải -> **2D Object** -> **Sprites** -> **Square** (hoặc Capsule), đổi tên thành **`Player`**.
2. Tại cửa sổ **Inspector** của Player:
   - **Rigidbody 2D**:
     - *Gravity Scale*: đổi về **`0`** (vì đây là game góc nhìn Top-Down).
     - *Constraints*: tick chọn **`Freeze Rotation Z`**.
   - **Circle Collider 2D** (hoặc Box Collider 2D):
     - Chỉnh kích thước vừa với hình nhân vật.
   - Bấm **Add Component** và thêm các Script:
     - `PlayerController`
     - `HealthComponent`
     - `HitFlash`
     - `PlayerCombat`
     - `Trail Renderer` (tùy chọn: chỉnh Time = 0.15, Width nhỏ dần để tạo vệt mờ khi Dash).
3. **Tạo điểm cầm vũ khí và nòng súng:**
   - Chuột phải vào `Player` -> **Create Empty** -> đặt tên là `WeaponPivot`.
   - Chuột phải vào `WeaponPivot` -> **2D Object -> Sprite -> Square** -> đổi tên thành `GunVisual` (kéo dài thành hình chữ nhật nhỏ xíu làm khẩu súng, đặt nằm ngang lệch sang phải x = 0.5).
   - Chuột phải vào `GunVisual` -> **Create Empty** -> đặt tên là `FirePoint` (kéo ra ngay đầu mũi súng, x = 0.5).
   - Kéo `WeaponPivot` và `FirePoint` vào các ô tương ứng trên component `PlayerCombat` của Player.

---

## 🔫 BƯỚC 3: TẠO VIÊN ĐẠN (BULLET PREFAB)
1. Trong Hierarchy: Chuột phải -> **2D Object -> Sprites -> Circle** -> đổi tên thành `Bullet`.
2. Chỉnh Scale nhỏ lại (ví dụ: `X: 0.3, Y: 0.3`).
3. Đổi màu Sprite sang màu vàng cam rực rỡ.
4. Thêm component:
   - **Rigidbody 2D**: *Gravity Scale = 0*, *Collision Detection = Continuous*.
   - **Circle Collider 2D**: tick chọn **`Is Trigger = True`**.
   - `Projectile`:
     - *Target Layers*: chọn layer của Kẻ địch (hoặc `Default`).
5. Kéo `Bullet` từ Hierarchy xuống cửa sổ **Project** (thư mục `Assets/Prefabs/`) để biến thành **Prefab**. Sau đó xóa `Bullet` trên Hierarchy đi.

---

## 📦 BƯỚC 4: TẠO OBJECT POOLER VÀ DATA VŨ KHÍ
1. **Tạo Object Pooler:**
   - Trong Hierarchy -> Chuột phải -> **Create Empty** -> đổi tên thành `[Managers]`.
   - Add component: `ObjectPooler` và `GameManager`.
   - Trên `ObjectPooler`:
     - Nhấn `+` thêm 1 Pool.
     - *Tag*: đặt là **`PlayerBullet`**.
     - *Prefab*: kéo Prefab `Bullet` ở Bước 3 vào.
     - *Size*: đặt `30`.
2. **Tạo Dữ liệu Vũ khí (ScriptableObject):**
   - Trong cửa sổ Project: Chuột phải -> **Create -> DungeonGame -> Weapon Data** -> đặt tên là `PistolData`.
   - Click vào `PistolData`, chỉnh:
     - *Damage*: 25
     - *Projectile Speed*: 15
     - *Fire Rate*: 0.2
     - *Projectile Pool Tag*: **`PlayerBullet`** (phải trùng tên với tag trong ObjectPooler).
   - Kéo file `PistolData` vào ô **Current Weapon** của `PlayerCombat` trên Player.

---

## 🎯 BƯỚC 5: TẠO KẺ ĐỊCH THỬ NGHIỆM (DUMMY ENEMY)
Để thử nghiệm sát thương, knockback và nhấp nháy đỏ:
1. Chuột phải Hierarchy -> **2D Object -> Sprites -> Square** -> đổi tên thành `Enemy_Dummy`.
2. Đổi màu sang **Đỏ**.
3. Thêm các component:
   - **Rigidbody 2D**: *Gravity Scale = 0*, *Linear Drag = 3*, *Freeze Rotation Z = Checked*.
   - **Box Collider 2D**.
   - `HealthComponent` (Max Health = 100).
   - `HitFlash`.
   - `DamageOnContact` (Damage = 10, Target Layer = chọn Layer của Player).

---

## 🎥 BƯỚC 6: GẮN CAMERA THEO DÕI
1. Chọn `Main Camera` trong Hierarchy.
2. Add component `CameraFollow2D`.
3. Kéo GameObject `Player` vào ô **Target**.

---

## ▶️ BẤM PLAY VÀ TRẢI NGHIỆM!
- **W, A, S, D**: Di chuyển 8 hướng mượt mà, nhân vật tự động quay mặt theo chuột.
- **Space** hoặc **Chuột phải**: Lướt (Dash) né đòn tức thì kèm miễn nhiễm sát thương.
- **Chuột trái**: Xoay nòng súng 360 độ và bắn đạn liên thanh với độ giật nhẹ.
- Bắn vào `Enemy_Dummy`: Kẻ địch bị nảy lùi lại (Knockback) và nhấp nháy đỏ khi nhận sát thương.
