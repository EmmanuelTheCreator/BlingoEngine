using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LingoEngine.Director.Host;

/// <summary>
/// Hosts the debugging SignalR server within the Director process.
/// </summary>
public interface IDebugServer
{
    /// <summary>Provides access to the publisher used by the game loop.</summary>
    IDebugPublisher Publisher { get; }

    /// <summary>Starts the server on the specified URL without blocking.</summary>
    Task StartAsync(string url, CancellationToken ct = default);

    /// <summary>Stops the server and disposes all resources.</summary>
    Task StopAsync();
}

/// <summary>
/// Default implementation of <see cref="IDebugServer"/>.
/// </summary>
public sealed class DebugServer : IDebugServer
{
    private WebApplication? _app;
    private CancellationTokenSource? _cts;

    /// <inheritdoc />
    public IDebugPublisher Publisher
        => _app?.Services.GetRequiredService<IDebugPublisher>()
           ?? throw new InvalidOperationException("Server not started.");

    /// <inheritdoc />
    public async Task StartAsync(string url, CancellationToken ct = default)
    {
        if (_app is not null)
        {
            return;
        }

        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseKestrel();
        builder.WebHost.UseUrls(url);
        builder.Services.AddSingleton<IBus, Bus>();
        builder.Services.AddSingleton<IDebugPublisher, DebugPublisher>();
        builder.Services.AddSignalR();

        var app = builder.Build();
        app.MapHub<DirectorHub>("/director");
        await app.StartAsync(_cts.Token).ConfigureAwait(false);

        _app = app;
    }

    /// <inheritdoc />
    public async Task StopAsync()
    {
        if (_app is null)
        {
            return;
        }

        try
        {
            _cts?.Cancel();
            await _app.StopAsync().ConfigureAwait(false);
            await _app.DisposeAsync().ConfigureAwait(false);
        }
        finally
        {
            _cts?.Dispose();
            _app = null;
            _cts = null;
        }
    }
}
