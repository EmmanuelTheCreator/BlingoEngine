using System;
using System.Collections.Generic;
using System.Linq;
using AbstUI.Commands;
using AbstUI.Components.Containers;
using AbstUI.Components.Inputs;
using AbstUI.Primitives;
using AbstUI.Windowing;
using LingoEngine.Director.Core.Projects.Commands;
using LingoEngine.Director.Core.Remote.Commands;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Net.RNetContracts;
using LingoEngine.Net.RNetHost.Common;
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
        var dialog = _serviceProvider.GetRequiredService<RNetSettingsDialog>();
        var projectServer = _serviceProvider.GetRequiredService<IRNetProjectServer>();
        var pipeServer = _serviceProvider.GetService<IRNetPipeServer>();
        dialog.ProjectServerAvailable = projectServer is not DummyRNetProjectServer;
        dialog.PipeServerAvailable = pipeServer is not null;

        var rootNode = dialog.Create();
        var window = _windowManager.ShowCustomDialog("Remote Settings", rootNode.Framework<IAbstFrameworkPanel>());
        if (window != null)
        {
            dialog.RequestClose = window.Close;
        }

        return true;
    }
}

/// <summary>Displays and edits RNet configuration settings.</summary>
public class RNetSettingsDialog
{
    private static readonly KeyValuePair<string, string> HostRoleItem = new(RNetRemoteRole.Host.ToString(), "Remote Host");
    private static readonly KeyValuePair<string, string> ClientRoleItem = new(RNetRemoteRole.Client.ToString(), "Remote Client");

    private readonly ILingoFrameworkFactory _factory;
    private readonly IRNetConfiguration _settings;
    private readonly IAbstCommandManager _commandManager;
    private readonly IAbstWindowManager _windowManager;

    private Action _requestClose = new(() => { });
    private AbstInputCombobox? _roleCombo;
    private AbstInputCombobox? _transportCombo;
    private AbstInputCheckbox? _autoStartCheckbox;

    public bool ProjectServerAvailable { get; set; } = true;
    public bool PipeServerAvailable { get; set; } = true;

    public Action RequestClose
    {
        get => _requestClose;
        set
        {
            if (value != null)
            {
                _requestClose = value;
            }
        }
    }

    public RNetSettingsDialog(
        ILingoFrameworkFactory factory,
        IRNetConfiguration settings,
        IAbstCommandManager commandManager,
        IAbstWindowManager windowManager)
    {
        _factory = factory;
        _settings = settings;
        _commandManager = commandManager;
        _windowManager = windowManager;
    }

    public AbstPanel Create()
    {
        EnsureValidConfiguration();

        var root = _factory.CreatePanel("RemoteSettingsRoot");
        root.Width = 340;
        root.Height = 190;

        var wrap = _factory.CreateWrapPanel(AOrientation.Vertical, "RemoteSettingsWrap");
        wrap.Width = root.Width - 30;
        wrap.Height = root.Height - 30;
        wrap.Margin = new AMargin(15, 15, 15, 15);
        wrap.ItemMargin = new APoint(0, 6);

        wrap.Compose()
            .NewLine("TransportRow")
            .AddLabel("TransportLabel", "Transport:", 11, 120)
            .Configure(row =>
            {
                _transportCombo = _factory.CreateInputCombobox("RemoteTransportCombo", OnTransportChanged);
                _transportCombo.Width = 180;
                PopulateTransportOptions(_settings.RemoteRole);
                row.AddItem(_transportCombo);
            })
            .NewLine("RoleRow")
            .AddLabel("RoleLabel", "Remote Mode:", 11, 120)
            .Configure(row =>
            {
                _roleCombo = _factory.CreateInputCombobox("RemoteModeCombo", OnRoleChanged);
                _roleCombo.Width = 180;
                PopulateRoleOptions(_settings.ClientType);
                row.AddItem(_roleCombo);
            })
            .NewLine("PortRow")
            .AddLabel("PortLabel", "Port:", 11, 120)
            .AddNumericInputInt("PortInput", _settings, s => s.Port, 80)
            .NewLine("AutoStartRow")
            .AddLabel("AutoStartLabel", "Auto-start host:", 11, 120)
            .Configure(row =>
            {
                _autoStartCheckbox = _factory.CreateInputCheckbox("AutoStartCheckbox", value => _settings.AutoStartRNetHostOnStartup = value);
                _autoStartCheckbox.Checked = _settings.AutoStartRNetHostOnStartup;
                row.AddItem(_autoStartCheckbox);
            })
            .NewLine("ButtonsRow")
            .AddButton("BtnSave", "Save", () =>
            {
                if (!_commandManager.Handle(new SaveDirProjectSettingsCommand()))
                {
                    _windowManager.ShowNotification(
                        "Project not set correcty. Set project settings first.",
                        AbstUINotificationType.Error);
                    return;
                }

                _requestClose();
            })
            .Finalize();

        UpdateControlAvailability();

        root.AddItem(wrap, 0, 0);
        return root;
    }

    private void OnRoleChanged(string? selectedKey)
    {
        if (string.IsNullOrWhiteSpace(selectedKey))
        {
            return;
        }

        _settings.RemoteRole = Enum.Parse<RNetRemoteRole>(selectedKey);
        EnsureValidConfiguration();

        if (_roleCombo is not null)
        {
            var current = _settings.RemoteRole.ToString();
            if (_roleCombo.SelectedKey != current)
            {
                _roleCombo.SelectedKey = current;
            }
        }

        PopulateTransportOptions(_settings.RemoteRole);
        PopulateRoleOptions(_settings.ClientType);
        UpdateControlAvailability();
    }

    private void OnTransportChanged(string? selectedKey)
    {
        if (string.IsNullOrWhiteSpace(selectedKey))
        {
            return;
        }

        _settings.ClientType = Enum.Parse<RNetClientType>(selectedKey);
        EnsureValidConfiguration();
        PopulateRoleOptions(_settings.ClientType);
        UpdateControlAvailability();
    }

    private void PopulateRoleOptions(RNetClientType transport)
    {
        if (_roleCombo is null)
        {
            return;
        }

        var items = BuildRoleItems(transport).ToList();
        _roleCombo.ClearItems();
        foreach (var item in items)
        {
            _roleCombo.AddItem(item.Key, item.Value);
        }

        var selectedRole = _settings.RemoteRole.ToString();
        if (!items.Any(i => i.Key == selectedRole))
        {
            _settings.RemoteRole = RNetRemoteRole.Client;
            selectedRole = _settings.RemoteRole.ToString();
        }

        if (_roleCombo.SelectedKey != selectedRole)
        {
            _roleCombo.SelectedKey = selectedRole;
        }
    }

    private void PopulateTransportOptions(RNetRemoteRole role)
    {
        if (_transportCombo is null)
        {
            return;
        }

        var items = BuildTransportItems(role).ToList();
        _transportCombo.ClearItems();
        foreach (var item in items)
        {
            _transportCombo.AddItem(item.Key, item.Value);
        }

        if (!items.Any(i => i.Key == _settings.ClientType.ToString()))
        {
            if (items.FirstOrDefault() is { Key: { } key })
            {
                _settings.ClientType = Enum.Parse<RNetClientType>(key);
            }
        }

        var transportKey = _settings.ClientType.ToString();
        if (_transportCombo.SelectedKey != transportKey)
        {
            _transportCombo.SelectedKey = transportKey;
        }
    }

    private IEnumerable<KeyValuePair<string, string>> BuildRoleItems(RNetClientType transport)
    {
        if (HostAvailableForTransport(transport))
        {
            yield return HostRoleItem;
        }

        yield return ClientRoleItem;
    }

    private IEnumerable<KeyValuePair<string, string>> BuildTransportItems(RNetRemoteRole role)
    {
        if (role != RNetRemoteRole.Host || PipeServerAvailable)
        {
            yield return new KeyValuePair<string, string>(RNetClientType.Pipe.ToString(), "Pipe");
        }

        if (role != RNetRemoteRole.Host || ProjectServerAvailable)
        {
            yield return new KeyValuePair<string, string>(RNetClientType.Project.ToString(), "Project (Socket)");
        }
    }

    private bool HostAvailableForTransport(RNetClientType transport) => transport switch
    {
        RNetClientType.Project => ProjectServerAvailable,
        RNetClientType.Pipe => PipeServerAvailable,
        _ => false,
    };

    private IEnumerable<RNetClientType> GetHostCapableTransports()
    {
        if (PipeServerAvailable)
        {
            yield return RNetClientType.Pipe;
        }

        if (ProjectServerAvailable)
        {
            yield return RNetClientType.Project;
        }
    }

    private bool HostModeAvailable() => GetHostCapableTransports().Any();

    private void EnsureValidConfiguration()
    {
        if (_settings.RemoteRole != RNetRemoteRole.Host)
        {
            return;
        }

        var hostTransports = GetHostCapableTransports().ToList();
        if (!hostTransports.Any())
        {
            _settings.RemoteRole = RNetRemoteRole.Client;
            return;
        }

        if (!hostTransports.Contains(_settings.ClientType))
        {
            _settings.ClientType = hostTransports.First();
        }
    }

    private void UpdateControlAvailability()
    {
        var isHost = _settings.RemoteRole == RNetRemoteRole.Host;
        if (_autoStartCheckbox is not null)
        {
            _autoStartCheckbox.Enabled = isHost;
            _autoStartCheckbox.Checked = _settings.AutoStartRNetHostOnStartup;
        }

        if (_transportCombo is not null)
        {
            _transportCombo.Enabled = isHost ? HostModeAvailable() : true;
        }
    }
}
