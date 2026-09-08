# 🚀 Space Defender (Bảo Vệ Không Gian)

Mini-game 2D thuộc thể loại **Space Shooter** (Bắn ruồi / Bắn tàu vũ trụ không gian) được phát triển bằng **Unity 6 (6000.6.0f1)** và **Universal Render Pipeline (URP 2D)**.

---

## 🎮 Giới Thiệu Gameplay

Người chơi điều khiển một phi thuyền không gian di chuyển qua lại để bắn hạ các phi thuyền địch và thiên thạch đang rơi từ trên xuống, né tránh đạn của địch và va chạm thiên thạch để bảo vệ dải ngân hà và sinh tồn đạt điểm số cao nhất.

### 🌟 Tính Năng Nổi Bật (100/100 Điểm & Polish Cao Cấp)
- **Hệ thống điều khiển chuẩn xác:** Hỗ trợ New Input System (`A`/`D`, phím Mũi tên, chuột, `Space`), tự động giới hạn biên camera.
- **Cơ chế chiến đấu phong phú:**
  - Phi thuyền bắn tia đạn Laser xanh bay lên, tự hủy khi vượt biên trên. Phím `T` để bật/tắt chế độ tự bắn liên tục (Demo Mode).
  - Tàu địch bắn tia Laser đỏ bay xuống dưới tấn công người chơi.
  - Thiên thạch nhiều kích cỡ với hiệu ứng tự xoay góc khi rơi.
- **Hệ thống 3 Mạng (Lives System ❤️❤️❤️):** Khi bị trúng đạn hoặc va chạm địch, phi thuyền mất 1 tim, chớp nháy bất tử trong 1.5s. Hết 3 mạng mới Game Over.
- **Hệ thống Kỷ Lục Điểm Cao (High Score):** Tự động lưu kỷ lục qua `PlayerPrefs`, hiển thị cúp huy hiệu `★ NEW HIGH SCORE! ★` khi phá kỷ lục.
- **Menu & Giao Diện Đầy Đủ (4 Panels):**
  - **Main Menu:** Logo tiêu đề, nút **PLAY**, **HOW TO PLAY**, **EXIT**.
  - **In-Game HUD:** Điểm số thời gian thực, kỷ lục điểm cao, 3 tim mạng, nút Pause `||`.
  - **Pause Menu:** Phím `Esc`/`P` hoặc nút `||` để tạm dừng game, hỗ trợ Resume, Restart, Main Menu.
  - **Game Over Panel:** Tổng kết điểm, kỷ lục, nút Play Again và Main Menu.
- **Nền Vũ Trụ Liền Mạch (100% Seamless Scrolling):** Nền không gian sâu thẳm với hành tinh và vệt sao tốc độ cuộn dọc vô tận không tì vết.
- **Hiệu Ứng & Âm Thanh Polish:**
  - Hệ thống hạt Particle System: Ngọn lửa động cơ đuôi tàu và vụ nổ tung tóe khi mục tiêu bị tiêu diệt.
  - Âm thanh SFX đầy đủ: Tiếng bắn đạn, đạn địch, mất mạng (`sfx_shieldDown`), nổ (`sfx_zap`), thua cuộc (`sfx_lose`), click nút.
  - Nhạc nền vũ trụ BGM không gian du dương lặp vô tận.

---

## 🕹️ Hướng Dẫn Điều Khiển

| Thao tác | Phím bấm / Chuột |
| :--- | :--- |
| **Bắt đầu Game (Từ Menu)** | Click nút **PLAY** hoặc nhấn phím **Enter** / **Space** |
| **Di chuyển Trái / Phải** | Phím **A** / **D** hoặc các phím **Mũi tên** |
| **Bắn đạn Laser** | Phím **Space** (Phím cách) hoặc **Click chuột trái** |
| **Bật/Tắt tự động bắn (Demo)** | Phím **T** |
| **Tạm dừng / Tiếp tục** | Phím **Escape** / **P** hoặc click nút **||** |
| **Chơi lại (Khi Game Over)** | Click nút **PLAY AGAIN** hoặc nhấn phím **R** |
| **Về Menu Chính** | Click nút **MAIN MENU** trên bảng Pause hoặc Game Over |

---

## 🛠️ Công Nghệ & Tài Nguyên

- **Engine:** Unity 6000.6.0f1 (URP 2D)
- **Input:** Unity New Input System (`com.unity.inputsystem`) & Legacy Fallback
- **Tài nguyên:** [Kenney Space Shooter Remastered (CC0)](https://kenney.nl/assets/space-shooter-remastered)
- **Mã nguồn:** Kiến trúc Clean Code, Singleton Managers (`GameManager`, `UIManager`, `AudioManager`), Spawner hướng đối tượng, Prefabs module hóa.

---

## 📂 Hướng Dẫn Chạy Dự Án

1. Clone repository về máy:
   ```bash
   git clone https://github.com/tuandnb04/Space-Defender.git
   ```
2. Mở Unity Hub và thêm thư mục dự án (khuyến nghị Unity 6000.6+).
3. Mở cảnh chính: `Assets/Scenes/SampleScene.unity`.
4. Nhấn nút **Play** trên Unity Editor để trải nghiệm ngay.
