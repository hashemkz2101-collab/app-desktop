using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace TapeKhashDesktop.Data
{
    public class ApiException : Exception
    {
        public ApiException(string message) : base(message) { }
    }

    public static class ApiClient
    {
        private static readonly HttpClient Client;

        static ApiClient()
        {
            // ویندوز ۷ به‌صورت پیش‌فرض TLS 1.2 را فعال ندارد؛ صریحاً فعالش می‌کنیم وگرنه اتصال به هاست HTTPS شکست می‌خورد.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            Client = new HttpClient { Timeout = TimeSpan.FromSeconds(40) };
        }

        public static async Task<JObject> Call(string action, JObject body = null, bool auth = true)
        {
            body = body ?? new JObject();
            var url = Session.BaseUrl + "?action=" + action;

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(body.ToString(), System.Text.Encoding.UTF8, "application/json")
            };
            if (auth && !string.IsNullOrEmpty(Session.Token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Session.Token);

            HttpResponseMessage resp;
            try
            {
                resp = await Client.SendAsync(request);
            }
            catch (Exception)
            {
                throw new ApiException("خطا در اتصال به سرور. اتصال اینترنت را بررسی کنید.");
            }

            var text = await resp.Content.ReadAsStringAsync();
            JObject json;
            try { json = JObject.Parse(text); }
            catch { throw new ApiException("پاسخ سرور نامعتبر بود (کد " + (int)resp.StatusCode + ")."); }

            if (json.Value<bool?>("ok") != true)
                throw new ApiException(json.Value<string>("error") ?? "خطای نامشخص از سرور.");

            return json;
        }

        /// <summary>آپلود فایل چندبخشی به یک آدرس مطلق (upload.php تپه/کاتالوگ، importTapesExcel، یا sync.php)</summary>
        public static async Task<JObject> UploadMultipart(
            string url, string fileField, string filePath, string mimeType,
            System.Collections.Generic.Dictionary<string, string> extraFields = null,
            bool auth = false)
        {
            using (var form = new MultipartFormDataContent())
            {
                if (extraFields != null)
                    foreach (var kv in extraFields)
                        form.Add(new StringContent(kv.Value), kv.Key);

                var bytes = File.ReadAllBytes(filePath);
                var fileContent = new ByteArrayContent(bytes);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
                form.Add(fileContent, fileField, Path.GetFileName(filePath));

                var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = form };
                if (auth && !string.IsNullOrEmpty(Session.Token))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Session.Token);

                HttpResponseMessage resp;
                try { resp = await Client.SendAsync(request); }
                catch (Exception) { throw new ApiException("خطا در ارسال فایل به سرور."); }

                var text = await resp.Content.ReadAsStringAsync();
                JObject json;
                try { json = JObject.Parse(text); }
                catch { throw new ApiException("پاسخ سرور نامعتبر بود (کد " + (int)resp.StatusCode + ")."); }

                bool ok = json.Value<bool?>("ok") == true
                          || json.Value<bool?>("success") == true
                          || json.Value<string>("status") == "success";
                if (!ok)
                    throw new ApiException(json.Value<string>("error") ?? json.Value<string>("message") ?? "خطا در آپلود فایل.");

                return json;
            }
        }

        /// <summary>دانلود مستقیم یک فایل (برای همگام‌سازی) و ذخیره در مسیر محلی</summary>
        public static async Task DownloadFile(string url, string destinationPath)
        {
            var dir = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var bytes = await Client.GetByteArrayAsync(url);
            File.WriteAllBytes(destinationPath, bytes);
        }

        // ---------------- همگام‌سازی فایل‌ها (sync.php) ----------------

        public static async Task<JObject> SyncList(string syncKey)
        {
            var url = Session.FileSyncUrl + "?action=list&key=" + Uri.EscapeDataString(syncKey);
            var text = await Client.GetStringAsync(url);
            var json = JObject.Parse(text);
            if (json.Value<bool?>("ok") != true) throw new ApiException(json.Value<string>("error") ?? "خطا در دریافت لیست فایل‌ها.");
            return json;
        }

        public static async Task SyncDownload(string syncKey, string relativePath, string destinationPath)
        {
            var url = Session.FileSyncUrl + "?action=download&key=" + Uri.EscapeDataString(syncKey) + "&path=" + Uri.EscapeDataString(relativePath);
            await DownloadFile(url, destinationPath);
        }

        public static async Task SyncUpload(string syncKey, string relativePath, string localFilePath)
        {
            using (var form = new MultipartFormDataContent())
            {
                form.Add(new StringContent(syncKey), "key");
                form.Add(new StringContent(relativePath), "path");
                var bytes = File.ReadAllBytes(localFilePath);
                var fileContent = new ByteArrayContent(bytes);
                form.Add(fileContent, "file", Path.GetFileName(localFilePath));

                var url = Session.FileSyncUrl + "?action=upload";
                var resp = await Client.PostAsync(url, form);
                var text = await resp.Content.ReadAsStringAsync();
                var json = JObject.Parse(text);
                if (json.Value<bool?>("ok") != true) throw new ApiException(json.Value<string>("error") ?? "خطا در آپلود فایل همگام‌سازی.");
            }
        }
    }
}
