using Neptuo.Windows.HotKeys;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

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
        private HotkeyCollectionBase? hotkeys;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            service = new MonitorService();

            trayIcon = new AppTrayIcon(this, service);
            trayIcon.Show();
            RegisterHotkeys();
        }

        private void RegisterHotkeys()
        {
            hotkeys.Add(Key.H, ModifierKeys.Windows | ModifierKeys.Alt, (k, m) => service.SwitchAllToThis());
            hotkeys.Add(Key.O, ModifierKeys.Windows | ModifierKeys.Alt, (k, m) => service.SwitchAllToOther());
        }

        protected override void OnExit(ExitEventArgs e)
        {
            hotkeys.Dispose();
            trayIcon.Dispose();

            base.OnExit(e);
        }
    }
}
