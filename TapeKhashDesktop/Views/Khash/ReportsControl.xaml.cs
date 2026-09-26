using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using TapeKhashDesktop.Data;

namespace TapeKhashDesktop.Views.Khash
{
    public class DateReportItem
    {
        public string Date { get; set; }
        public string Summary { get; set; }
    }

    public partial class ReportsControl : UserControl
    {
        private readonly ObservableCollection<DateReportItem> _byDate = new ObservableCollection<DateReportItem>();

        public ReportsControl()
        {
            InitializeComponent();
            ByDateList.ItemsSource = _byDate;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var f = await ApiClient.Call("getFinanceDashboard");
                var d = await ApiClient.Call("getKhashDashboard");
                TodayPanel.Children.Add(new TextBlock { Text = $"تعداد فاکتور امروز: {f.Value<int>("todayInvoiceCount")}" });
                TodayPanel.Children.Add(new TextBlock { Text = $"مبلغ کل امروز: {EmployeesControl.Fmt(f.Value<double>("todayInvoiceAmount"))}" });
                TodayPanel.Children.Add(new TextBlock { Text = $"سهم نیروها امروز: {EmployeesControl.Fmt(f.Value<double>("todayEmployeeShare"))}" });
                TodayPanel.Children.Add(new TextBlock { Text = $"پرداختی امروز: {EmployeesControl.Fmt(f.Value<double>("todayPaymentAmount"))}" });
                TodayPanel.Children.Add(new TextBlock { Text = $"سفارشات امروز: {d.Value<int>("todayCount")}" });
                TodayPanel.Children.Add(new TextBlock { Text = $"منتظر چاپ: {d.Value<int>("pendingPrintCount")}" });
            }
            catch { }

            try
            {
                var s = await ApiClient.Call("getKhashSummaryReport");
                SummaryPanel.Children.Add(new TextBlock { Text = $"کل: {s.Value<int>("total")}" });
                SummaryPanel.Children.Add(new TextBlock { Text = $"ارسال از خاش: {s.Value<int>("sentFromKhash")}" });
                SummaryPanel.Children.Add(new TextBlock { Text = $"دریافت از خاش: {s.Value<int>("receivedFromKhash")}" });
                SummaryPanel.Children.Add(new TextBlock { Text = $"چاپ‌شده: {s.Value<int>("printedIranshahr")}" });
                SummaryPanel.Children.Add(new TextBlock { Text = $"ارسال به خاش: {s.Value<int>("sentBackToKhash")}" });
            }
            catch { }

            try
            {
                var res = await ApiClient.Call("getKhashReportByDate");
                foreach (var item in (JArray)res["report"])
                {
                    var g = (JObject)item;
                    _byDate.Add(new DateReportItem
                    {
                        Date = g.Value<string>("date"),
                        Summary = $"کل: {g.Value<int>("total")} — چاپ‌شده: {g.Value<int>("printedIranshahr")} — ارسال به خاش: {g.Value<int>("sentBackToKhash")}"
                    });
                }
            }
            catch { }
        }
    }
}
