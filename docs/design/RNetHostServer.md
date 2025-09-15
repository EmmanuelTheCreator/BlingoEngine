# RNet Host Server

The `LingoEngine.Net.RNetHost` library exposes a SignalR-based host for remote tooling. It can be added to any application that configures the LingoEngine via `ILingoEngineRegistration`.

## Usage

Reference the **LingoEngine.Net.RNetHost** project and enable the host when registering the engine:

```csharp
using LingoEngine.Net.RNetHost;
using LingoEngine.Setup;

var registration = LingoEngineSetup.Create();
registration.WithRNetHostServer(); // listens on port 61699 by default
```

To listen on a different port:

```csharp
registration.WithRNetHostServer(7000);
```

`WithRNetHostServer` registers the server. It will start automatically during engine build only if `IRNetConfiguration.AutoStartRNetHostOnStartup` is set to `true`.
