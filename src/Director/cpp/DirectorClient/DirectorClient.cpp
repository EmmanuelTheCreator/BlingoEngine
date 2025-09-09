#include "DirectorClient.h"

void DirectorClient::Connect(const std::string& hubUrl, const HelloDto& hello)
{
    auto connection = signalr::hub_connection_builder::create(hubUrl).build();
    connection.start().get();
    connection.invoke<void>("SessionHello", hello).get();
    _connection = std::make_shared<signalr::hub_connection>(std::move(connection));
}

void DirectorClient::Disconnect()
{
    if (_connection)
    {
        _connection->stop().get();
        _connection.reset();
    }
}

void DirectorClient::StreamFrames(std::function<void(const StageFrameDto&)> handler)
{
    if (_connection)
    {
        _connection->on("StreamFrames", [handler](const StageFrameDto& dto)
        {
            handler(dto);
        });
    }
}

void DirectorClient::StreamDeltas(std::function<void(const SpriteDeltaDto&)> handler)
{
    if (_connection)
    {
        _connection->on("StreamDeltas", [handler](const SpriteDeltaDto& dto)
        {
            handler(dto);
        });
    }
}

void DirectorClient::StreamKeyframes(std::function<void(const KeyframeDto&)> handler)
{
    if (_connection)
    {
        _connection->on("StreamKeyframes", [handler](const KeyframeDto& dto)
        {
            handler(dto);
        });
    }
}

void DirectorClient::StreamFilmLoops(std::function<void(const FilmLoopDto&)> handler)
{
    if (_connection)
    {
        _connection->on("StreamFilmLoops", [handler](const FilmLoopDto& dto)
        {
            handler(dto);
        });
    }
}

void DirectorClient::StreamSounds(std::function<void(const SoundEventDto&)> handler)
{
    if (_connection)
    {
        _connection->on("StreamSounds", [handler](const SoundEventDto& dto)
        {
            handler(dto);
        });
    }
}

void DirectorClient::StreamTempos(std::function<void(const TempoDto&)> handler)
{
    if (_connection)
    {
        _connection->on("StreamTempos", [handler](const TempoDto& dto)
        {
            handler(dto);
        });
    }
}

void DirectorClient::StreamColorPalettes(std::function<void(const ColorPaletteDto&)> handler)
{
    if (_connection)
    {
        _connection->on("StreamColorPalettes", [handler](const ColorPaletteDto& dto)
        {
            handler(dto);
        });
    }
}

void DirectorClient::StreamFrameScripts(std::function<void(const FrameScriptDto&)> handler)
{
    if (_connection)
    {
        _connection->on("StreamFrameScripts", [handler](const FrameScriptDto& dto)
        {
            handler(dto);
        });
    }
}

void DirectorClient::StreamTransitions(std::function<void(const TransitionDto&)> handler)
{
    if (_connection)
    {
        _connection->on("StreamTransitions", [handler](const TransitionDto& dto)
        {
            handler(dto);
        });
    }
}

void DirectorClient::StreamMemberProperties(std::function<void(const MemberPropertyDto&)> handler)
{
    if (_connection)
    {
        _connection->on("StreamMemberProperties", [handler](const MemberPropertyDto& dto)
        {
            handler(dto);
        });
    }
}

void DirectorClient::StreamTextStyles(std::function<void(const TextStyleDto&)> handler)
{
    if (_connection)
    {
        _connection->on("StreamTextStyles", [handler](const TextStyleDto& dto)
        {
            handler(dto);
        });
    }
}

MovieStateDto DirectorClient::GetMovieSnapshot()
{
    return _connection->invoke<MovieStateDto>("GetMovieSnapshot").get();
}

void DirectorClient::SendCommand(const DebugCommandDto& cmd)
{
    if (_connection)
    {
        _connection->invoke<void>("SendCommand", cmd).get();
    }
}

void DirectorClient::SendHeartbeat()
{
    if (_connection)
    {
        _connection->invoke<void>("Heartbeat").get();
    }
}

