# 🚀 Space Defender (Bảo Vệ Không Gian)

Mini-game 2D thuộc thể loại **Space Shooter** (Bắn ruồi / Bắn tàu vũ trụ không gian) được phát triển bằng **Unity 6 (6000.6.0f1)** và **Universal Render Pipeline (URP 2D)**.

---

## 🎮 Giới Thiệu Gameplay

Người chơi điều khiển một phi thuyền không gian di chuyển qua lại để bắn hạ các phi thuyền địch và thiên thạch đang rơi từ trên xuống, né tránh đạn của địch và va chạm thiên thạch để bảo vệ dải ngân hà và sinh tồn đạt điểm số cao nhất.

### 🌟 Tính Năng Nổi Bật (Arcade Master Suite)
- **Hệ thống điều khiển chuẩn xác:** Hỗ trợ New Input System (`A`/`D`, phím Mũi tên, chuột, `Space`), tự động giới hạn biên camera.
- **Vật phẩm bổ trợ (Power-ups System):**
  - ⚡ **Triple Shot (Tia Chớp Vàng):** Bắn chùm 3 tia laser tỏa góc cánh quạt cực mạnh kéo dài 10 giây.
  - 🛡️ **Energy Shield (Khiên Xanh):** Tạo lồng bảo vệ quanh tàu, chống đỡ hoàn toàn 1 đòn tấn công mà không mất mạng.
  - ⭐ **Health Restore (Ngôi Sao Đỏ):** Hồi phục +1 trái tim sinh lực (tối đa 3 tim).
  - Tỉ lệ rơi ngẫu nhiên 25% khi bắn hạ kẻ địch hoặc thiên thạch.
- **Trận Đấu Trùm Mini-Boss (Red UFO Mothership):**
  - Xuất hiện tại các mốc điểm số cao (80 điểm trở lên).
  - Thanh máu Boss chuyên dụng (**Boss HP Bar**) trên đỉnh màn hình hiển thị trực quan (20/20 HP).
  - Di chuyển lượn sóng ngang, tấn công xả đạn Laser chùm 2-3 phát liên hồi.
  - Khi bị hạ gục mang lại 100 điểm thưởng, hiệu ứng nổ lớn và rơi chắc chắn 1 vật phẩm Power-up.
- **Độ Khó Tăng Tiến Động (Dynamic Difficulty Scaling):**
  - Tần suất xuất hiện kẻ địch dồn dập hơn và tốc độ bay nhanh hơn tương ứng theo điểm số của người chơi.
- **Rung Lắc Màn Hình & Điểm Số Bay (Camera Shake & Floating Text):**
  - Rung camera tác động mạnh khi trúng đòn hoặc diệt Boss, rung nhẹ phấn khích khi nổ tàu địch.
  - Chữ số điểm bay màu neon rực rỡ (`+10`, `+15`, `+25`, `+100 BOSS DEFEATED!`) nổi lên và mờ dần.
- **Cơ chế chiến đấu phong phú:**
  - Phi thuyền bắn tia đạn Laser xanh bay lên, tự hủy khi vượt biên trên. Phím `T` để bật/tắt chế độ tự bắn liên tục (Demo Mode).
  - Tàu địch bắn tia Laser đỏ bay xuống dưới tấn công người chơi.
  - Thiên thạch nhiều kích cỡ với hiệu ứng tự xoay góc khi rơi.
- **Hệ thống 3 Mạng (Lives System ❤️❤️❤️):** Khi bị trúng đạn hoặc va chạm địch, phi thuyền mất 1 tim, chớp nháy bất tử trong 1.5s. Hết 3 mạng mới Game Over.
- **Hệ thống Kỷ Lục Điểm Cao (High Score):** Tự động lưu kỷ lục qua `PlayerPrefs`, hiển thị cúp huy hiệu `★ NEW HIGH SCORE! ★` khi phá kỷ lục.
- **Menu & Giao Diện Đầy Đủ (4 Panels):**
  - **Main Menu:** Logo tiêu đề, nút **PLAY**, **HOW TO PLAY**, **EXIT**.
  - **In-Game HUD:** Điểm số thời gian thực, kỷ lục điểm cao, 3 tim mạng, thanh Boss HP Bar, nút Pause `||`.
  - **Pause Menu:** Phím `Esc`/`P` hoặc nút `||` để tạm dừng game, hỗ trợ Resume, Restart, Main Menu.
  - **Game Over Panel:** Tổng kết điểm, kỷ lục, nút Play Again và Main Menu.
- **Nền Vũ Trụ Liền Mạch (100% Seamless Scrolling):** Nền không gian sâu thẳm với hành tinh và vệt sao tốc độ cuộn dọc vô tận không tì vết.
- **Hiệu Ứng & Âm Thanh Polish:**
  - Hệ thống hạt Particle System: Ngọn lửa động cơ đuôi tàu và vụ nổ tung tóe khi mục tiêu bị tiêu diệt.
  - Âm thanh SFX đầy đủ: Tiếng bắn đạn, đạn địch, ăn vật phẩm (`sfx_shieldUp`), mất mạng (`sfx_shieldDown`), nổ (`sfx_zap`), thua cuộc (`sfx_lose`), click nút.
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
