using MonitorInputSwitcher.Views;
using Neptuo.Windows.HotKeys;
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
        public const string Title = "Monitor Input Switcher";

        private ShortcutService shortcuts;
        private AppTrayIcon trayIcon;
        private MonitorService service;
        private MonitorListModel model;
        private HotkeyCollectionBase hotkeys;

        public App()
        {
            shortcuts = new ShortcutService(
               companyName: "Neptuo",
               suiteName: "Desktop Utils",
               productName: "Monitor Input Switcher"
            );

            hotkeys = new ComponentDispatcherHotkeyCollection();
            service = new MonitorService();
            model = new MonitorListModel(service);
            trayIcon = new AppTrayIcon(this, model, () => new HelpWindow(service, shortcuts, model));
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

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
            hotkeys?.Dispose();
            trayIcon?.Dispose();

            base.OnExit(e);
        }
    }
}
