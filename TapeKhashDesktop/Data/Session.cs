using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TapeKhashDesktop.Data
{
    public class AppUser
    {
        public int Id;
        public string Username;
        public string DisplayName;
        public string Role;
        public List<string> Apps = new List<string>();

        public bool IsAdmin => Role == "admin";
        public bool HasApp(string app) => IsAdmin || Apps.Contains(app);

        public static AppUser FromJson(JObject o)
        {
            var u = new AppUser
            {
                Id = o.Value<int?>("id") ?? 0,
                Username = o.Value<string>("username") ?? "",
                DisplayName = o.Value<string>("display_name") ?? "",
                Role = o.Value<string>("role") ?? "user",
            };
            var apps = o["apps"] as JArray;
            if (apps != null)
                foreach (var a in apps) u.Apps.Add(a.ToString());
            return u;
        }
    }

    public static class Session
    {
        // آدرس پیش‌فرض بک‌اند - اگر پوشه‌ی روی هاست را عوض کردید همین‌جا یا در صفحه‌ی ورود تغییر دهید
        public const string DefaultBaseUrl = "https://mahroch.ir/tape-khash-api/index.php";
        public const string DefaultFileSyncUrl = "https://mahroch.ir/file/sync.php";

        public static string BaseUrl = DefaultBaseUrl;
        public static string FileSyncUrl = DefaultFileSyncUrl;
        public static string Token;
        public static string RememberToken;
        public static AppUser User;
        public static string LocalSyncFolder;

        private static string ConfigPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TapeKhashDesktop", "session.json");

        public static bool IsLoggedIn => !string.IsNullOrEmpty(Token) && User != null;

        public static void Save()
        {
            try
            {
                var dir = Path.GetDirectoryName(ConfigPath);
                if (dir != null && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

                var data = new
                {
                    baseUrl = BaseUrl,
                    fileSyncUrl = FileSyncUrl,
                    rememberToken = RememberToken,
                    localSyncFolder = LocalSyncFolder,
                };
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(data));
            }
            catch { /* ذخیره‌ی تنظیمات محلی حیاتی نیست؛ خطا نادیده گرفته می‌شود */ }
        }

        public static void Load()
        {
            try
            {
                if (!File.Exists(ConfigPath)) return;
                var json = JObject.Parse(File.ReadAllText(ConfigPath));
                BaseUrl = json.Value<string>("baseUrl") ?? DefaultBaseUrl;
                FileSyncUrl = json.Value<string>("fileSyncUrl") ?? DefaultFileSyncUrl;
                RememberToken = json.Value<string>("rememberToken");
                LocalSyncFolder = json.Value<string>("localSyncFolder");
            }
            catch { }
        }

        public static void ClearLogin()
        {
            Token = null;
            RememberToken = null;
            User = null;
            Save();
        }
    }
}
