#include "DirectorClient.h"
#include <iostream>

int main()
{
    DirectorClient client;
    HelloDto hello{ "demo-project", "client-1", "1.0" };

    client.Connect("http://localhost:5000/directorHub", hello);

    client.StreamFrames([](const StageFrameDto& frame)
    {
        std::cout << "Received frame " << frame.FrameId << std::endl;
    });

    client.SendHeartbeat();

    client.Disconnect();
    return 0;
}

