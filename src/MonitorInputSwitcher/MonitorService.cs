using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MonitorInputSwitcher
{
    public class MonitorService
    {
        public string FindThisName()
            => GetSettings().FirstOrDefault(s => s.Key == Environment.MachineName).Key;

        public string FindOtherName()
            => GetSettings().FirstOrDefault(s => s.Key != Environment.MachineName).Key;

        public bool HasThisForMonitor(int index) 
            => FindThis(GetSettings()).Value?.ContainsKey(index) ?? false;

        public bool HasOtherForMonitor(int index) 
            => FindOther(GetSettings()).Value?.ContainsKey(index) ?? false;

        public int GetMonitorCount()
            => Win32.GetMonitorHandles().Count;

        public void SwitchAllToThis()
            => SwitchToThis(-1);

        public void SwitchToThis(int index)
            => Switch(settings => FindThis(settings).Value, index);

        public void SwitchAllToOther()
            => SwitchToOther(-1);

        public void SwitchToOther(int index)
            => Switch(settings => FindOther(settings).Value, index);

        private static void Switch(Func<Dictionary<string, Dictionary<int, int>>, Dictionary<int, int>?> selector, int index = -1)
        {
            var settings = GetSettings();
            var handles = Win32.GetMonitorHandles();

            var other = selector(settings);
            if (other == null)
            {
                Console.WriteLine("Missing other configuration.");
                return;
            }

            for (int i = 0; i < handles.Count; i++)
            {
                if (index >= 0 && index != i)
                    continue;

                if (other.TryGetValue(i, out var input))
                    Win32.SetInputType(handles[i], (Win32.InputType)input);
            }
        }

        private static Dictionary<string, Dictionary<int, int>> GetSettings()
        {
            var filePath = GetSettingsFilePath();
            if (File.Exists(filePath))
            {
                var settings = JsonSerializer.Deserialize<Dictionary<string, Dictionary<int, int>>>(File.ReadAllText(filePath));
                if (settings != null)
                    return settings;
            }

            return new Dictionary<string, Dictionary<int, int>>();
        }

        private static string GetSettingsFilePath() 
            => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "MonitorInputSwitcher.json");

        public string GetSettingsPath()
        {
            var filePath = GetSettingsFilePath();
            if (!File.Exists(filePath))
            {
                var settings = new Dictionary<string, Dictionary<int, int>>();
                var thisMachine = settings[Environment.MachineName] = new Dictionary<int, int>();
                var otherMachine = settings["(Other Machine Name)"] = new Dictionary<int, int>();
                for (int i = 0; i < GetMonitorCount(); i++)
                {
                    thisMachine[i] = i;
                    otherMachine[i] = i;
                }

                File.WriteAllText(filePath, JsonSerializer.Serialize(settings, new JsonSerializerOptions() { WriteIndented = true }));
            }

            return filePath;
        }

        private static KeyValuePair<string, Dictionary<int, int>> FindThis(Dictionary<string, Dictionary<int, int>> settings)
            => settings.FirstOrDefault(s => s.Key == Environment.MachineName);

        private static KeyValuePair<string, Dictionary<int, int>> FindOther(Dictionary<string, Dictionary<int, int>> settings)
            => settings.FirstOrDefault(s => s.Key != Environment.MachineName);
    }
}
