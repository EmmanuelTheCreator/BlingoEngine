using AbstUI.Commands;
using AbstUI.Components.Containers;
using AbstUI.Primitives;
using AbstUI.Windowing;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Net.RNetContracts;
using LingoEngine.Director.Core.Remote.Commands;

namespace LingoEngine.Director.Core.Remote;

/// <summary>Displays and edits RNet configuration settings.</summary>
public class RNetSettingsDialog : IAbstCommandHandler<OpenRNetSettingsCommand>
{
    private readonly ILingoFrameworkFactory _factory;
    private readonly IAbstWindowManager _windowManager;
    private readonly IRNetConfiguration _settings;

    public RNetSettingsDialog(ILingoFrameworkFactory factory, IAbstWindowManager windowManager, IRNetConfiguration settings)
    {
        _factory = factory;
        _windowManager = windowManager;
        _settings = settings;
    }

    public bool CanExecute(OpenRNetSettingsCommand command) => true;

    public bool Handle(OpenRNetSettingsCommand command)
    {
        var root = _factory.CreateWrapPanel(AOrientation.Vertical, "RemoteSettingsRoot");
        root.Compose()
            .NewLine("PortRow")
            .AddLabel("PortLabel", "Port:", 11, 60)
            .AddNumericInputInt("PortInput", _settings, s => s.Port, 60)
            .NewLine("AutoStartRow")
            .AddStateButton("AutoStartHost", _settings, null, s => s.AutoStartRNetHostOnStartup, "Auto-start host on startup");
        _windowManager.ShowCustomDialog("Remote Settings", root.Framework<IAbstFrameworkPanel>());
        return true;
    }
}
