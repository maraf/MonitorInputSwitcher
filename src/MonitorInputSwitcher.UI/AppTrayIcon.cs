using MonitorInputSwitcher.Views;
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
        private HelpWindow? help;

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
            trayIcon.Text = App.Title;
            trayIcon.Icon = Icon.ExtractAssociatedIcon(Process.GetCurrentProcess().MainModule.FileName);
            trayIcon.Visible = true;

            trayIcon.MouseClick += OnIconClick;

            BuildContextMenu();
        }

        public void Reload()
            => BuildContextMenu();

        private void BuildContextMenu()
        {
            trayIcon!.ContextMenuStrip = new ContextMenuStrip();

            string otherName = service.FindOtherName();
            string thisName = service.FindThisName();

            if (!String.IsNullOrEmpty(otherName))
                AddItem(trayIcon.ContextMenuStrip.Items, $"Other ({otherName})", () => service.SwitchAllToOther());

            if (!String.IsNullOrEmpty(thisName))
                AddItem(trayIcon.ContextMenuStrip.Items, $"This ({thisName})", () => service.SwitchAllToThis());

            AddSeparator();

            for (int i = 0; i < service.GetMonitorCount(); i++)
            {
                var index = i;
                var monitorGroup = new ToolStripMenuItem($"Monitor {i + 1}");

                if (service.HasOtherForMonitor(i))
                    AddItem(monitorGroup.DropDownItems, $"Other ({otherName})", () => service.SwitchToOther(index));

                if (service.HasThisForMonitor(i))
                    AddItem(monitorGroup.DropDownItems, $"This ({thisName})", () => service.SwitchToThis(index));

                if (monitorGroup.DropDownItems.Count > 0)
                    trayIcon.ContextMenuStrip.Items.Add(monitorGroup);
            }

            AddSeparator();

            AddItem(trayIcon.ContextMenuStrip.Items, "Help", OnOpenHelp);
            AddItem(trayIcon.ContextMenuStrip.Items, "Exit", () => app.Shutdown());
        }

        private void OnOpenHelp()
        {
            if (help == null)
            {
                help = new HelpWindow(this, service);
                help.Closed += (s, e) => help = null;
            }

            help.Activate();
            help.Show();
        }

        private void AddItem(ToolStripItemCollection items, string title, Action handler)
        {
            items.Add(title).Click += (sender, e) => handler();
        }

        private void AddSeparator()
        {
            if (trayIcon!.ContextMenuStrip.Items.Count > 0)
                trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
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
