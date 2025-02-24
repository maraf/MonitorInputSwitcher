using Neptuo.Observables.Commands;

namespace MonitorInputSwitcher;

public class MonitorModel(MonitorService service, string name, int index)
{
    public string Name { get; } = name;
    public Command SwitchToOther { get; } = new SwitchMonitorToOtherCommand(service, index);
    public Command SwitchToThis { get; } = new SwitchMonitorToThisCommand(service, index);
}
