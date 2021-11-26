using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MonitorInputSwitcher
{
    public static class Win32
    {
        public static List<IntPtr> GetMonitorHandles()
        {
            var hMonitors = new List<IntPtr>();
            MonitorEnumProc callback = (IntPtr hMonitor, IntPtr hdc, ref Win32.Rect prect, int d) =>
            {
                hMonitors.Add(hMonitor);
                return true;
            };

            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, 0);
            return hMonitors;
        }

        public static bool SetInputType(IntPtr hMonitor, Win32.InputType inputType)
        {
            // get number of physical displays (assume only one for simplicity)
            var success = GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, out uint physicalMonitorCount);
            if (!success)
                return false;

            var physicalMonitorArray = new Win32.PhysicalMonitor[physicalMonitorCount]; //count will be 1 for extended displays and 2(or more) for mirrored displays

            success = GetPhysicalMonitorsFromHMONITOR(hMonitor, physicalMonitorCount, physicalMonitorArray);
            if (!success)
                return false;
            var physicalMonitor = physicalMonitorArray[physicalMonitorArray.Length - 1]; //if count > 1 then we assume the laptop screen is 1st in array and the mirrored monitor is 2nd

            success = SetVCPFeature(physicalMonitor.hPhysicalMonitor, INPUT_SELECT, (int)inputType);
            if (!success)
                return false;

            success = DestroyPhysicalMonitors(physicalMonitorCount, physicalMonitorArray);
            if (!success)
                return false;

            return true;
        }

        public enum InputType
        {
            //VGA = 1,
            //DVI = 3,
            //HDMI = 4,
            //YPbPr = 12,
            //DisplayPort = 15,

            // MY
            DP = 15,
            mDP = 16,
            HDMI1 = 17,
            HDMI2 = 18
        }

        internal const int PHYSICAL_MONITOR_DESCRIPTION_SIZE = 128;
        internal const int INPUT_SELECT = 0x60;
        internal const int WM_HOTKEY = 0x0312;

        [DllImport("user32")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lpRect, MonitorEnumProc callback, int dwData);

        [DllImport("Dxva2.dll")]
        private static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, out uint pdwNumberOfPhysicalMonitors);

        [DllImport("Dxva2.dll")]
        private static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint dwPhysicalMonitorArraySize, [Out] PhysicalMonitor[] physicalMonitorArray);

        [DllImport("Dxva2.dll")]
        private static extern bool DestroyPhysicalMonitors(uint dwPhysicalMonitorArraySize, [Out] PhysicalMonitor[] physicalMonitorArray);

        [DllImport("Dxva2.dll")]
        private static extern bool SetVCPFeature(IntPtr hMonitor, byte bVCPCode, int dwNewValue);

        private delegate bool MonitorEnumProc(IntPtr hDesktop, IntPtr hdc, ref Rect pRect, int dwData);

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct PhysicalMonitor
        {
            public IntPtr hPhysicalMonitor;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = PHYSICAL_MONITOR_DESCRIPTION_SIZE)]
            public string szPhysicalMonitorDescription;
        }
    }
}
