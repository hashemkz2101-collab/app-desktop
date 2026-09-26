using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using TapeKhashDesktop.Data;

namespace TapeKhashDesktop.Views.Tape
{
    public partial class EditTapeControl : UserControl
    {
        private readonly ObservableCollection<TapeRow> _rows = new ObservableCollection<TapeRow>();

        public EditTapeControl()
        {
            InitializeComponent();
            List.ItemsSource = _rows;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e) => await Search();
        private async void OnSearchClick(object sender, RoutedEventArgs e) => await Search();

        private async System.Threading.Tasks.Task Search()
        {
            ErrorText.Text = "";
            try
            {
                var res = await ApiClient.Call("searchAdminTapes", new JObject { ["query"] = QueryBox.Text });
                _rows.Clear();
                foreach (var item in (JArray)res["results"]) _rows.Add(TapeRow.FromJson((JObject)item));
            }
            catch (ApiException ex) { ErrorText.Text = ex.Message; }
        }

        private async void OnEditClick(object sender, RoutedEventArgs e)
        {
            var row = (TapeRow)((FrameworkElement)sender).DataContext;
            var dlg = new EditTapeWindow(row) { Owner = Window.GetWindow(this) };
            dlg.ShowDialog();
            if (dlg.Saved) await Search();
        }
    }
}
