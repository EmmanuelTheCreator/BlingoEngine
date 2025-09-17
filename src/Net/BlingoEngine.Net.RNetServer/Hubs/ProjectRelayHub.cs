using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace BlingoEngine.Net.RNetServer;

public class ProjectRelayHub : Hub
{
    private readonly ProjectRegistry _registry;
    private readonly ILogger<ProjectRelayHub> _logger;

    public ProjectRelayHub(ProjectRegistry registry, ILogger<ProjectRelayHub> logger)
    {
        _registry = registry;
        _logger = logger;
    }

    public Task Register(string projectName, bool isHost)
    {
        var info = _registry.Projects.GetOrAdd(projectName, _ => new ProjectRegistry.ProjectInfo());
        if (isHost)
        {
            if (info.HostConnectionId is { } oldId && oldId != Context.ConnectionId)
            {
                _ = Clients.Client(oldId).SendAsync("HostReplaced");
            }
            info.HostConnectionId = Context.ConnectionId;
        }
        else
        {
            info.Clients[Context.ConnectionId] = 0;
        }
        return Task.CompletedTask;
    }

    public Task RequestProject(string projectName)
    {
        if (_registry.Projects.TryGetValue(projectName, out var info) && info.HostConnectionId is { } host)
        {
            return Clients.Client(host).SendAsync("RequestProject", Context.ConnectionId);
        }
        return Task.CompletedTask;
    }

    public Task SendProject(string projectName, string clientConnectionId, string projectJson)
    {
        return Clients.Client(clientConnectionId).SendAsync("ProjectData", projectJson);
    }

    public Task ForwardToClients(string projectName, string method, object[] args)
    {
        if (_registry.Projects.TryGetValue(projectName, out var info))
        {
            return Clients.Clients(info.Clients.Keys.ToList()).SendCoreAsync(method, args);
        }
        return Task.CompletedTask;
    }

    public Task ForwardToHost(string projectName, string method, object[] args)
    {
        if (_registry.Projects.TryGetValue(projectName, out var info) && info.HostConnectionId is { } host)
        {
            return Clients.Client(host).SendCoreAsync(method, args);
        }
        return Task.CompletedTask;
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        foreach (var kv in _registry.Projects)
        {
            var p = kv.Value;
            if (p.HostConnectionId == Context.ConnectionId)
            {
                p.HostConnectionId = null;
                p.Clients.Clear();
            }
            else
            {
                p.Clients.TryRemove(Context.ConnectionId, out _);
            }
        }
        return base.OnDisconnectedAsync(exception);
    }
}

