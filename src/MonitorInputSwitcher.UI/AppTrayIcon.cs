using MonitorInputSwitcher.Views;
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
        private readonly MonitorListModel model;
        private readonly Func<HelpWindow> helpFactory;
        private NotifyIcon? trayIcon;
        private HelpWindow? help;

        public AppTrayIcon(App app, MonitorListModel model, Func<HelpWindow> helpFactory)
        {
            this.app = app;
            this.model = model;
            this.helpFactory = helpFactory;
        }

        public void Show()
        {
            trayIcon = new NotifyIcon();
            trayIcon.Text = App.Title;
            trayIcon.Icon = Icon.ExtractAssociatedIcon(Process.GetCurrentProcess().MainModule!.FileName);
            trayIcon.Visible = true;

            trayIcon.MouseClick += OnIconClick;

            model.OnReload += BuildContextMenu;

            BuildContextMenu();
        }

        public void Reload()
            => BuildContextMenu();

        private void BuildContextMenu()
        {
            trayIcon!.ContextMenuStrip = new ContextMenuStrip();

            if (!String.IsNullOrEmpty(model.OtherName))
                AddItem(trayIcon.ContextMenuStrip.Items, $"Other ({model.OtherName})", () => model.SwitchAllToOther.Execute());

            if (!String.IsNullOrEmpty(model.ThisName))
                AddItem(trayIcon.ContextMenuStrip.Items, $"This ({model.ThisName})", () => model.SwitchAllToThis.Execute());

            AddSeparator(trayIcon.ContextMenuStrip);

            foreach (var monitor in model.Monitors)
            {
                var monitorGroup = new ToolStripMenuItem(monitor.Name);

                if (monitor.SwitchToOther.CanExecute())
                    AddItem(monitorGroup.DropDownItems, $"Other ({model.OtherName})", () => monitor.SwitchToOther.Execute());

                if (monitor.SwitchToThis.CanExecute())
                    AddItem(monitorGroup.DropDownItems, $"This ({model.ThisName})", () => monitor.SwitchToThis.Execute());

                if (monitorGroup.DropDownItems.Count > 0)
                    trayIcon.ContextMenuStrip.Items.Add(monitorGroup);
            }

            AddSeparator(trayIcon.ContextMenuStrip);

            AddItem(trayIcon.ContextMenuStrip.Items, "Help", OnOpenHelp);
            AddItem(trayIcon.ContextMenuStrip.Items, "Exit", () => app.Shutdown());
        }

        private void OnOpenHelp()
        {
            if (help == null)
            {
                help = helpFactory();
                help.Closed += (s, e) => help = null;
            }

            help.Activate();
            help.Show();
        }

        private static void AddItem(ToolStripItemCollection items, string title, Action handler)
        {
            items.Add(title).Click += (sender, e) => handler();
        }

        private static void AddSeparator(ContextMenuStrip contextMenuStrip)
        {
            if (contextMenuStrip.Items.Count > 0)
                contextMenuStrip.Items.Add(new ToolStripSeparator());
        }

        private void OnIconClick(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                model.SwitchAllToOther.Execute();
            else if (e.Button == MouseButtons.Middle)
                model.SwitchAllToThis.Execute();
        }

        public void Dispose()
        {
            trayIcon?.Dispose();
            model.OnReload -= BuildContextMenu;
        }
    }
}
