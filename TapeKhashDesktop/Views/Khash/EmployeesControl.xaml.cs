using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using TapeKhashDesktop.Data;

namespace TapeKhashDesktop.Views.Khash
{
    public class EmployeeItem
    {
        public string Name { get; set; }
        public string Summary { get; set; }
    }

    public partial class EmployeesControl : UserControl
    {
        private readonly ObservableCollection<EmployeeItem> _items = new ObservableCollection<EmployeeItem>();

        public EmployeesControl()
        {
            InitializeComponent();
            List.ItemsSource = _items;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e) => await Load();

        private async System.Threading.Tasks.Task Load()
        {
            ErrorText.Text = "";
            try
            {
                var res = await ApiClient.Call("getAllEmployeeBalances");
                _items.Clear();
                foreach (var item in (JArray)res["balances"])
                {
                    var b = (JObject)item;
                    _items.Add(new EmployeeItem
                    {
                        Name = b.Value<string>("name"),
                        Summary = $"سهم کل: {Fmt(b.Value<double>("totalShare"))} — پرداختی: {Fmt(b.Value<double>("totalPaid"))} — مانده: {Fmt(b.Value<double>("balance"))}"
                    });
                }
            }
            catch (ApiException ex) { ErrorText.Text = ex.Message; }
        }

        private async void OnAddClick(object sender, RoutedEventArgs e)
        {
            var name = NewNameBox.Text.Trim();
            if (string.IsNullOrEmpty(name)) return;
            try
            {
                await ApiClient.Call("addEmployee", new JObject { ["name"] = name });
                NewNameBox.Text = "";
                await Load();
            }
            catch (ApiException ex) { ErrorText.Text = ex.Message; }
        }

        public static string Fmt(double n) => n.ToString("N0", CultureInfo.InvariantCulture);
    }
}
