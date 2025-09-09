# ?? H? th?ng ?i?u khi?n Thang máy

## ?? Mô t?
?ng d?ng WinForm ?i?u khi?n 2 thang máy 6 t?ng qua Serial v?i Arduino, có PostgreSQL logging.
WinForm ch? g?i l?nh - Arduino x? lý toàn b? logic ?i?u khi?n.

## ?? Tính n?ng
- ? G?i l?nh ?i?u khi?n 2 thang máy (6 t?ng: 0-5)
- ? G?i l?nh g?i thang máy t? các t?ng
- ? L?nh an toàn: D?ng kh?n c?p, báo cháy, b?o trì
- ? **Auto-logging** real-time v?i PostgreSQL/Supabase
- ? Clear logs display (database v?n gi? data)
- ? Arduino x? lý logic t?i ?u và phân ph?i t?i

## ?? Cách s? d?ng

### 1. Setup Supabase Database:
1. Truy c?p [Supabase Dashboard](https://supabase.com/dashboard)
2. M? **SQL Editor**
3. Ch?y script `simple_setup.sql`
4. L?y connection string t? **Settings > Database**

### 2. C?u hình ?ng d?ng:
- **T? ??ng**: `config.json` ?ã ???c c?u hình cho Supabase
- **Auto-connect** khi kh?i ??ng

### 3. Ch?y ?ng d?ng:
1. Ch?n COM Port
2. Click "K?t n?i"
3. G?i l?nh ?i?u khi?n thang máy
4. Arduino x? lý logic và báo cáo tr?ng thái
5. **Logs t? ??ng l?u** vào Supabase real-time

## ?? Files chính
- `Form1.cs` - WinForm UI và Serial communication
- `simple_setup.sql` - Supabase setup script
- `config.json` - Supabase config (s?n sàng)
- `elevator_control_arduino.ino` - Arduino logic x? lý

## ??? Requirements
- .NET 8.0 Windows
- Supabase account (free)
- Arduino/COM Port

## ?? Supabase Features
- ? Cloud PostgreSQL database
- ? **Automatic real-time logging**
- ? Row Level Security
- ? Auto-backup
- ? Free tier available

## ?? Phân chia Logic
- **WinForm**: UI, Serial communication, **Auto database logging**
- **Arduino**: Logic ?i?u khi?n, t?i ?u hóa, an toàn, phân ph?i t?i

## ?? Auto-Logging Features
- ?? **Real-time auto-save** m?i ho?t ??ng vào database
- ?? **Session tracking** t? ??ng
- ??? **Structured logging** v?i metadata
- ?? **No manual save required** - everything is automatic
- ??? **Clear display only** - database keeps all data

**?? Arduino x? lý toàn b? logic thông minh - WinForm auto-log m?i th?!**