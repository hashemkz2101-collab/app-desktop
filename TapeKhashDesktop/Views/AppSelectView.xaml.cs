using System.Windows;
using System.Windows.Controls;
using TapeKhashDesktop.Data;

namespace TapeKhashDesktop.Views
{
    public partial class AppSelectView : UserControl
    {
        public AppSelectView()
        {
            InitializeComponent();
            var user = Session.User;
            WelcomeText.Text = "سلام، " + (user?.DisplayName ?? "");

            bool hasTape = user?.HasApp("tape") == true;
            bool hasKhash = user?.HasApp("khash") == true;

            TapeButton.Visibility = hasTape ? Visibility.Visible : Visibility.Collapsed;
            KhashButton.Visibility = hasKhash ? Visibility.Visible : Visibility.Collapsed;
            NoAccessText.Visibility = (!hasTape && !hasKhash) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnOpenTape(object sender, RoutedEventArgs e) => MainWindow.Instance.NavigateToTape();
        private void OnOpenKhash(object sender, RoutedEventArgs e) => MainWindow.Instance.NavigateToKhash();

        private void OnLogoutClick(object sender, RoutedEventArgs e)
        {
            Session.ClearLogin();
            MainWindow.Instance.NavigateToLogin();
        }
    }
}
