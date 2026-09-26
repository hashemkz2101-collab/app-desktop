using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using TapeKhashDesktop.Data;

namespace TapeKhashDesktop.Views.Tape
{
    public partial class UsersManagementControl : UserControl
    {
        public UsersManagementControl()
        {
            InitializeComponent();
        }

        private async void OnLoaded(object sender, RoutedEventArgs e) => await Load();

        private async System.Threading.Tasks.Task Load()
        {
            ErrorText.Text = "";
            UsersPanel.Children.Clear();
            try
            {
                var res = await ApiClient.Call("getUsers");
                foreach (var item in (JArray)res["users"])
                    UsersPanel.Children.Add(BuildUserRow((JObject)item));
            }
            catch (ApiException ex) { ErrorText.Text = ex.Message; }
        }

        private UIElement BuildUserRow(JObject u)
        {
            int id = u.Value<int>("id");
            var apps = (u["apps"] as JArray) ?? new JArray();
            bool hasTape = false, hasKhash = false;
            foreach (var a in apps)
            {
                if (a.ToString() == "tape") hasTape = true;
                if (a.ToString() == "khash") hasKhash = true;
            }

            var border = new Border { BorderBrush = System.Windows.Media.Brushes.LightGray, BorderThickness = new Thickness(1), Padding = new Thickness(10), Margin = new Thickness(0, 4, 0, 0) };
            var stack = new StackPanel();

            var title = new TextBlock
            {
                Text = $"{u.Value<string>("display_name")} ({u.Value<string>("username")}) — {(u.Value<string>("role") == "admin" ? "مدیر" : "کاربر عادی")}",
                FontWeight = FontWeights.Bold
            };
            stack.Children.Add(title);

            var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 6, 0, 0) };

            var tapeCheck = new CheckBox { Content = "تپه‌ها", IsChecked = hasTape, Margin = new Thickness(0, 0, 12, 0) };
            var khashCheck = new CheckBox { Content = "خاش", IsChecked = hasKhash, Margin = new Thickness(0, 0, 12, 0) };
            var activeToggle = new CheckBox { Content = "فعال", IsChecked = u.Value<bool?>("is_active") ?? true };

            async void Update()
            {
                var newApps = new JArray();
                if (tapeCheck.IsChecked == true) newApps.Add("tape");
                if (khashCheck.IsChecked == true) newApps.Add("khash");
                try
                {
                    await ApiClient.Call("updateUser", new JObject
                    {
                        ["id"] = id,
                        ["is_active"] = activeToggle.IsChecked == true,
                        ["apps"] = newApps,
                    });
                }
                catch (ApiException ex) { ErrorText.Text = ex.Message; }
            }

            tapeCheck.Checked += (s, e) => Update();
            tapeCheck.Unchecked += (s, e) => Update();
            khashCheck.Checked += (s, e) => Update();
            khashCheck.Unchecked += (s, e) => Update();
            activeToggle.Checked += (s, e) => Update();
            activeToggle.Unchecked += (s, e) => Update();

            row.Children.Add(tapeCheck);
            row.Children.Add(khashCheck);
            row.Children.Add(activeToggle);
            stack.Children.Add(row);
            border.Child = stack;
            return border;
        }

        private async void OnAddUserClick(object sender, RoutedEventArgs e)
        {
            var dlg = new AddUserWindow { Owner = Window.GetWindow(this) };
            dlg.ShowDialog();
            if (dlg.Created) await Load();
        }
    }
}
