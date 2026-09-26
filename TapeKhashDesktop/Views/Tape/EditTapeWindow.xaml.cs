using System.Windows;
using Newtonsoft.Json.Linq;
using TapeKhashDesktop.Data;

namespace TapeKhashDesktop.Views.Tape
{
    public partial class EditTapeWindow : Window
    {
        private readonly TapeRow _row;
        public bool Saved { get; private set; }

        public EditTapeWindow(TapeRow row)
        {
            InitializeComponent();
            _row = row;
            BoxCodeBox.Text = row.Box;
            TapeCodeBox.Text = row.Tape;
        }

        private async void OnSaveClick(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = "";
            try
            {
                await ApiClient.Call("updateTape", new JObject
                {
                    ["id"] = _row.Id,
                    ["boxCode"] = BoxCodeBox.Text.Trim(),
                    ["tapeCode"] = TapeCodeBox.Text.Trim(),
                    ["imageCode"] = TapeCodeBox.Text.Trim(),
                });
                Saved = true;
                Close();
            }
            catch (ApiException ex) { ErrorText.Text = ex.Message; }
        }

        private void OnCancelClick(object sender, RoutedEventArgs e) => Close();
    }
}
