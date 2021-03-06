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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MonitorInputSwitcher.Views
{
    public partial class HelpWindow : Window
    {
        private readonly AppTrayIcon trayIcon;
        private readonly MonitorService service;
        private readonly ShortcutService shortcuts;

        public HelpWindow(AppTrayIcon trayIcon, MonitorService service, ShortcutService shortcuts)
        {
            this.trayIcon = trayIcon;
            this.service = service;
            this.shortcuts = shortcuts;

            InitializeComponent();

            tblVersion.Text = GetVersion();
            tbxSettingsPath.Text = service.GetSettingsPath();
            cbxAutoStart.IsChecked = shortcuts.Exists(Environment.SpecialFolder.Startup);
            cbxAutoStart.Checked += cbxAutoStart_Changed;
            cbxAutoStart.Unchecked += cbxAutoStart_Changed;

            Dictionary<string, string> currentValues = new Dictionary<string, string>();
            for (int i = 0; i < service.GetMonitorCount(); i++)
                currentValues[$"Monitor {i + 1}"] = service.FindCurrentInput(i)?.ToString() ?? "Unknown";

            itcCurrentValues.ItemsSource = currentValues;
        }

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
            => trayIcon.Reload();

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
    }
}
