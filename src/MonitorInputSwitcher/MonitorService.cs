using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MonitorInputSwitcher
{
    public class MonitorService
    {
        public void SwitchAll()
        {
            Console.WriteLine($"MachineName '{Environment.MachineName}'.");

            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "MonitorInputSwitcher.json");
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Missing configuration.");
                return;
            }

            var settings = JsonSerializer.Deserialize<Dictionary<string, Dictionary<int, int>>>(File.ReadAllText(filePath));
            if (settings == null)
            {
                Console.WriteLine("Missing configuration.");
                return;
            }

            var handles = Win32.GetMonitorHandles();

            var other = settings.FirstOrDefault(s => s.Key != Environment.MachineName);
            if (other.Key == null)
            {
                Console.WriteLine("Missing other configuration.");
                return;
            }

            for (int i = 0; i < handles.Count; i++)
            {
                if (other.Value.TryGetValue(i, out var input))
                    Win32.SetInputType(handles[i], (Win32.InputType)input);
            }
        }
    }
}
