using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using TapeKhashDesktop.Data;

namespace TapeKhashDesktop.Views.Tape
{
    public partial class SearchTapeControl : UserControl
    {
        private readonly ObservableCollection<TapeRow> _results = new ObservableCollection<TapeRow>();

        public SearchTapeControl()
        {
            InitializeComponent();
            ResultsList.ItemsSource = _results;
        }

        private void QueryBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) _ = Search();
        }

        private async void OnSearchClick(object sender, RoutedEventArgs e) => await Search();

        private async System.Threading.Tasks.Task Search()
        {
            ErrorText.Text = ""; MessageText.Text = "";
            try
            {
                var res = await ApiClient.Call("searchData", new JObject { ["query"] = QueryBox.Text });
                _results.Clear();
                foreach (var item in (JArray)res["results"])
                    _results.Add(TapeRow.FromJson((JObject)item));
            }
            catch (ApiException ex) { ErrorText.Text = ex.Message; }
            catch { ErrorText.Text = "خطا در ارتباط با سرور."; }
        }

        private async void OnTakeClick(object sender, RoutedEventArgs e)
        {
            var row = (TapeRow)((FrameworkElement)sender).DataContext;
            try
            {
                await ApiClient.Call("takeTape", new JObject { ["id"] = row.Id });
                MessageText.Text = $"تپه «{row.Tape}» برداشته شد.";
                await Search();
            }
            catch (ApiException ex) { ErrorText.Text = ex.Message; }
        }

        private async void OnReturnClick(object sender, RoutedEventArgs e)
        {
            var row = (TapeRow)((FrameworkElement)sender).DataContext;
            try
            {
                await ApiClient.Call("returnTape", new JObject { ["id"] = row.Id });
                MessageText.Text = $"تپه «{row.Tape}» برگشت داده شد.";
                await Search();
            }
            catch (ApiException ex) { ErrorText.Text = ex.Message; }
        }
    }
}
