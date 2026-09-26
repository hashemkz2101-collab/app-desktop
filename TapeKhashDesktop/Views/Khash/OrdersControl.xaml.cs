using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Newtonsoft.Json.Linq;
using TapeKhashDesktop.Data;

namespace TapeKhashDesktop.Views.Khash
{
    public partial class OrdersControl : UserControl
    {
        private static readonly (string field, string label)[] Steps =
        {
            ("sent_from_khash", "ارسال از خاش"),
            ("received_from_khash", "دریافت از خاش"),
            ("printed_iranshahr", "چاپ در ایرانشهر"),
            ("sent_iranshahr_to_khash", "ارسال به خاش"),
        };

        public OrdersControl()
        {
            InitializeComponent();
        }

        private async void OnLoaded(object sender, RoutedEventArgs e) => await LoadOrders();

        private string CurrentFilter()
        {
            if (FilterPendingReceive.IsChecked == true) return "pending_receive";
            if (FilterPendingPrint.IsChecked == true) return "pending_print";
            if (FilterPendingSendBack.IsChecked == true) return "pending_send_back";
            if (FilterCompleted.IsChecked == true) return "completed";
            return "all";
        }

        private async void OnFilterChanged(object sender, RoutedEventArgs e) => await LoadOrders();

        private async System.Threading.Tasks.Task LoadOrders()
        {
            OrdersPanel.Children.Clear();
            try
            {
                var res = await ApiClient.Call("getKhashOrders", new JObject { ["statusFilter"] = CurrentFilter() });
                foreach (var item in (JArray)res["orders"])
                    OrdersPanel.Children.Add(BuildOrderRow((JObject)item));
            }
            catch (ApiException ex) { ErrorText.Text = ex.Message; }
        }

        private UIElement BuildOrderRow(JObject o)
        {
            var orderNo = o.Value<string>("order_no");
            var border = new Border { BorderBrush = Brushes.LightGray, BorderThickness = new Thickness(1), Padding = new Thickness(10), Margin = new Thickness(0, 4, 0, 0) };
            var stack = new StackPanel();
            stack.Children.Add(new TextBlock { Text = orderNo, FontWeight = FontWeights.Bold });
            stack.Children.Add(new TextBlock { Text = "تاریخ: " + o.Value<string>("jalali_date"), Foreground = Brushes.Gray, FontSize = 11 });

            var stepsPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 6, 0, 0) };
            foreach (var (field, label) in Steps)
            {
                bool current = o.Value<bool?>(field) ?? false;
                var toggle = new ToggleButton_
                {
                    Content = label,
                    IsChecked = current,
                    Margin = new Thickness(0, 0, 6, 0),
                };
                var localField = field;
                toggle.Click += async (s, e) =>
                {
                    try
                    {
                        await ApiClient.Call("updateKhashStatus", new JObject
                        {
                            ["orderNo"] = orderNo,
                            ["field"] = localField,
                            ["value"] = !current,
                        });
                        await LoadOrders();
                    }
                    catch (ApiException ex) { ErrorText.Text = ex.Message; }
                };
                stepsPanel.Children.Add(toggle);
            }
            stack.Children.Add(stepsPanel);
            border.Child = stack;
            return border;
        }

        private async void OnNumberPartChanged(object sender, TextChangedEventArgs e)
        {
            PreviewText.Text = "شماره: —";
            if (string.IsNullOrWhiteSpace(CodeBox.Text) || string.IsNullOrWhiteSpace(RowBox.Text) || string.IsNullOrWhiteSpace(SimilarBox.Text)) return;
            try
            {
                var res = await ApiClient.Call("buildKhashNumberPreview", new JObject
                {
                    ["code"] = CodeBox.Text, ["row"] = RowBox.Text, ["similar"] = SimilarBox.Text
                });
                PreviewText.Text = "شماره: " + res.Value<string>("fullNumber");
            }
            catch { }
        }

        private async void OnSuggestClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CodeBox.Text)) return;
            try
            {
                var res = await ApiClient.Call("suggestNextKhashNumber", new JObject { ["code"] = CodeBox.Text });
                RowBox.Text = res.Value<int>("row").ToString();
                SimilarBox.Text = res.Value<int>("similar").ToString();
            }
            catch { }
        }

        private async void OnSubmitClick(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = ""; MessageText.Text = "";
            if (string.IsNullOrWhiteSpace(CodeBox.Text) || string.IsNullOrWhiteSpace(RowBox.Text) || string.IsNullOrWhiteSpace(SimilarBox.Text))
            {
                ErrorText.Text = "کد، ردیف و شماره مشابه را وارد کنید.";
                return;
            }
            try
            {
                var res = await ApiClient.Call("createKhashOrder", new JObject
                {
                    ["code"] = CodeBox.Text, ["row"] = RowBox.Text, ["similar"] = SimilarBox.Text
                });
                MessageText.Text = "سفارش " + res.Value<string>("fullNumber") + " ثبت شد.";
                CodeBox.Text = ""; RowBox.Text = ""; SimilarBox.Text = "";
                await LoadOrders();
            }
            catch (ApiException ex) { ErrorText.Text = ex.Message; }
        }
    }

    /// <summary>یک ToggleButton ساده با ظاهر Chip (سبز وقتی فعال است) برای مراحل سفارش</summary>
    public class ToggleButton_ : System.Windows.Controls.Primitives.ToggleButton
    {
        public ToggleButton_()
        {
            Padding = new Thickness(10, 4, 10, 4);
            Checked += (s, e) => Background = Brushes.LightGreen;
            Unchecked += (s, e) => Background = Brushes.WhiteSmoke;
            Background = (IsChecked == true) ? Brushes.LightGreen : Brushes.WhiteSmoke;
        }
    }
}
