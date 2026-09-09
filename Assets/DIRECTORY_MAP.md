# 🗺️ Space Defender - Bản Đồ Cấu Trúc Dự Án (Directory Map)

> **Tài liệu hướng dẫn tra cứu nhanh cho các lập trình viên khi tiếp tục phát triển dự án.**

---

## 📁 1. Cấu Trúc Tổng Quan (Assets Directory Tree)

```text
Assets/
├── Materials/                          # Toàn bộ Material hạt và hiệu ứng
│   ├── ExplosionParticleMat.mat        # Material hạt nổ URP Additive
│   └── ThrusterParticleMat.mat         # Material ngọn lửa phản lực Neon
├── Prefabs/                            # Toàn bộ Prefab trong game (24 Prefabs)
│   ├── Boss_UFO.prefab                 # Prefab UFO Mothership Boss
│   ├── Enemy_Blue / Green / Red.prefab # Các loại tàu địch
│   ├── EnemyLaser.prefab / Laser.prefab# Tia đạn laser của địch & người chơi
│   ├── ExplosionVFX.prefab             # Hiệu ứng nổ hạt phân mảnh
│   ├── FloatingScore.prefab            # Chữ điểm số bay (+100, COMBO x3)
│   ├── HomingMissile.prefab            # Tên lửa tầm nhiệt tự tìm mục tiêu
│   ├── Meteor_Big / Med / Grey.prefab  # Thiên thạch thông thường
│   ├── Player.prefab                   # Phi thuyền người chơi đầy đủ component
│   ├── PowerUp_Health / Shield / ...   # Các vật phẩm bổ trợ
│   ├── Shockwave.prefab                # Sóng xung kích EMP Bomb
│   ├── SplittingMeteor_Big/Med/Small   # Thiên thạch phân tách khi bắn vỡ
│   └── StarPickup.prefab               # Ngôi sao tiền tệ nâng cấp
├── Scenes/
│   └── SampleScene.unity               # Scene gameplay chính của game
├── Scripts/                            # Toàn bộ mã nguồn C# (39 files, 7 modules)
│   ├── Combat/                         # Cơ chế chiến đấu, đạn, va chạm & hiệu ứng nổ
│   ├── Core/                           # Quản lý cốt lõi, Game Loop, Audio, Save/Load
│   ├── Editor/                         # Tool tạo scene, build game tự động
│   ├── Enemies/                        # AI quái thường, UFO Boss & Spawner
│   ├── Environment/                    # Nền sao Parallax, vật phẩm rơi
│   ├── Player/                         # Điều khiển phi thuyền, Gamepad, vũ khí
│   └── UI/                             # Canvas, HUD, Modal Hangar, Cài đặt, Perk
├── Settings/
│   └── DefaultVolumeProfile.asset      # Cấu hình Post-Processing URP (Bloom, Vignette)
├── Shaders/
│   └── SpriteHitFlash.shader           # Shader chớp trắng khi nhận sát thương
└── kenney_space-shooter-remastered/    # Bộ Sprite & Âm thanh gốc CC0 của Kenney
```

---

## 🧭 2. Bảng Tra Cứu Nhanh (Quick Navigation Table)

Khi bạn muốn **thêm tính năng** hoặc **sửa lỗi**, hãy mở đúng file theo bảng dưới đây:

| Mục tiêu phát triển / Sửa đổi | File cần mở | Thư mục |
| :--- | :--- | :--- |
| **Thêm / Sửa tàu phi thuyền người chơi** | `PlayerController.cs` & `PlayerController.Movement.cs` | `Assets/Scripts/Player/` |
| **Chỉnh tốc độ bay, Dash né đòn, Gamepad** | `PlayerController.Movement.cs` | `Assets/Scripts/Player/` |
| **Chỉnh vũ khí, đạn laser, Fever mode, Graze** | `PlayerController.Weapons.cs` | `Assets/Scripts/Player/` |
| **Chỉnh lượng máu, bom EMP, nhận sát thương** | `PlayerController.Combat.cs` | `Assets/Scripts/Player/` |
| **Chỉnh Drone hộ tống bay quanh tàu** | `CompanionDrone.cs` | `Assets/Scripts/Player/` |
| **Chỉnh AI Boss, đạn xoắn ốc Danmaku, Laser quét** | `BossController.cs` | `Assets/Scripts/Enemies/` |
| **Chỉnh quái thường, đường bay địch, máu địch** | `Enemy.cs` | `Assets/Scripts/Enemies/` |
| **Chỉnh tần suất xuất hiện quái theo Wave** | `EnemySpawner.cs` | `Assets/Scripts/Enemies/` |
| **Chỉnh thiên thạch vỡ vụn** | `SplittingMeteor.cs` | `Assets/Scripts/Enemies/` |
| **Chỉnh nhạc nền Synthwave, SFX, Beat-drop** | `AudioManager.cs` | `Assets/Scripts/Core/` |
| **Chỉnh Bloom Neon, Vignette tim đập, Glitch EMP**| `PostProcessingManager.cs` | `Assets/Scripts/Core/` |
| **Chỉnh Rung màn hình (Camera Shake)** | `CameraShake.cs` | `Assets/Scripts/Core/` |
| **Chỉnh Vòng lặp Game (Start, Pause, Game Over)** | `GameManager.cs` | `Assets/Scripts/Core/` |
| **Chỉnh Combo, điểm thưởng liên hoàn** | `ComboManager.cs` | `Assets/Scripts/Core/` |
| **Chỉnh hệ thống Thẻ nâng cấp Roguelite Perk** | `PerkManager.cs` & `UIManager.PerkModal.cs` | `Core/` & `UI/` |
| **Chỉnh HUD máu, điểm số, icon bom, cooldown** | `UIManager.HUD.cs` | `Assets/Scripts/UI/` |
| **Chỉnh giao diện Hangar chọn & nâng cấp tàu** | `UIManager.Hangar.cs` | `Assets/Scripts/UI/` |
| **Chỉnh Menu Cài đặt, Hướng dẫn phím HowToPlay** | `UIManager.Modals.cs` | `Assets/Scripts/UI/` |
| **Chỉnh Bảng xếp hạng điểm cao** | `UIManager.Leaderboard.cs` | `Assets/Scripts/UI/` |
| **Chỉnh Hiệu ứng nền vũ trụ trôi (Parallax)** | `ParallaxBackground.cs` | `Assets/Scripts/Environment/` |
| **Chỉnh tỷ lệ rớt Vật phẩm (Shield, Star, Power)**| `PowerUp.cs` & `StarPickup.cs` | `Assets/Scripts/Environment/` |

---

## 🧩 3. Chi Tiết Các Phân Vùng Mã Nguồn (Script Modules)

### 🚀 `Player/` (Quản lý Người chơi)
- Sử dụng mô hình **Partial Class** để chia nhỏ `PlayerController` không bị phình to:
  - `PlayerController.cs`: Khai báo thông số, vòng đời Awake/Start, hit-box cockpit, đuôi lửa phản lực Plasma.
  - `PlayerController.Movement.cs`: Bay 2D (Bàn phím + Gamepad Analog 360°), lướt Dash bất tử (i-frame), tàn ảnh Ghost Trail.
  - `PlayerController.Weapons.cs`: Bắn đạn đơn/đôi/ba, tên lửa tự tìm mục tiêu, cơ chế xẹt đạn (Graze), trạng thái Overdrive Fever.
  - `PlayerController.Combat.cs`: Nhận sát thương, kích hoạt EMP Shockwave, rung tay cầm Haptic Dual-Motor, chết nổ tàu.
  - `CompanionDrone.cs`: Drone đồng minh bay quỹ đạo xung quanh yểm trợ hỏa lực.

### 👾 `Enemies/` (Quản lý Kẻ địch)
- `Enemy.cs`: Quái thường (V-formation, kamikaze, zic-zac), bắn đạn laser định kỳ, rớt PowerUp khi chết.
- `EnemySpawner.cs`: Quản lý đợt quái (Wave 1 đến Wave vô tận), spawn bầy quái, gọi Boss khi đạt mốc điểm.
- `BossController.cs`: Red UFO Mothership với nhiều pha chiến đấu: Bắn vòng hoa đạn Danmaku 12 tia, bắn chùm tia Hyper Beam quét ngang màn hình, triệu hồi drone bảo vệ.
- `SplittingMeteor.cs`: Thiên thạch phân mảnh khi bắn nổ thành nhiều mảnh nhỏ.

### ⚔️ `Combat/` (Đạn & Hiệu ứng Chạm)
- `Laser.cs`: Đạn laser 2 đầu (đạn người chơi và đạn địch), xử lý nảy đạn Tesla Arc, va chạm vật lý.
- `HomingMissile.cs`: Tên lửa tự dò mục tiêu gần nhất.
- `ShockwaveEffect.cs`: Sóng chấn động hình tròn bung rộng xóa đạn địch và hủy diệt quái.
- `HitFlashEffect.cs`: Nháy trắng Sprite khi bị trúng đạn.
- `HitSparkEffect.cs`: Bắn tia lửa điện khi đạn trúng mục tiêu.
- `AutoDestroy.cs`: Tự hủy hoặc đưa object về Pool sau khoảng thời gian `lifetime`.

### ⚙️ `Core/` (Hệ thống Trung tâm)
- `GameManager.cs`: Singleton điều phối trạng thái trò chơi (Menu ➔ Playing ➔ Paused ➔ GameOver).
- `AudioManager.cs`: Bộ sinh nhạc nền Procedural Synthwave 132 BPM (Kick, Snare, Hi-hat, Bass saw) và Ambient Hangar Pad 66 BPM, quản lý SFX.
- `PostProcessingManager.cs`: Điều khiển URP Bloom neon, Vignette nhịp tim nguy kịch, Chromatic Aberration giật quang sai.
- `CameraShake.cs`: Rung camera khi trúng đạn hoặc nổ bom, kèm cờ bật/tắt chống say xe.
- `ComboManager.cs`: Hệ số nhân điểm x1 đến x5 theo nhịp độ tiêu diệt địch.
- `PerkManager.cs`: Quản lý 8 loại thẻ bổ trợ Roguelite sau mỗi đợt Boss.
- `ObjectPoolManager.cs`: Tái sử dụng đạn và hạt nổ để đạt **Zero-GC** (không giật lag thu gom rác).
- `AchievementManager.cs` & `LeaderboardManager.cs`: Thành tựu và bảng xếp hạng điểm cao lưu vào `PlayerPrefs`.

### 🖥️ `UI/` (Giao diện Người dùng)
- Sử dụng mô hình **Partial Class** chia nhỏ `UIManager`:
  - `UIManager.cs`: Khởi tạo và liên kết các Panel chính.
  - `UIManager.HUD.cs`: Hiển thị điểm số, máu tim, lượng bom, thanh Fever, thanh Boss HP (áp dụng Dirty-flag cache tránh rebuild canvas).
  - `UIManager.Hangar.cs`: Xem trước phi thuyền, so sánh chỉ số, mua nâng cấp vĩnh viễn bằng sao.
  - `UIManager.Modals.cs`: Menu Settings (BGM, SFX, Screen Shake), bảng hướng dẫn How To Play (Keyboard + Gamepad), Achievements.
  - `UIManager.PerkModal.cs`: Hiển thị 3 lá bài Perk khi qua màn cho người chơi chọn.
  - `UIManager.Leaderboard.cs`: Bảng vàng vinh danh điểm cao.

---

## 🎯 4. Quy Chuẩn Đặt Tên & Code Cho Lần Làm Việc Sau

1. **Namespace**:
   - Toàn bộ code thuộc tàu người chơi đặt trong `namespace Player`.
   - Các script độc lập khác giữ liên kết chặt chẽ qua Singleton `Instance`.
2. **Hiệu năng (Zero Allocation)**:
   - Sử dụng `NonAlloc` physics (ví dụ: `Physics2D.OverlapCircleNonAlloc`).
   - Mọi đạn laser và tia lửa phải thông qua `ObjectPoolManager.Spawn` thay vì `Instantiate`.
3. **Độ an toàn Gamepad & Audio**:
   - Khi thêm tính năng rung tay cầm, luôn bọc trong `#if ENABLE_INPUT_SYSTEM` và nhớ ngắt motor trong `OnDisable`.
   - Khi thêm âm thanh, luôn có cơ chế fallback âm thanh mặc định để không bị NullReferenceException nếu thiếu file `.wav`.
