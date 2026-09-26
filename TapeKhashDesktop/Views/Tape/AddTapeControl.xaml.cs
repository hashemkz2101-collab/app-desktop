using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using TapeKhashDesktop.Data;

namespace TapeKhashDesktop.Views.Tape
{
    public partial class AddTapeControl : UserControl
    {
        private string _imagePath;

        public AddTapeControl()
        {
            InitializeComponent();
        }

        private void OnPickImageClick(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "تصاویر|*.jpg;*.jpeg;*.png;*.webp" };
            if (dlg.ShowDialog() == true)
            {
                _imagePath = dlg.FileName;
                PreviewImage.Source = new BitmapImage(new Uri(_imagePath));
            }
        }

        private async void OnSubmitClick(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = ""; MessageText.Text = "";
            var boxCode = BoxCodeBox.Text.Trim();
            var tapeCode = TapeCodeBox.Text.Trim();

            if (string.IsNullOrEmpty(boxCode) || string.IsNullOrEmpty(tapeCode))
            {
                ErrorText.Text = "کد باکس و کد تپه را وارد کنید.";
                return;
            }
            if (!Regex.IsMatch(tapeCode, "^[BGbg][0-9]+$"))
            {
                ErrorText.Text = "کد تپه باید مانند G1 یا B22 باشد.";
                return;
            }

            SubmitButton.IsEnabled = false;
            Progress.Visibility = Visibility.Visible;
            try
            {
                string imageCode = "";
                if (!string.IsNullOrEmpty(_imagePath))
                {
                    var cfg = await ApiClient.Call("getUploadConfig");
                    var uploadUrl = cfg.Value<string>("uploadImageApi");
                    var uploadKey = cfg.Value<string>("uploadKey");

                    var ext = System.IO.Path.GetExtension(_imagePath).ToLower();
                    var mime = ext == ".png" ? "image/png" : ext == ".webp" ? "image/webp" : "image/jpeg";

                    var uploadRes = await ApiClient.UploadMultipart(
                        uploadUrl, "file", _imagePath, mime,
                        new System.Collections.Generic.Dictionary<string, string> { ["key"] = uploadKey });
                    imageCode = uploadRes.Value<string>("name") ?? "";
                }

                var body = new JObject { ["boxCode"] = boxCode, ["tapeCode"] = tapeCode };
                body["imageCode"] = string.IsNullOrEmpty(imageCode) ? tapeCode : imageCode;

                var res = await ApiClient.Call("addTape", body);
                MessageText.Text = res.Value<string>("mode") == "image-added"
                    ? "عکس جدید به تپه‌ی موجود اضافه شد."
                    : "تپه با موفقیت ثبت شد.";

                BoxCodeBox.Text = ""; TapeCodeBox.Text = ""; _imagePath = null; PreviewImage.Source = null;
            }
            catch (ApiException ex) { ErrorText.Text = ex.Message; }
            catch (Exception) { ErrorText.Text = "خطا در ارتباط با سرور."; }
            finally
            {
                SubmitButton.IsEnabled = true;
                Progress.Visibility = Visibility.Collapsed;
            }
        }
    }
}
