# C++ LingoEngine.RNetProjectClient

This folder contains a C++ implementation of the `RNetProjectClient` using the Microsoft SignalR client and `nlohmann::json`.

This client talks directly to the core **LingoEngine**, enabling any project built on the engine to be remotely controlled.

## Build

Install dependencies using vcpkg:

```bash
vcpkg install microsoft-signalr nlohmann-json
```

Compile the example:

```bash
g++ -std=c++17 LingoEngine.RNetProjectClient.cpp example.cpp \
    -I$VCPKG_ROOT/installed/x64-linux/include \
    -L$VCPKG_ROOT/installed/x64-linux/lib \
    -lsignalrclient -lcpprest -lssl -lcrypto -pthread
```

The example connects to an RNet project host, registers a frame stream handler, fetches the current project JSON, sends a heartbeat, and disconnects.
