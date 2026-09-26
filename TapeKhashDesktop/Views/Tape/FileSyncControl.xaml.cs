using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using TapeKhashDesktop.Data;

namespace TapeKhashDesktop.Views.Tape
{
    public partial class FileSyncControl : UserControl
    {
        private string _uploadKey;

        public FileSyncControl()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            FolderBox.Text = Session.LocalSyncFolder ?? "";
        }

        private void OnChooseFolderClick(object sender, RoutedEventArgs e)
        {
            using (var dlg = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FolderBox.Text = dlg.SelectedPath;
                    Session.LocalSyncFolder = dlg.SelectedPath;
                    Session.Save();
                }
            }
        }

        private async void OnSyncClick(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = ""; SummaryText.Text = ""; LogList.Items.Clear();

            if (string.IsNullOrWhiteSpace(FolderBox.Text) || !Directory.Exists(FolderBox.Text))
            {
                ErrorText.Text = "ابتدا یک پوشه‌ی محلی معتبر انتخاب کنید.";
                return;
            }

            Progress.Visibility = Visibility.Visible;
            try
            {
                if (_uploadKey == null)
                {
                    var cfg = await ApiClient.Call("getUploadConfig");
                    _uploadKey = cfg.Value<string>("uploadKey");
                }

                var remoteRes = await ApiClient.SyncList(_uploadKey);
                var remoteFiles = new Dictionary<string, (long size, long mtime, string hash)>();
                foreach (var item in (JArray)remoteRes["files"])
                {
                    var f = (JObject)item;
                    remoteFiles[f.Value<string>("path")] = (f.Value<long>("size"), f.Value<long>("mtime"), f.Value<string>("hash"));
                }

                var localFiles = ScanLocalFolder(FolderBox.Text);

                int downloaded = 0, uploaded = 0, skipped = 0;

                // فایل‌های هاست -> دانلود اگر جدید هستند یا هش فرق دارد و هاست جدیدتر است
                foreach (var kv in remoteFiles)
                {
                    var relPath = kv.Key;
                    var remote = kv.Value;
                    var localPath = Path.Combine(FolderBox.Text, relPath.Replace('/', Path.DirectorySeparatorChar));

                    if (!localFiles.ContainsKey(relPath))
                    {
                        await ApiClient.SyncDownload(_uploadKey, relPath, localPath);
                        LogList.Items.Add("⬇ دانلود: " + relPath);
                        downloaded++;
                    }
                    else if (localFiles[relPath].hash != remote.hash)
                    {
                        var localMtime = new DateTimeOffset(File.GetLastWriteTimeUtc(localPath)).ToUnixTimeSeconds();
                        if (remote.mtime >= localMtime)
                        {
                            await ApiClient.SyncDownload(_uploadKey, relPath, localPath);
                            LogList.Items.Add("⬇ به‌روزرسانی (از هاست): " + relPath);
                            downloaded++;
                        }
                        else
                        {
                            await ApiClient.SyncUpload(_uploadKey, relPath, localPath);
                            LogList.Items.Add("⬆ به‌روزرسانی (به هاست): " + relPath);
                            uploaded++;
                        }
                    }
                    else
                    {
                        skipped++;
                    }
                }

                // فایل‌های محلی که در هاست نیستند -> آپلود
                foreach (var kv in localFiles)
                {
                    if (!remoteFiles.ContainsKey(kv.Key))
                    {
                        var localPath = Path.Combine(FolderBox.Text, kv.Key.Replace('/', Path.DirectorySeparatorChar));
                        await ApiClient.SyncUpload(_uploadKey, kv.Key, localPath);
                        LogList.Items.Add("⬆ آپلود: " + kv.Key);
                        uploaded++;
                    }
                }

                SummaryText.Text = $"دانلود: {downloaded} — آپلود: {uploaded} — بدون تغییر: {skipped}";
            }
            catch (ApiException ex) { ErrorText.Text = ex.Message; }
            catch (Exception ex) { ErrorText.Text = "خطا در همگام‌سازی: " + ex.Message; }
            finally { Progress.Visibility = Visibility.Collapsed; }
        }

        private Dictionary<string, (long size, long mtime, string hash)> ScanLocalFolder(string root)
        {
            var result = new Dictionary<string, (long, long, string)>();
            foreach (var path in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
            {
                var relPath = path.Substring(root.Length).TrimStart(Path.DirectorySeparatorChar).Replace(Path.DirectorySeparatorChar, '/');
                var info = new FileInfo(path);
                var hash = Md5OfFile(path);
                result[relPath] = (info.Length, new DateTimeOffset(info.LastWriteTimeUtc).ToUnixTimeSeconds(), hash);
            }
            return result;
        }

        private static string Md5OfFile(string path)
        {
            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(path))
            {
                var hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
