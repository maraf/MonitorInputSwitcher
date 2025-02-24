using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ComputeConfigurationSelector = System.Func<System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, int>>, System.Collections.Generic.KeyValuePair<string, System.Collections.Generic.Dictionary<string, int>>>;

namespace MonitorInputSwitcher
{
    public class MonitorService
    {
        public string FindThisName()
            => GetSettings().Computers.FirstOrDefault(s => s.Key == Environment.MachineName).Key;

        public string FindOtherName()
            => GetSettings().Computers.FirstOrDefault(s => s.Key != Environment.MachineName).Key;

        public bool HasThisForMonitor(int index)
            => HasMonitorForComputer(index, FindThis);

        public bool HasOtherForMonitor(int index)
            => HasMonitorForComputer(index, FindOther);

        private static bool HasMonitorForComputer(int index, ComputeConfigurationSelector computerSelector)
        {
            var configuration = GetSettings();
            var computerConfiguration = computerSelector(configuration.Computers);

            if (TryGetMonitorConfigurationKey(configuration, index, out var configurationKey))
                return computerConfiguration.Value?.ContainsKey(configurationKey) ?? false;

            return false;
        }

        private static bool TryGetMonitorConfigurationKey(ConfigurationVersion2 configuration, int index, out string name)
        {
            if (configuration.Monitors.Count == 0)
            {
                name = index.ToString();
                return true;
            }

            if (configuration.Monitors.TryGetValue(index, out name))
                return true;

            name = string.Empty;
            return false;
        }

        public int GetMonitorCount()
            => Win32.GetMonitorHandles().Count;

        public void SwitchAllToThis()
            => SwitchToThis(-1);

        public void SwitchToThis(int index)
            => Switch(FindThis, index);

        public void SwitchAllToOther()
            => SwitchToOther(-1);

        public void SwitchToOther(int index)
            => Switch(FindOther, index);

        public int? FindCurrentInput(int index)
        {
            var handles = Win32.GetMonitorHandles();
            if (index >= handles.Count)
                return null;

            var value = Win32.FindInputType(handles[index]);
            if (value == null)
                return null;

            return (int)value;
        }

        public string GetDefaultMonitorName(int index)
            => $"Monitor {index}";

        public string GetMonitorName(int index)
        {
            if (GetSettings().Monitors.TryGetValue(index, out var name))
                return name;

            return GetDefaultMonitorName(index);
        }

        private static void Switch(ComputeConfigurationSelector computerSelector, int index = -1)
        {
            var configuration = GetSettings();
            var handles = Win32.GetMonitorHandles();
            var other = computerSelector(configuration.Computers);

            for (int i = 0; i < handles.Count; i++)
            {
                if (index >= 0 && index != i)
                    continue;

                if (TryGetMonitorConfigurationKey(configuration, i, out var configurationKey) && other.Value.TryGetValue(configurationKey, out var value))
                {
                    var target = (Win32.InputType)value;

                    var current = Win32.FindInputType(handles[i]);
                    if (current == null || current.Value != target)
                        Win32.SetInputType(handles[i], target);
                }
            }
        }

        public List<(string Name, int Index)> GetMonitors()
        {
            var settings = GetSettings();
            var result = new List<(string Name, int Index)>();
            if (settings.Monitors != null && settings.Monitors.Count > 0)
            {
                foreach (var item in settings.Monitors)
                    result.Add((item.Value, item.Key));
            }
            else
            {
                for (int i = 0; i < GetMonitorCount(); i++)
                    result.Add((GetDefaultMonitorName(i), i));
            }

            return result;
        }

        private static ConfigurationVersion2 GetSettings()
        {
            var filePath = GetSettingsFilePath();
            if (File.Exists(filePath))
            {
                var jsonContent = File.ReadAllText(filePath);
                var version = JsonSerializer.Deserialize<ConfigurationVersion>(jsonContent);
                if (version?.Version == 2)
                {
                    var configuration = JsonSerializer.Deserialize<ConfigurationVersion2>(jsonContent);
                    if (configuration != null)
                        return configuration;
                }

                if (version == null || version.Version == null)
                {
                    var configuration = JsonSerializer.Deserialize<ConfigurationVersion1>(jsonContent);
                    if (configuration != null)
                    {
                        return new ConfigurationVersion2(new(), configuration);
                    }
                }
            }

            return new ConfigurationVersion2(new(), new());
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

        private static KeyValuePair<string, Dictionary<string, int>> FindThis(Dictionary<string, Dictionary<string, int>> settings)
            => settings.FirstOrDefault(s => s.Key == Environment.MachineName);

        private static KeyValuePair<string, Dictionary<string, int>> FindOther( Dictionary<string, Dictionary<string, int>> settings)
            => settings.FirstOrDefault(s => s.Key != Environment.MachineName);

        private record ConfigurationVersion(int? Version);

        private class ConfigurationVersion1 : Dictionary<string, Dictionary<string, int>>
        { }

        private record ConfigurationVersion2(Dictionary<int, string> Monitors, Dictionary<string, Dictionary<string, int>> Computers);
    }
}
