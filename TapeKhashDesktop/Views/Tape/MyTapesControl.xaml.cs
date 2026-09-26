using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using TapeKhashDesktop.Data;

namespace TapeKhashDesktop.Views.Tape
{
    public partial class MyTapesControl : UserControl
    {
        private readonly ObservableCollection<TapeRow> _rows = new ObservableCollection<TapeRow>();

        public MyTapesControl()
        {
            InitializeComponent();
            ResultsList.ItemsSource = _rows;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e) => await Load();

        private async System.Threading.Tasks.Task Load()
        {
            ErrorText.Text = "";
            try
            {
                var res = await ApiClient.Call("getMyTapes");
                _rows.Clear();
                foreach (var item in (JArray)res["tapes"]) _rows.Add(TapeRow.FromJson((JObject)item));
                TitleText.Text = $"تپه‌های در اختیار شما ({_rows.Count})";
            }
            catch (ApiException ex) { ErrorText.Text = ex.Message; }
        }

        private async void OnReturnClick(object sender, RoutedEventArgs e)
        {
            var row = (TapeRow)((FrameworkElement)sender).DataContext;
            try
            {
                await ApiClient.Call("returnTape", new JObject { ["id"] = row.Id });
                await Load();
            }
            catch (ApiException ex) { ErrorText.Text = ex.Message; }
        }
    }
}
