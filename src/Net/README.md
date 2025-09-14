# RNet

**RNet** stands for *Remote Net* and provides a lightweight protocol for driving a LingoEngine movie from another process.
It is built on top of SignalR and streams movie state, frame data, and debug commands so tools can control a running movie like a remote control.

RNet hooks directly into **LingoEngine**, enabling any project using the core engine to be remotely controlled.

The projects in this folder implement the different pieces of the system:

- **LingoEngine.Net.RNetContracts** – shared data contracts describing frames, sprites, and debug commands.
- **LingoEngine.Net.RNetHost** – a SignalR server that exposes an engine instance over RNet.
- **LingoEngine.Net.RNetClient** – a client library for connecting to an RNet host.
- **LingoEngine.Net.RNetTerminal** – a console application used for debugging and experimenting with the protocol.
- **cpp/LingoEngine.RNetClient** – a minimal C++ client showing how to consume the protocol from native code.

Together these components allow external tools to inspect and control movies in real time.
