using System.Windows;
using Newtonsoft.Json.Linq;
using TapeKhashDesktop.Data;

namespace TapeKhashDesktop.Views.Tape
{
    public partial class AddUserWindow : Window
    {
        public bool Created { get; private set; }

        public AddUserWindow()
        {
            InitializeComponent();
        }

        private async void OnSaveClick(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = "";
            if (string.IsNullOrWhiteSpace(UsernameBox.Text) || string.IsNullOrWhiteSpace(PasswordBox.Text) || string.IsNullOrWhiteSpace(DisplayNameBox.Text))
            {
                ErrorText.Text = "همه‌ی فیلدها الزامی است.";
                return;
            }
            try
            {
                var apps = new JArray();
                if (TapeAccessCheck.IsChecked == true) apps.Add("tape");
                if (KhashAccessCheck.IsChecked == true) apps.Add("khash");

                await ApiClient.Call("addUser", new JObject
                {
                    ["username"] = UsernameBox.Text.Trim(),
                    ["password"] = PasswordBox.Text,
                    ["display_name"] = DisplayNameBox.Text.Trim(),
                    ["role"] = IsAdminCheck.IsChecked == true ? "admin" : "user",
                    ["apps"] = apps,
                });
                Created = true;
                Close();
            }
            catch (ApiException ex) { ErrorText.Text = ex.Message; }
        }

        private void OnCancelClick(object sender, RoutedEventArgs e) => Close();
    }
}
