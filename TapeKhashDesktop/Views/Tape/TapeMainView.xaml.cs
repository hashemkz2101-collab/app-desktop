using System.Windows;
using System.Windows.Controls;
using TapeKhashDesktop.Data;

namespace TapeKhashDesktop.Views.Tape
{
    public partial class TapeMainView : UserControl
    {
        public TapeMainView()
        {
            InitializeComponent();
            if (Session.User?.IsAdmin == true)
                AdminTab.Visibility = Visibility.Visible;
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.NavigateToAppSelect();
        }
    }
}
