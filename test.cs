using System;

public class Test
{
    public static void Main()
    {
        // 🔧 Thay đổi thông tin kết nối của bạn ở đây
        string host = "db.xxx.supabase.co";
        int port = 5432;
        string database = "postgres";   // Supabase mặc định là postgres
        string username = "postgres";
        string password = "YOUR_PWD";

        // Nếu host là supabase.co => dùng SSL Require
        bool isSupabase = host.EndsWith(".supabase.co", StringComparison.OrdinalIgnoreCase);
        string sslMode = isSupabase ? "Require" : "Disable";

        string connString =
            $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode={sslMode};Timeout=15;";

        Console.WriteLine("🔗 Đang thử kết nối tới PostgreSQL...");

        try
        {
            using var conn = new NpgsqlConnection(connString);
            conn.Open();

            Console.WriteLine("✅ Kết nối thành công!");

            using var cmd = new NpgsqlCommand("SELECT version()", conn);
            string version = (string)cmd.ExecuteScalar();
            Console.WriteLine("🖥 PostgreSQL version: " + version);
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Lỗi kết nối: " + ex.Message);
            if (ex.InnerException != null)
                Console.WriteLine("❌ Inner: " + ex.InnerException.Message);
        }
    }
}