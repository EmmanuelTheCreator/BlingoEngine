using System;
using System.IO;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace LingoEngine.Net.RNetPipeServer;

/// <summary>
/// Simple server using named pipes on Windows or Unix domain sockets elsewhere.
/// </summary>
public interface IRNetPipeServer : IAsyncDisposable
{
    /// <summary>Starts listening on the specified endpoint.</summary>
    Task StartAsync(string endpoint, CancellationToken ct = default);

    /// <summary>Sends a JSON payload with the specified command name.</summary>
    Task SendAsync<T>(string command, T payload, CancellationToken ct = default);

    /// <summary>Sends a pre-serialized JSON payload with the specified command name.</summary>
    Task SendRawAsync(string command, string json, CancellationToken ct = default);

    /// <summary>Raised when a command is received from a client.</summary>
    event Action<string, string> ClientMessageReceived;

    /// <summary>Stops the server.</summary>
    Task StopAsync();
}

/// <inheritdoc />
public sealed class RNetPipeServer : IRNetPipeServer
{
    private Stream? _stream;
    private Socket? _listener;
    private string? _socketPath;
    private CancellationTokenSource? _readerCts;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    /// <inheritdoc />
    public event Action<string, string>? ClientMessageReceived;

    /// <inheritdoc />
    public async Task StartAsync(string endpoint, CancellationToken ct = default)
    {
        if (OperatingSystem.IsWindows())
        {
            var server = new NamedPipeServerStream(endpoint, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            await server.WaitForConnectionAsync(ct).ConfigureAwait(false);
            _stream = server;
        }
        else
        {
            _socketPath = Path.Combine(Path.GetTempPath(), endpoint);
            if (File.Exists(_socketPath))
            {
                File.Delete(_socketPath);
            }

            _listener = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            var ep = new UnixDomainSocketEndPoint(_socketPath);
            _listener.Bind(ep);
            _listener.Listen(1);
            var socket = await _listener.AcceptAsync(ct).ConfigureAwait(false);
            _stream = new NetworkStream(socket, ownsSocket: true);
        }

        _readerCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _ = Task.Run(() => ReadLoopAsync(_readerCts.Token));
    }

    /// <inheritdoc />
    public async Task SendAsync<T>(string command, T payload, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(payload, _jsonOptions);
        await SendRawAsync(command, json, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task SendRawAsync(string command, string json, CancellationToken ct = default)
    {
        if (_stream is null)
        {
            throw new InvalidOperationException("Server not started.");
        }

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
            ClientMessageReceived?.Invoke(parts[0], json);
        }
    }

    /// <inheritdoc />
    public Task StopAsync()
    {
        _readerCts?.Cancel();
        _stream?.Dispose();
        _stream = null;
        _listener?.Dispose();
        if (_socketPath is not null && File.Exists(_socketPath))
        {
            File.Delete(_socketPath);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
    }
}
