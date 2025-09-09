# Hệ thống điều khiển thang máy (Arduino + WinForms)

Dự án mô phỏng/điều khiển 2 thang máy (6 tầng: 0-5) bằng Arduino và ứng dụng Windows Forms (.NET 8). Ứng dụng PC gửi lệnh qua Serial, Arduino xử lý toàn bộ logic vận hành. Tùy chọn ghi log real-time vào PostgreSQL/Supabase.

## Thành phần chính
- Phần mềm PC: `ControlThangMay/` (WinForms .NET 8, giao tiếp Serial, ghi log PostgreSQL)
- Firmware Arduino:
  - `ControlThangMay/elevator_control_arduino.ino` (khuyến nghị dùng – phiên bản tối ưu realtime)
  - `ThangMay.ino` (phiên bản cũ hơn)
- CSDL (tùy chọn): `ControlThangMay/simple_setup.sql` – script tạo bảng cho PostgreSQL/Supabase
- Cấu hình DB: `ControlThangMay/config.json`

## Kiến trúc tổng quan
- PC (WinForms):
  - Giao diện điều khiển 2 thang (gọi tầng, mở/đóng cửa, dừng khẩn, bảo trì, báo cháy…)
  - Gửi lệnh dạng văn bản qua Serial (COM)
  - Hiển thị nhật ký, lọc theo loại, đếm thông báo tab
  - Ghi log vào PostgreSQL (tùy chọn) qua Npgsql
- Arduino (Mega 2560 khuyến nghị):
  - Xử lý logic 2 cabin độc lập, phân bổ cabin theo điểm số (khoảng cách, hướng, tải hàng đợi)
  - LCD 20x4 cho mỗi cabin, LED hiển thị hàng đợi, nút gọi sảnh và trong cabin
  - Phát sự kiện qua Serial: snapshot trạng thái (S,...) và event nút/ trạng thái (EV,...)

## Tính năng nổi bật
- 2 thang máy, 6 tầng (0–5)
- Gọi thang tại sảnh theo hướng Lên/Xuống; chọn tầng trong cabin
- Điều khiển: Đi đến tầng, Mở/Đóng cửa, Dừng khẩn, Báo cháy (về tầng trệt), Bảo trì, Khởi động lại
- Ghi log real-time (tùy chọn) vào PostgreSQL với metadata (phiên, máy, người dùng, mức ưu tiên)

## Giao thức Serial (tóm tắt)
- Lệnh từ PC gửi sang Arduino (mỗi lệnh kết thúc bằng \n):
  - INIT_SYSTEM
  - E1_INIT, E2_INIT
  - E1_GOTO_<floor>, E2_GOTO_<floor>
  - E1_STOP, E2_STOP
  - E1_OPEN_DOOR, E2_OPEN_DOOR
  - E1_CLOSE_DOOR, E2_CLOSE_DOOR
  - E1_RESTART, E2_RESTART
  - CALL_TO_FLOOR_<floor>_[UP|DOWN]
  - EMERGENCY_STOP_ALL, FIRE_ALARM, MAINTENANCE_MODE
  - SYSTEM_RESTART, SYSTEM_SHUTDOWN
- Phản hồi/telemetry từ Arduino:
  - Xác nhận lệnh: `CMD,ACK,<COMMAND>` hoặc lỗi: `CMD,ERROR,UNKNOWN_COMMAND,<raw>`
  - Sự kiện nút: `EV,BTN,<grp>,<idx>,<pressed>,<t>` (grp: H/A/B/O/C)
  - Sự kiện trạng thái: `EV,ST,<car>,<floor>,<dir>,<state>,<t>` (car: A/B; dir:-1/0/1; state:0..6)
  - Ảnh chụp nhanh: `S,<t>,Af,Ad,As,Aup,Adn,Bf,Bd,Bs,Bup,Bdn,hall,cabA,cabB,open,close`

## Yêu cầu hệ thống
- Windows 10/11, .NET 8 SDK/Runtime
- Arduino IDE (khuyến nghị Arduino Mega 2560 do dùng nhiều chân số/analog)
- PostgreSQL 14+ hoặc Supabase (tùy chọn, để ghi log)

## Hướng dẫn cài đặt và chạy
1) Phần cứng
- Kết nối bo Arduino (Mega 2560) qua USB. Lắp LCD 20x4 cho Cabin A/B, nút sảnh/cabin và LED theo sơ đồ chân trong file `.ino` (ví dụ: LED/cabin A: 46–51, cabin B: 52–A3; nút sảnh: 36–45; cảm biến cửa A6/A7...).

2) Nạp firmware Arduino
- Mở `ControlThangMay/elevator_control_arduino.ino` bằng Arduino IDE
- Board: Arduino Mega 2560, Baud: 9600
- Upload chương trình

3) Cấu hình cơ sở dữ liệu (tùy chọn)
- Tạo DB PostgreSQL hoặc dự án Supabase
- Chạy script `ControlThangMay/simple_setup.sql`
- Sửa `ControlThangMay/config.json` cho phù hợp (SSL, tài khoản, mật khẩu). Ví dụ:
```json
{
  "database": {
    "host": "localhost",
    "port": 5432,
    "database": "postgres",
    "username": "postgres",
    "password": "123456",
    "enableSSL": false,
    "enableLogging": true
  }
}
```
Ghi chú: Ứng dụng sẽ tự tạo/sửa `config.json` nếu thiếu; có thể tắt ghi log bằng `enableLogging=false`.

4) Chạy ứng dụng WinForms
- Mở `ControlThangMay/ControlThangMay.sln` bằng Visual Studio 2022 (hoặc dùng `dotnet build`)
- Build cấu hình Debug, chạy `ControlThangMay.exe` (trong `ControlThangMay/bin/Debug/net8.0-windows/`)
- Chọn COM Port và Baud 9600, nhấn “KẾT NỐI”
- Gửi lệnh điều khiển thang máy từ giao diện

## Nhật ký và cơ sở dữ liệu
- Ứng dụng hiển thị log theo màu/loại (System/Elevator/User/Emergency/Error/Warning) và tự động lưu DB nếu bật
- Bảng chính: `elevator` (danh mục), `log` (nhật ký). Chỉ số thời gian và loại log đã được tạo trong script
- Mức ưu tiên log: Emergency(4), Error(3), Warning/System(2), Elevator/User(1)

## Cấu trúc thư mục
- `ControlThangMay/` – mã nguồn WinForms, cấu hình, script SQL, firmware Arduino tối ưu
- `ThangMay.ino` – firmware Arduino phiên bản cũ

## Sự cố thường gặp
- Không thấy COM Port: kiểm tra driver/USB, dùng nút “Làm mới” trong app
- Không nhận dữ liệu: đúng Baud (9600), cáp USB, chọn đúng cổng
- DB không kết nối: kiểm tra host/port, SSL (Supabase cần SSL=true), tài khoản/mật khẩu, firewall

---
Tác giả: Vo-Xuan-Duong – Project_Arduino_Control_Elevator
