# C++ LingoEngine.RNetClient

This folder contains a C++ implementation of the `RNetClient` using the Microsoft SignalR client and `nlohmann::json`.

This client talks directly to the core **LingoEngine**, enabling any project built on the engine to be remotely controlled.

## Build

Install dependencies using vcpkg:

```bash
vcpkg install microsoft-signalr nlohmann-json
```

Compile the example:

```bash
g++ -std=c++17 LingoEngine.RNetClient.cpp example.cpp \
    -I$VCPKG_ROOT/installed/x64-linux/include \
    -L$VCPKG_ROOT/installed/x64-linux/lib \
    -lsignalrclient -lcpprest -lssl -lcrypto -pthread
```

The example connects to an RNet hub, registers a frame stream handler, sends a heartbeat, and disconnects.
