using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MonitorInputSwitcher.Views
{
    public partial class HelpWindow : Window
    {
        private readonly MonitorService service;
        private readonly ShortcutService shortcuts;
        private readonly MonitorListModel model;

        public HelpWindow(MonitorService service, ShortcutService shortcuts, MonitorListModel model)
        {
            this.service = service;
            this.shortcuts = shortcuts;
            this.model = model;

            InitializeComponent();

            tblVersion.Text = GetVersion();
            tbxSettingsPath.Text = service.GetSettingsPath();
            cbxAutoStart.IsChecked = shortcuts.Exists(Environment.SpecialFolder.Startup);
            cbxAutoStart.Checked += cbxAutoStart_Changed;
            cbxAutoStart.Unchecked += cbxAutoStart_Changed;

            Dictionary<string, string> currentValues = new Dictionary<string, string>();
            for (int i = 0; i < service.GetMonitorCount(); i++)
            {
                string name = service.GetMonitorName(i);
                string defaultName = service.GetDefaultMonitorName(i);
                if (name != defaultName)
                {
                    name = $"{defaultName} ({name})";
                }
                currentValues[name] = GetCurrentInput(i);
            }

            itcCurrentValues.ItemsSource = currentValues;
        }

        private string GetCurrentInput(int index)
            => service.FindCurrentInput(index)?.ToString() ?? "Unknown";

        private string GetVersion()
        {
            var version = GetType().Assembly.GetName().Version;
            if (version == null)
                return "0.0.0";

            return version.ToString(3);
        }

        private void btnOpenSettings_Click(object sender, RoutedEventArgs e)
        {
            var process = Process.Start(new ProcessStartInfo()
            {
                FileName = service.GetSettingsPath(),
                UseShellExecute = true
            });
        }

        private void btnReloadSettings_Click(object sender, RoutedEventArgs e)
            => model.Reload();

        private void btnOpenGitHub_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = "https://github.com/maraf/MonitorInputSwitcher",
                UseShellExecute = true
            });
        }

        private void cbxAutoStart_Changed(object sender, RoutedEventArgs e)
        {
            if (shortcuts.Exists(Environment.SpecialFolder.Startup))
                shortcuts.Delete(Environment.SpecialFolder.Startup);
            else
                shortcuts.Create(Environment.SpecialFolder.Startup);
        }

        private async void btnIdentify_Click(object sender, RoutedEventArgs e)
        {
            btnIdentify.IsEnabled = false;

            Window[] wnds = new Window[Screen.AllScreens.Length];
            for (int i = 0; i < Screen.AllScreens.Length; i++)
            {
                var screen = Screen.AllScreens[i];

                var size = 200;
                StackPanel content = new StackPanel()
                {
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                string title = service.GetDefaultMonitorName(i);
                content.Children.Add(new TextBlock(new Run(title))
                {
                    FontSize = 24,
                    TextAlignment = TextAlignment.Center
                });
                string name = service.GetMonitorName(i);
                if (title != name)
                {
                    content.Children.Add(new TextBlock(new Run($"\"{name}\""))
                    {
                        FontSize = 20,
                        TextAlignment = TextAlignment.Center
                    });
                }
                content.Children.Add(new TextBlock(new Run($"Input {GetCurrentInput(i)}"))
                {
                    TextAlignment = TextAlignment.Center
                });
                Window wnd = new Window
                {
                    Title = title,
                    WindowStyle = WindowStyle.None,
                    ShowInTaskbar = false,
                    Width = size,
                    Height = size,
                    Left = screen.WorkingArea.Left + (screen.WorkingArea.Width / 2) - size,
                    Top = screen.WorkingArea.Top + (screen.WorkingArea.Height / 2) - size,
                    Content = content
                };
                wnd.Show();
                wnds[i] = wnd;
            }

            await Task.Delay(10 * 1000);

            for (int i = 0;i < wnds.Length; i++)
                wnds[i].Close();

            btnIdentify.IsEnabled = true;
        }
    }
}
