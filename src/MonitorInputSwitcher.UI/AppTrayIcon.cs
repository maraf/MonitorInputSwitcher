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
        private readonly MonitorService service;
        private NotifyIcon? trayIcon;

        public AppTrayIcon(App app, MonitorService service)
        {
            Ensure.NotNull(app, "app");
            Ensure.NotNull(service, "service");
            this.app = app;
            this.service = service;
        }

        public void Show()
        {
            trayIcon = new NotifyIcon();
            trayIcon.Icon = Icon.ExtractAssociatedIcon(Process.GetCurrentProcess().MainModule.FileName);
            trayIcon.Visible = true;

            trayIcon.MouseClick += OnIconClick;

            trayIcon.ContextMenuStrip = new ContextMenuStrip();
            trayIcon.ContextMenuStrip.Items.Add("Other").Click += (sender, e) => service.SwitchAllToOther();
            trayIcon.ContextMenuStrip.Items.Add("This").Click += (sender, e) => service.SwitchAllToThis();
            trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
            trayIcon.ContextMenuStrip.Items.Add("Exit").Click += (sender, e) => app.Shutdown();
        }

        private void OnIconClick(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                service.SwitchAllToOther();
        }

        public void Dispose()
        {
            trayIcon?.Dispose();
        }
    }
}
