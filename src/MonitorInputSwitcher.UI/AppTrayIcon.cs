using Neptuo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonitorInputSwitcher
{
    public class AppTrayIcon : IDisposable
    {
        private readonly App app;
        private NotifyIcon? trayIcon;

        public AppTrayIcon(App app)
        {
            Ensure.NotNull(app, "app");
            this.app = app;
        }

        public void Show()
        {
            trayIcon = new NotifyIcon();
            trayIcon.Icon = Icon.ExtractAssociatedIcon(Process.GetCurrentProcess().MainModule.FileName);
            trayIcon.Visible = true;

            trayIcon.MouseClick += OnIconClick;

            trayIcon.ContextMenuStrip = new ContextMenuStrip();
            trayIcon.ContextMenuStrip.Items.Add("Exit").Click += (sender, e) => app.Shutdown();
        }

        private void OnIconClick(object? sender, MouseEventArgs e)
        {

        }

        public void Dispose()
        {
            trayIcon?.Dispose();
        }
    }
}
