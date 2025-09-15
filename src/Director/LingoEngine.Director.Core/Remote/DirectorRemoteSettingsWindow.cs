using AbstUI.Components.Containers;
using AbstUI.Components.Texts;
using AbstUI.Components.Inputs;
using AbstUI.Primitives;
using LingoEngine.Director.Core.UI;
using LingoEngine.FrameworkCommunication;

namespace LingoEngine.Director.Core.Remote;

/// <summary>Window for editing remote connection settings.</summary>
public class DirectorRemoteSettingsWindow : DirectorWindow<IDirFrameworkRemoteSettingsWindow>
{
    private readonly DirectorRemoteSettings _settings;
    private readonly AbstWrapPanel _root;

    /// <summary>Root panel used by framework-specific wrappers.</summary>
    public AbstWrapPanel RootPanel => _root;

    public DirectorRemoteSettingsWindow(IServiceProvider serviceProvider, DirectorRemoteSettings settings, ILingoFrameworkFactory factory)
        : base(serviceProvider, DirectorMenuCodes.RemoteSettingsWindow)
    {
        _settings = settings;
        Width = 180;
        Height = 80;
        X = 100;
        Y = 100;

        _root = factory.CreateWrapPanel(AOrientation.Vertical, "RemoteSettingsRoot");
        _root.Compose()
            .NewLine("PortRow")
            .AddLabel("PortLabel", "Port:", 11, 60)
            .AddNumericInputInt("PortInput", _settings, s => s.Port, 60);
    }
}
