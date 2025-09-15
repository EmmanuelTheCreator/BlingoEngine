using System;
using System.IO;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace LingoEngine.Net.RNetPipeClient;

/// <summary>
/// Simple client using named pipes on Windows or Unix domain sockets elsewhere.
/// </summary>
public interface IRNetPipeClient : IAsyncDisposable
{
    /// <summary>Connects to the pipe or Unix socket.</summary>
    Task ConnectAsync(string endpoint, CancellationToken ct = default);

    /// <summary>Sends a JSON payload with the specified command name.</summary>
    Task SendAsync<T>(string command, T payload, CancellationToken ct = default);

    /// <summary>Raised when a command is received.</summary>
    event Action<string, string> MessageReceived;

    /// <summary>Disconnects from the server.</summary>
    Task DisconnectAsync();
}

/// <inheritdoc />
public sealed class RNetPipeClient : IRNetPipeClient
{
    private Stream? _stream;
    private CancellationTokenSource? _readerCts;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    /// <inheritdoc />
    public event Action<string, string>? MessageReceived;

    /// <inheritdoc />
    public async Task ConnectAsync(string endpoint, CancellationToken ct = default)
    {
        if (OperatingSystem.IsWindows())
        {
            var client = new NamedPipeClientStream(".", endpoint, PipeDirection.InOut, PipeOptions.Asynchronous);
            await client.ConnectAsync(ct).ConfigureAwait(false);
            _stream = client;
        }
        else
        {
            var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            var path = Path.Combine(Path.GetTempPath(), endpoint);
            var ep = new UnixDomainSocketEndPoint(path);
            await socket.ConnectAsync(ep, ct).ConfigureAwait(false);
            _stream = new NetworkStream(socket, ownsSocket: true);
        }

        _readerCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _ = Task.Run(() => ReadLoopAsync(_readerCts.Token));
    }

    /// <inheritdoc />
    public async Task SendAsync<T>(string command, T payload, CancellationToken ct = default)
    {
        if (_stream is null)
        {
            throw new InvalidOperationException("Not connected.");
        }

        var json = JsonSerializer.Serialize(payload, _jsonOptions);
        var header = $"{command} {Encoding.UTF8.GetByteCount(json)}\n";
        var headerBytes = Encoding.UTF8.GetBytes(header);
        await _stream.WriteAsync(headerBytes, ct).ConfigureAwait(false);
        var jsonBytes = Encoding.UTF8.GetBytes(json);
        await _stream.WriteAsync(jsonBytes, ct).ConfigureAwait(false);
        await _stream.FlushAsync(ct).ConfigureAwait(false);
    }

    private async Task ReadLoopAsync(CancellationToken ct)
    {
        if (_stream is null)
        {
            return;
        }

        var reader = new StreamReader(_stream, Encoding.UTF8, leaveOpen: true);

        while (!ct.IsCancellationRequested)
        {
            var header = await reader.ReadLineAsync(ct).ConfigureAwait(false);
            if (header is null)
            {
                break;
            }

            var parts = header.Split(' ', 2);
            if (parts.Length != 2 || !int.TryParse(parts[1], out var length))
            {
                continue;
            }

            var buffer = new char[length];
            var read = 0;
            while (read < length)
            {
                var r = await reader.ReadAsync(buffer.AsMemory(read, length - read), ct).ConfigureAwait(false);
                if (r == 0)
                {
                    return;
                }

                read += r;
            }

            var json = new string(buffer);
            MessageReceived?.Invoke(parts[0], json);
        }
    }

    /// <inheritdoc />
    public Task DisconnectAsync()
    {
        _readerCts?.Cancel();
        _stream?.Dispose();
        _stream = null;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync().ConfigureAwait(false);
    }
}
