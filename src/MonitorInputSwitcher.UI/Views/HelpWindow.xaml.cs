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

        public HelpWindow(AppTrayIcon trayIcon, MonitorService service)
        {
            this.trayIcon = trayIcon;
            this.service = service;

            InitializeComponent();

            tblVersion.Text = GetType().Assembly.GetName().Version.ToString(3);
            tbxSettingsPath.Text = service.GetSettingsPath();
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
    }
}
