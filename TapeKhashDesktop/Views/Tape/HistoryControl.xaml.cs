using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using TapeKhashDesktop.Data;

namespace TapeKhashDesktop.Views.Tape
{
    public class HistoryItem
    {
        public string Title { get; set; }
        public string Sub { get; set; }
        public string Notes { get; set; }
    }

    public partial class HistoryControl : UserControl
    {
        private readonly ObservableCollection<HistoryItem> _items = new ObservableCollection<HistoryItem>();

        public HistoryControl()
        {
            InitializeComponent();
            List.ItemsSource = _items;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e) => await Load();
        private async void OnFilterClick(object sender, RoutedEventArgs e) => await Load();

        private async System.Threading.Tasks.Task Load()
        {
            try
            {
                var body = new JObject();
                if (!string.IsNullOrWhiteSpace(QueryBox.Text)) body["tapeQuery"] = QueryBox.Text.Trim();
                var res = await ApiClient.Call("getHistory", body);
                _items.Clear();
                foreach (var item in (JArray)res["history"])
                {
                    var h = (JObject)item;
                    _items.Add(new HistoryItem
                    {
                        Title = $"{h.Value<string>("action")} — {h.Value<string>("tape_code")}",
                        Sub = $"{h.Value<string>("display_name")} · {h.Value<string>("event_time")}",
                        Notes = h.Value<string>("notes"),
                    });
                }
            }
            catch { }
        }
    }
}
