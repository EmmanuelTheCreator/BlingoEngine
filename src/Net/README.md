# RNet

**RNet** stands for *Remote Net* and provides a lightweight protocol for driving a BlingoEngine movie from another process.
It is built on top of SignalR and streams movie state, frame data, and commands so tools can control a running movie like a remote control.

RNet hooks directly into **BlingoEngine**, enabling any project using the core engine to be remotely controlled.

The projects in this folder implement the different pieces of the system:

- **BlingoEngine.Net.RNetContracts** – shared data contracts describing frames, sprites, and commands.
- **BlingoEngine.Net.RNetProjectHost** – a SignalR server that exposes an engine instance over RNet.
- **BlingoEngine.Net.RNetProjectClient** – a client library for connecting to an RNet project host.
- **BlingoEngine.Net.RNetClientPlayer** – consumes a host and applies updates to a local BlingoEngine player.
- **BlingoEngine.Net.RNetTerminal** – a console application used for debugging and experimenting with the protocol.
- **BlingoEngine.Net.RNetServer** – forwards messages between project hosts and project clients.
- **cpp/BlingoEngine.RNetProjectClient** – a minimal C++ client showing how to consume the protocol from native code.

Together these components allow external tools to inspect and control movies in real time.

## Example

To expose an engine instance over RNet, register and start the host during engine setup:

```csharp
var engine = BlingoEngine.Setup.Engine
    .WithRNetProjectHostServer(7000) // custom port
    .Build();
```

Clients can then connect using `BlingoRNetProjectClient`:

```csharp
var client = new BlingoRNetProjectClient();
await client.ConnectAsync(new Uri("http://localhost:7000/director"), new HelloDto("project", "client", "1.0", "Sample client"));
```

