using System;
using System.Collections.Generic;
using AbstUI.Commands;
using AbstUI.Components.Containers;
using AbstUI.Primitives;
using AbstUI.Windowing;
using LingoEngine.Director.Core.Projects.Commands;
using LingoEngine.Director.Core.Remote.Commands;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Net.RNetContracts;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Director.Core.Remote;


public class RNetSettingsDialogHandler : IAbstCommandHandler<OpenRNetSettingsCommand>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAbstWindowManager _windowManager;

    public RNetSettingsDialogHandler(IServiceProvider serviceProvider, IAbstWindowManager windowManager)
    {
        _serviceProvider = serviceProvider;
        _windowManager = windowManager;
    }

    public bool CanExecute(OpenRNetSettingsCommand command) => true;

    public bool Handle(OpenRNetSettingsCommand command)
    {
        Action requestClose = new Action(() => { });
        var dialog = _serviceProvider.GetRequiredService<RNetSettingsDialog>();
        var rootNode = dialog.Create();
        var window = _windowManager.ShowCustomDialog("Remote Settings", rootNode.Framework<IAbstFrameworkPanel>());
        if (window != null)
            dialog.RequestClose = window.Close;
        return true;
    }
}


/// <summary>Displays and edits RNet configuration settings.</summary>
public class RNetSettingsDialog
{
    private readonly ILingoFrameworkFactory _factory;
    private readonly IRNetConfiguration _settings;
    private readonly IAbstCommandManager _commandManager;
    private readonly IAbstWindowManager _windowManager;
    private static readonly KeyValuePair<string, string>[] ClientTypes =
    [
        new KeyValuePair<string, string>(RNetClientType.Project.ToString(), "Project Client (SignalR)"),
        new KeyValuePair<string, string>(RNetClientType.Pipe.ToString(), "Pipe Client"),
    ];
    private Action _requestClose = new Action(() => { });
    public Action RequestClose {
        get => _requestClose;
        set
        {
            if (value != null)
                _requestClose = value;
        }
    }
    public RNetSettingsDialog(ILingoFrameworkFactory factory, IRNetConfiguration settings, IAbstCommandManager commandManager, IAbstWindowManager windowManager)
    {
        _factory = factory;
        _settings = settings;
        _commandManager = commandManager;
        _windowManager = windowManager;
    }

   
    public AbstPanel Create()
    {
        var root = _factory.CreatePanel("RemoteSettingsRoot");
        root.Width = 300;
        root.Height = 140;
        AbstWrapPanel wrap = _factory.CreateWrapPanel(AOrientation.Vertical, "RemoteSettingsWrap");
        wrap.Width = root.Width - 30;
        wrap.Height = root.Height - 30;
        wrap.Margin = new AMargin(15, 15, 15, 15);
        wrap.Compose()
            .NewLine("ClientTypeRow")
            .AddLabel("ClientTypeLabel", "Client:", 11, 60)
            .AddCombobox(
                "ClientTypeCombo",
                ClientTypes,
                180,
                _settings.ClientType.ToString(),
                selected =>
                {
                    if (!string.IsNullOrWhiteSpace(selected))
                    {
                        _settings.ClientType = Enum.Parse<RNetClientType>(selected);
                    }
                })
            .NewLine("PortRow")
            .AddLabel("PortLabel", "Port:", 11, 60)
            .AddNumericInputInt("PortInput", _settings, s => s.Port, 60)
            .NewLine("AutoStartRow")
            .AddButton("BtnSave", "Save", () =>
            {
                var result = _commandManager.Handle(new SaveDirProjectSettingsCommand());
                if (!result)
                {
                    _windowManager.ShowNotification("Project not set correcty. Set project settings first.", AbstUINotificationType.Error);
                }
                else
                    _requestClose();
            })
            ;
        root.AddItem(wrap, 0, 0);
        return root;
    }
}
