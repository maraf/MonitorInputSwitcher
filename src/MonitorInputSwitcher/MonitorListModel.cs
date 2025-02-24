using Neptuo.Observables.Collections;
using Neptuo.Observables.Commands;

namespace MonitorInputSwitcher;

public class MonitorListModel
{
    private MonitorService service;

    public ObservableCollection<MonitorModel> Monitors { get; } = new();
    public string ThisName { get; private set; }
    public string OtherName { get; private set; }
    public Command SwitchAllToThis { get; }
    public Command SwitchAllToOther { get; }
    public event Action OnReload;

    public MonitorListModel(MonitorService service)
    {
        this.service = service;
        SwitchAllToThis = new SwitchAllToThisCommand(service);
        SwitchAllToOther = new SwitchAllToOtherCommand(service);
        Build();
    }

    private void Build()
    {
        OtherName = service.FindOtherName();
        ThisName = service.FindThisName();

        foreach (var monitor in service.GetMonitors())
            Monitors.Add(new MonitorModel(service, monitor.Name, monitor.Index));
    }

    public void Reload()
    {
        Monitors.Clear();
        Build();
        OnReload?.Invoke();
    }
}
