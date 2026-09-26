using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using TapeKhashDesktop.Data;

namespace TapeKhashDesktop.Views.Khash
{
    public partial class PaymentsControl : UserControl
    {
        private readonly ObservableCollection<string> _items = new ObservableCollection<string>();

        public PaymentsControl()
        {
            InitializeComponent();
            List.ItemsSource = _items;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var res = await ApiClient.Call("getActiveEmployeeNames");
                EmployeeCombo.Items.Clear();
                foreach (var n in (JArray)res["names"]) EmployeeCombo.Items.Add(n.ToString());
                if (EmployeeCombo.Items.Count > 0) EmployeeCombo.SelectedIndex = 0;
            }
            catch { }
            await LoadPayments();
        }

        private async System.Threading.Tasks.Task LoadPayments()
        {
            try
            {
                var res = await ApiClient.Call("getPayments");
                _items.Clear();
                foreach (var item in (JArray)res["payments"])
                {
                    var p = (JObject)item;
                    _items.Add($"{p.Value<string>("employee_name")} — {EmployeesControl.Fmt(p.Value<double>("amount"))} — {p.Value<string>("jalali_date")}");
                }
            }
            catch { }
        }

        private async void OnSubmitClick(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = ""; MessageText.Text = "";
            if (EmployeeCombo.SelectedItem == null || string.IsNullOrWhiteSpace(AmountBox.Text))
            {
                ErrorText.Text = "نیرو و مبلغ را وارد کنید.";
                return;
            }
            try
            {
                var body = new JObject
                {
                    ["employeeName"] = EmployeeCombo.SelectedItem.ToString(),
                    ["amount"] = AmountBox.Text,
                    ["description"] = DescriptionBox.Text ?? "",
                };
                if (!string.IsNullOrWhiteSpace(DateBox.Text)) body["jalaliDate"] = DateBox.Text.Trim();

                var res = await ApiClient.Call("createPayment", body);
                MessageText.Text = "پرداخت ثبت شد. مانده جدید: " + EmployeesControl.Fmt(res.Value<double>("balance"));
                AmountBox.Text = ""; DescriptionBox.Text = ""; DateBox.Text = "";
                await LoadPayments();
            }
            catch (ApiException ex) { ErrorText.Text = ex.Message; }
        }
    }
}
