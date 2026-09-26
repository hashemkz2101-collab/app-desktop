using System.Windows;
using System.Windows.Controls;

namespace TapeKhashDesktop.Views.Khash
{
    public partial class KhashMainView : UserControl
    {
        public KhashMainView()
        {
            InitializeComponent();
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.NavigateToAppSelect();
        }
    }
}
