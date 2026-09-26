using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using TapeKhashDesktop.Data;

namespace TapeKhashDesktop.Views.Tape
{
    public partial class ExcelImportControl : UserControl
    {
        public ExcelImportControl()
        {
            InitializeComponent();
        }

        private async void OnPickClick(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "فایل اکسل|*.xlsx" };
            if (dlg.ShowDialog() != true) return;

            ErrorText.Text = ""; ResultText.Text = "";
            Progress.Visibility = Visibility.Visible;
            try
            {
                var url = Session.BaseUrl + "?action=importTapesExcel";
                var res = await ApiClient.UploadMultipart(
                    url, "file", dlg.FileName,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    auth: true);

                ResultText.Text = $"ایجاد شده: {res.Value<int>("created")} — به‌روزرسانی: {res.Value<int>("updated")} — رد شده: {res.Value<int>("skipped")}";
            }
            catch (ApiException ex) { ErrorText.Text = ex.Message; }
            catch (Exception) { ErrorText.Text = "خطا در ارسال فایل."; }
            finally { Progress.Visibility = Visibility.Collapsed; }
        }
    }
}
