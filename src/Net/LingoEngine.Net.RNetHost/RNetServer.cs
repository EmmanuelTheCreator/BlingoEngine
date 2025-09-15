using System.ComponentModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LingoEngine.Net.RNetHost;

/// <summary>
/// Hosts the SignalR server within the RNet process.
/// </summary>
public interface IRNetServer : INotifyPropertyChanged
{
    /// <summary>Provides access to the publisher used by the game loop.</summary>
    IRNetPublisher Publisher { get; }

    /// <summary>Indicates whether the server is currently running.</summary>
    bool IsEnabled { get; }

    /// <summary>Starts the server on the specified URL without blocking.</summary>
    Task StartAsync(string url, CancellationToken ct = default);

    /// <summary>Stops the server and disposes all resources.</summary>
    Task StopAsync();
}

/// <summary>
/// Default implementation of <see cref="IRNetServer"/>.
/// </summary>
public sealed class RNetServer : IRNetServer
{
    private WebApplication? _app;
    private CancellationTokenSource? _cts;
    private bool _isEnabled;

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc />
    public bool IsEnabled
    {
        get => _isEnabled;
        private set
        {
            if (_isEnabled == value)
            {
                return;
            }

            _isEnabled = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEnabled)));
        }
    }

    /// <inheritdoc />
    public IRNetPublisher Publisher
        => _app?.Services.GetRequiredService<IRNetPublisher>()
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
        builder.Services.AddSingleton<IRNetPublisher, RNetPublisher>();
        builder.Services.AddSignalR();

        var app = builder.Build();
        app.MapHub<LingoRNetHub>("/director");
        await app.StartAsync(_cts.Token).ConfigureAwait(false);

        _app = app;
        IsEnabled = true;
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
            IsEnabled = false;
        }
    }
}
