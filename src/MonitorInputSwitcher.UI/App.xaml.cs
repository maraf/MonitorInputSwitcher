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

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            trayIcon = new AppTrayIcon(this);
            trayIcon.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            trayIcon.Dispose();

            base.OnExit(e);
        }
    }
}
