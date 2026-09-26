using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using TapeKhashDesktop.Data;

namespace TapeKhashDesktop.Views.Khash
{
    public class InvoiceItem
    {
        public string Number { get; set; }
        public string Summary { get; set; }
    }

    public partial class InvoicesControl : UserControl
    {
        private readonly ObservableCollection<InvoiceItem> _items = new ObservableCollection<InvoiceItem>();

        public InvoicesControl()
        {
            InitializeComponent();
            InvoicesList.ItemsSource = _items;
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
            await LoadInvoices();
        }

        private async System.Threading.Tasks.Task LoadInvoices()
        {
            try
            {
                var body = new JObject();
                if (!string.IsNullOrWhiteSpace(QueryBox.Text)) body["query"] = QueryBox.Text.Trim();
                var res = await ApiClient.Call("getInvoices", body);
                _items.Clear();
                foreach (var item in (JArray)res["invoices"])
                {
                    var i = (JObject)item;
                    _items.Add(new InvoiceItem
                    {
                        Number = i.Value<string>("full_invoice_no"),
                        Summary = $"مبلغ: {EmployeesControl.Fmt(i.Value<double>("total_amount"))} — سهم: {EmployeesControl.Fmt(i.Value<double>("employee_share"))} — نیرو: {i.Value<string>("employee_name")} — تاریخ: {i.Value<string>("jalali_date")}"
                    });
                }
            }
            catch { }
        }

        private async void OnNumberPartChanged(object sender, TextChangedEventArgs e)
        {
            PreviewText.Text = "شماره کامل: —";
            if (string.IsNullOrWhiteSpace(CodeBox.Text) || string.IsNullOrWhiteSpace(RowBox.Text) || string.IsNullOrWhiteSpace(SimilarBox.Text)) return;
            try
            {
                var res = await ApiClient.Call("buildInvoiceNumberPreview", new JObject
                {
                    ["code"] = CodeBox.Text, ["row"] = RowBox.Text, ["similar"] = SimilarBox.Text
                });
                PreviewText.Text = "شماره کامل: " + res.Value<string>("fullNumber");
            }
            catch { }
        }

        private async void OnSuggestClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CodeBox.Text)) return;
            try
            {
                var res = await ApiClient.Call("suggestNextInvoiceNumber", new JObject { ["code"] = CodeBox.Text });
                RowBox.Text = res.Value<int>("row").ToString();
                SimilarBox.Text = res.Value<int>("similar").ToString();
            }
            catch { }
        }

        private async void OnSubmitClick(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = ""; MessageText.Text = "";
            if (string.IsNullOrWhiteSpace(CodeBox.Text) || string.IsNullOrWhiteSpace(RowBox.Text) ||
                string.IsNullOrWhiteSpace(SimilarBox.Text) || string.IsNullOrWhiteSpace(AmountBox.Text) ||
                EmployeeCombo.SelectedItem == null)
            {
                ErrorText.Text = "همه‌ی فیلدهای الزامی را پر کنید.";
                return;
            }
            try
            {
                var body = new JObject
                {
                    ["code"] = CodeBox.Text,
                    ["row"] = RowBox.Text,
                    ["similar"] = SimilarBox.Text,
                    ["amount"] = AmountBox.Text,
                    ["employeeName"] = EmployeeCombo.SelectedItem.ToString(),
                    ["description"] = DescriptionBox.Text ?? "",
                };
                if (!string.IsNullOrWhiteSpace(DateBox.Text)) body["jalaliDate"] = DateBox.Text.Trim();

                var res = await ApiClient.Call("createInvoice", body);
                MessageText.Text = $"فاکتور {res.Value<string>("fullNumber")} ثبت شد. سهم نیرو: {EmployeesControl.Fmt(res.Value<double>("share"))}";
                CodeBox.Text = ""; RowBox.Text = ""; SimilarBox.Text = ""; AmountBox.Text = ""; DescriptionBox.Text = ""; DateBox.Text = "";
                await LoadInvoices();
            }
            catch (ApiException ex) { ErrorText.Text = ex.Message; }
        }

        private async void OnSearchClick(object sender, RoutedEventArgs e) => await LoadInvoices();
    }
}
