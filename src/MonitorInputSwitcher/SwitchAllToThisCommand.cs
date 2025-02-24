using Neptuo.Observables.Commands;

namespace MonitorInputSwitcher;

public class SwitchAllToThisCommand(MonitorService service) : Command
{
    public override bool CanExecute() => true;
    public override void Execute() => service.SwitchAllToThis();
}
