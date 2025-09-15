# RNet

**RNet** stands for *Remote Net* and provides a lightweight protocol for driving a LingoEngine movie from another process.
It is built on top of SignalR and streams movie state, frame data, and commands so tools can control a running movie like a remote control.

RNet hooks directly into **LingoEngine**, enabling any project using the core engine to be remotely controlled.

The projects in this folder implement the different pieces of the system:

- **LingoEngine.Net.RNetContracts** – shared data contracts describing frames, sprites, and commands.
- **LingoEngine.Net.RNetHost** – a SignalR server that exposes an engine instance over RNet.
- **LingoEngine.Net.RNetClient** – a client library for connecting to an RNet host.
- **LingoEngine.Net.RNetClientPlayer** – consumes a host and applies updates to a local LingoEngine player.
- **LingoEngine.Net.RNetTerminal** – a console application used for debugging and experimenting with the protocol.
- **cpp/LingoEngine.RNetClient** – a minimal C++ client showing how to consume the protocol from native code.

Together these components allow external tools to inspect and control movies in real time.

## Example

To expose an engine instance over RNet, register and start the host during engine setup:

```csharp
var engine = LingoEngine.Setup.Engine
    .WithRNetHostServer(7000) // custom port
    .Build();
```

Clients can then connect using `LingoRNetClient`:

```csharp
var client = new LingoRNetClient();
await client.ConnectAsync(new Uri("http://localhost:7000/director"), new HelloDto("project", "client", "1.0"));
```
