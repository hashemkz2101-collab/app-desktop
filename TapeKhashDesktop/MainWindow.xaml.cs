using System.Windows;
using System.Windows.Controls;
using TapeKhashDesktop.Data;
using TapeKhashDesktop.Views;

namespace TapeKhashDesktop
{
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            Session.Load();

            if (Session.IsLoggedIn)
                NavigateToAppSelect();
            else
                NavigateToLogin();
        }

        public void Navigate(UserControl view)
        {
            RootContent.Content = view;
        }

        public void NavigateToLogin()
        {
            Navigate(new LoginView());
        }

        public void NavigateToAppSelect()
        {
            Navigate(new AppSelectView());
        }

        public void NavigateToTape()
        {
            Navigate(new Tape.TapeMainView());
        }

        public void NavigateToKhash()
        {
            Navigate(new Khash.KhashMainView());
        }
    }
}
