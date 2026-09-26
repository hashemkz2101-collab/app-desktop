using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using TapeKhashDesktop.Data;

namespace TapeKhashDesktop.Views.Tape
{
    public partial class ActiveUsersControl : UserControl
    {
        private readonly ObservableCollection<string> _items = new ObservableCollection<string>();

        public ActiveUsersControl()
        {
            InitializeComponent();
            List.ItemsSource = _items;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var res = await ApiClient.Call("getUsersStatus");
                foreach (var item in (JArray)res["users"])
                {
                    var u = (JObject)item;
                    _items.Add($"{u.Value<string>("displayName")} — {u.Value<int>("count")} تپه فعال");
                }
            }
            catch { }
        }
    }
}
