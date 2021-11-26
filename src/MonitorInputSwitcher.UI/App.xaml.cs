using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None,
    ResourceDictionaryLocation.SourceAssembly
)]

namespace MonitorInputSwitcher
{
    public partial class App : Application
    {
        private AppTrayIcon? trayIcon;
        private MonitorService? service;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            service = new MonitorService();

            trayIcon = new AppTrayIcon(this, service);
            trayIcon.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            trayIcon.Dispose();

            base.OnExit(e);
        }
    }
}
