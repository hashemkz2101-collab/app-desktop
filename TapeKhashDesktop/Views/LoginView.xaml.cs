using System;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using TapeKhashDesktop.Data;

namespace TapeKhashDesktop.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
            BaseUrlBox.Text = Session.BaseUrl;
            Loaded += LoginView_Loaded;
        }

        private async void LoginView_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Session.RememberToken))
            {
                StatusText.Text = "در حال ورود خودکار...";
                try
                {
                    var res = await ApiClient.Call("loginWithRememberToken",
                        new JObject { ["remember_token"] = Session.RememberToken }, auth: false);
                    Session.Token = res.Value<string>("token");
                    Session.User = AppUser.FromJson((JObject)res["user"]);
                    MainWindow.Instance.NavigateToAppSelect();
                    return;
                }
                catch (Exception)
                {
                    Session.RememberToken = null;
                }
                StatusText.Text = "";
            }
        }

        private async void OnLoginClick(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = "";
            var username = UsernameBox.Text.Trim();
            var password = PasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ErrorText.Text = "نام کاربری و رمز عبور را وارد کنید.";
                return;
            }

            Session.BaseUrl = string.IsNullOrWhiteSpace(BaseUrlBox.Text) ? Session.DefaultBaseUrl : BaseUrlBox.Text.Trim();

            try
            {
                var body = new JObject
                {
                    ["username"] = username,
                    ["password"] = password,
                    ["remember"] = RememberCheck.IsChecked == true,
                };
                var res = await ApiClient.Call("login", body, auth: false);
                Session.Token = res.Value<string>("token");
                if (res["remember_token"] != null) Session.RememberToken = res.Value<string>("remember_token");
                Session.User = AppUser.FromJson((JObject)res["user"]);
                Session.Save();
                MainWindow.Instance.NavigateToAppSelect();
            }
            catch (ApiException ex)
            {
                ErrorText.Text = ex.Message;
            }
            catch (Exception)
            {
                ErrorText.Text = "خطا در ارتباط با سرور.";
            }
        }
    }
}
