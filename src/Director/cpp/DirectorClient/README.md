# C++ DirectorClient

This folder contains a C++ implementation of the `DirectorClient` using the Microsoft SignalR client and `nlohmann::json`.

## Build

Install dependencies using vcpkg:

```bash
vcpkg install microsoft-signalr nlohmann-json
```

Compile the example:

```bash
g++ -std=c++17 DirectorClient.cpp example.cpp \
    -I$VCPKG_ROOT/installed/x64-linux/include \
    -L$VCPKG_ROOT/installed/x64-linux/lib \
    -lsignalrclient -lcpprest -lssl -lcrypto -pthread
```

The example connects to a Director hub, registers a frame stream handler, sends a heartbeat, and disconnects.
