using Neptuo.Observables.Commands;

namespace MonitorInputSwitcher;

public class SwitchMonitorToThisCommand(MonitorService service, int index) : Command
{
    public override bool CanExecute() => service.HasThisForMonitor(index);
    public override void Execute() => service.SwitchToThis(index);
}
