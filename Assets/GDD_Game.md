1. Tổng quan dự án (Project Overview)
Tên dự án (Tạm gọi): Boom Dog.
Thể loại: Thẻ bài (Card Game), Turn-based Strategy, Party Game.
Nền tảng: PC / Mobile.
Engine: Unity (2.5D Perspective).
Mô hình mạng: Online Multiplayer (hỗ trợ phòng từ 2 - 5 người chơi).
2. Định hướng Nghệ thuật (Art & Visuals)
Camera: Góc nhìn 2.5D (Camera 3D đặt góc chếch từ trên xuống – Isometric hoặc Top-down nghiêng) chiếu vào một bàn chơi 3D.
Nhân vật & Môi trường (3D Low-poly):


Bàn chơi, bối cảnh xung quanh và các avatar đại diện cho người chơi sẽ được dựng bằng mô hình 3D Low-poly.
Avatar có thể có các animation đơn giản (vui mừng khi bốc được bài tốt, hoảng hốt khi bốc trúng mèo nổ).
Thẻ bài & UI (2D High-Detail):


Các lá bài hiển thị trên tay người chơi và khi được đánh ra giữa bàn sẽ là các asset 2D chất lượng cao.
Sử dụng Unity Canvas (hoặc Sprite Renderer trong không gian 3D với billboard effect) để đảm bảo thẻ bài luôn hiển thị sắc nét và dễ đọc text.
3. Lối chơi cốt lõi (Core Gameplay & Mechanics)
Vòng lặp trò chơi (Gameplay Loop):

Bắt đầu lượt: Người chơi có thể đánh ra bao nhiêu lá bài tùy thích (hoặc không đánh lá nào).
Kết thúc lượt: Bắt buộc phải bốc 1 lá bài từ nọc (Draw Pile), trừ khi vừa đánh lá bài có hiệu ứng bỏ qua lượt (Skip/Attack).
Điều kiện thua: Bốc trúng lá "Mèo Nổ" (Exploding Kitten) và không có lá "Gỡ Mìn" (Defuse). Người chơi đó sẽ bị loại.
Điều kiện thắng: Là người sống sót cuối cùng.
Hệ thống thẻ bài cơ bản:

Exploding Kitten: Loại người chơi nếu không có Defuse.
Defuse (Gỡ mìn): Vô hiệu hóa Mèo Nổ và đặt lại lá Mèo Nổ vào nọc.
Attack (Tấn công): Kết thúc lượt không cần bốc bài và ép người chơi tiếp theo phải đi 2 lượt.
Skip (Qua lượt): Kết thúc lượt không cần bốc bài.
See the Future (Nhìn trộm): Xem trước 3 lá trên cùng của nọc.
Shuffle (Xào bài): Trộn lại nọc.
Nope (Chặn): Hủy hiệu ứng của lá bài người khác vừa đánh (trừ Mèo Nổ và Defuse).

4. Kiến trúc Hệ thống & Kỹ thuật (Technical Architecture)
Hệ thống Mạng (Networking):
Có thể sử dụng Photon (PUN 2 / Fusion) hoặc Unity Netcode for GameObjects (NGO) kết hợp Unity Relay để xử lý Real-time Multiplayer.
Cơ chế Host-Client: Một người tạo phòng (Host), những người khác nhập code để vào (Client).
Quản lý Trạng thái (State Machine):
Cần một GameManager sử dụng State Machine để kiểm soát luồng trò chơi (Lobby State -> Deal Cards State -> Player Turn State -> End Game State).
Quản lý Thẻ bài:
Sử dụng ScriptableObject trong Unity để lưu trữ dữ liệu của từng loại thẻ (ID, Tên, Loại thẻ, Ảnh 2D, Mô tả hiệu ứng). Việc này giúp bạn dễ dàng thêm bớt thẻ mới mà không phải sửa code nhiều.
5. Giao diện người dùng (UI/UX)
Main Menu: Nút Tạo phòng (Create Room), Tìm phòng (Join Room), Cài đặt.
Lobby: Hiển thị danh sách người chơi trong phòng chờ, nút "Sẵn sàng" và "Bắt đầu" (dành cho Host).
In-game HUD:
Khu vực bài trên tay (chỉ bản thân nhìn thấy mặt trước).
Khu vực bài của đối thủ (chỉ nhìn thấy mặt lưng, hiển thị số lượng lá bài).
Khu vực Nọc (Draw Pile) và Mộ (Discard Pile) ở giữa bàn.
Log hệ thống (thông báo ai vừa đánh lá gì).

