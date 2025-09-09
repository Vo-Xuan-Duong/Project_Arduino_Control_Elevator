# ?? H? th?ng ?i?u khi?n Thang m�y

## ?? M� t?
?ng d?ng WinForm ?i?u khi?n 2 thang m�y 6 t?ng qua Serial v?i Arduino, c� PostgreSQL logging.
WinForm ch? g?i l?nh - Arduino x? l� to�n b? logic ?i?u khi?n.

## ?? T�nh n?ng
- ? G?i l?nh ?i?u khi?n 2 thang m�y (6 t?ng: 0-5)
- ? G?i l?nh g?i thang m�y t? c�c t?ng
- ? L?nh an to�n: D?ng kh?n c?p, b�o ch�y, b?o tr�
- ? **Auto-logging** real-time v?i PostgreSQL/Supabase
- ? Clear logs display (database v?n gi? data)
- ? Arduino x? l� logic t?i ?u v� ph�n ph?i t?i

## ?? C�ch s? d?ng

### 1. Setup Supabase Database:
1. Truy c?p [Supabase Dashboard](https://supabase.com/dashboard)
2. M? **SQL Editor**
3. Ch?y script `simple_setup.sql`
4. L?y connection string t? **Settings > Database**

### 2. C?u h�nh ?ng d?ng:
- **T? ??ng**: `config.json` ?� ???c c?u h�nh cho Supabase
- **Auto-connect** khi kh?i ??ng

### 3. Ch?y ?ng d?ng:
1. Ch?n COM Port
2. Click "K?t n?i"
3. G?i l?nh ?i?u khi?n thang m�y
4. Arduino x? l� logic v� b�o c�o tr?ng th�i
5. **Logs t? ??ng l?u** v�o Supabase real-time

## ?? Files ch�nh
- `Form1.cs` - WinForm UI v� Serial communication
- `simple_setup.sql` - Supabase setup script
- `config.json` - Supabase config (s?n s�ng)
- `elevator_control_arduino.ino` - Arduino logic x? l�

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

## ?? Ph�n chia Logic
- **WinForm**: UI, Serial communication, **Auto database logging**
- **Arduino**: Logic ?i?u khi?n, t?i ?u h�a, an to�n, ph�n ph?i t?i

## ?? Auto-Logging Features
- ?? **Real-time auto-save** m?i ho?t ??ng v�o database
- ?? **Session tracking** t? ??ng
- ??? **Structured logging** v?i metadata
- ?? **No manual save required** - everything is automatic
- ??? **Clear display only** - database keeps all data

**?? Arduino x? l� to�n b? logic th�ng minh - WinForm auto-log m?i th?!**