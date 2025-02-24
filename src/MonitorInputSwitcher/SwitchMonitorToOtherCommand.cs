using Neptuo.Observables.Commands;

namespace MonitorInputSwitcher;

public class SwitchMonitorToOtherCommand(MonitorService service, int index) : Command
{
    public override bool CanExecute() => service.HasOtherForMonitor(index);
    public override void Execute() => service.SwitchToOther(index);
}
