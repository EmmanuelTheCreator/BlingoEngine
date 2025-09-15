# RNet Host Server

The `LingoEngine.Net.RNetProjectHost` library exposes a SignalR-based host for remote tooling. It can be added to any application that configures the LingoEngine via `ILingoEngineRegistration`.

## Usage

Reference the **LingoEngine.Net.RNetProjectHost** project and enable the host when registering the engine:

```csharp
using LingoEngine.Net.RNetProjectHost;
using LingoEngine.Setup;

var registration = LingoEngineSetup.Create();
registration.WithRNetProjectHostServer(); // listens on port 61699 by default
```

To listen on a different port:

```csharp
registration.WithRNetProjectHostServer(7000);
```

`WithRNetProjectHostServer` registers the server. It will start automatically during engine build only if `IRNetConfiguration.AutoStartRNetHostOnStartup` is set to `true`.
