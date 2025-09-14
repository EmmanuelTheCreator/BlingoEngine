# RNet Host Server

The `LingoEngine.Net.RNetHost` library exposes a SignalR-based debug host for the Director tooling. It can be added to any application that configures the LingoEngine via `ILingoEngineRegistration`.

## Usage

Reference the **LingoEngine.Net.RNetHost** project and enable the host when registering the engine:

```csharp
using LingoEngine.Net.RNetHost;
using LingoEngine.Setup;

var registration = LingoEngineSetup.Create();
registration.WithDirectorHostServer(); // listens on http://localhost:61699 by default
```

To listen on a different address, provide the desired URL:

```csharp
registration.WithDirectorHostServer("http://0.0.0.0:7000");
```

`WithDirectorHostServer` registers the debug server and starts it as the engine builds, making it available for Director clients to connect.
