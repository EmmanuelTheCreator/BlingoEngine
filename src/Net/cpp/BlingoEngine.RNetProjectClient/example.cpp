#include "LingoEngine.RNetProjectClient.h"
#include <iostream>

int main()
{
    RNetProjectClient client;
    HelloDto hello{ "demo-project", "client-1", "1.0", "C++ sample" };

    client.Connect("http://localhost:5000/director", hello);

    client.StreamFrames([](const StageFrameDto& frame)
    {
        std::cout << "Received frame " << frame.FrameId << std::endl;
    });

    auto project = client.GetCurrentProject();
    std::cout << "Current project length: " << project.json.size() << std::endl;

    client.SendHeartbeat();

    client.Disconnect();
    return 0;
}

