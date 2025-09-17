#include "BlingoEngine.RNetProjectClient.h"

void RNetProjectClient::Connect(const std::string& hubUrl, const HelloDto& hello)
{
    auto connection = signalr::hub_connection_builder::create(hubUrl).build();
    connection.start().get();
    connection.invoke<void>("SessionHello", hello).get();
    _connection = std::make_shared<signalr::hub_connection>(std::move(connection));
}

void RNetProjectClient::Disconnect()
{
    if (_connection)
    {
        _connection->stop().get();
        _connection.reset();
    }
}

void RNetProjectClient::StreamFrames(std::function<void(const StageFrameDto&)> handler)
{
    if (_connection)
    {
        _connection->on("StreamFrames", [handler](const StageFrameDto& dto)
        {
            handler(dto);
        });
    }
}

void RNetProjectClient::StreamDeltas(std::function<void(const SpriteDeltaDto&)> handler)
{
    if (_connection)
    {
        _connection->on("StreamDeltas", [handler](const SpriteDeltaDto& dto)
        {
            handler(dto);
        });
    }
}

void RNetProjectClient::StreamKeyframes(std::function<void(const KeyframeDto&)> handler)
{
    if (_connection)
    {
        _connection->on("StreamKeyframes", [handler](const KeyframeDto& dto)
        {
            handler(dto);
        });
    }
}

void RNetProjectClient::StreamFilmLoops(std::function<void(const FilmLoopDto&)> handler)
{
    if (_connection)
    {
        _connection->on("StreamFilmLoops", [handler](const FilmLoopDto& dto)
        {
            handler(dto);
        });
    }
}

void RNetProjectClient::StreamSounds(std::function<void(const SoundEventDto&)> handler)
{
    if (_connection)
    {
        _connection->on("StreamSounds", [handler](const SoundEventDto& dto)
        {
            handler(dto);
        });
    }
}

void RNetProjectClient::StreamTempos(std::function<void(const TempoDto&)> handler)
{
    if (_connection)
    {
        _connection->on("StreamTempos", [handler](const TempoDto& dto)
        {
            handler(dto);
        });
    }
}

void RNetProjectClient::StreamColorPalettes(std::function<void(const ColorPaletteDto&)> handler)
{
    if (_connection)
    {
        _connection->on("StreamColorPalettes", [handler](const ColorPaletteDto& dto)
        {
            handler(dto);
        });
    }
}

void RNetProjectClient::StreamFrameScripts(std::function<void(const FrameScriptDto&)> handler)
{
    if (_connection)
    {
        _connection->on("StreamFrameScripts", [handler](const FrameScriptDto& dto)
        {
            handler(dto);
        });
    }
}

void RNetProjectClient::StreamTransitions(std::function<void(const TransitionDto&)> handler)
{
    if (_connection)
    {
        _connection->on("StreamTransitions", [handler](const TransitionDto& dto)
        {
            handler(dto);
        });
    }
}

void RNetProjectClient::StreamMemberProperties(std::function<void(const RNetMemberPropertyDto&)> handler)
{
    if (_connection)
    {
        _connection->on("StreamMemberProperties", [handler](const RNetMemberPropertyDto& dto)
        {
            handler(dto);
        });
    }
}

void RNetProjectClient::StreamMovieProperties(std::function<void(const RNetMoviePropertyDto&)> handler)
{
    if (_connection)
    {
        _connection->on("StreamMovieProperties", [handler](const RNetMoviePropertyDto& dto)
        {
            handler(dto);
        });
    }
}

void RNetProjectClient::StreamStageProperties(std::function<void(const RNetStagePropertyDto&)> handler)
{
    if (_connection)
    {
        _connection->on("StreamStageProperties", [handler](const RNetStagePropertyDto& dto)
        {
            handler(dto);
        });
    }
}

void RNetProjectClient::StreamSpriteCollectionEvents(std::function<void(const RNetSpriteCollectionEventDto&)> handler)
{
    if (_connection)
    {
        _connection->on("StreamSpriteCollectionEvents", [handler](const RNetSpriteCollectionEventDto& dto)
        {
            handler(dto);
        });
    }
}

void RNetProjectClient::StreamTextStyles(std::function<void(const TextStyleDto&)> handler)
{
    if (_connection)
    {
        _connection->on("StreamTextStyles", [handler](const TextStyleDto& dto)
        {
            handler(dto);
        });
    }
}

MovieStateDto RNetProjectClient::GetMovieSnapshot()
{
    return _connection->invoke<MovieStateDto>("GetMovieSnapshot").get();
}

LingoProjectJsonDto RNetProjectClient::GetCurrentProject()
{
    return _connection->invoke<LingoProjectJsonDto>("GetCurrentProject").get();
}

void RNetProjectClient::SendCommand(const RNetCommand& cmd)
{
    if (_connection)
    {
        _connection->invoke<void>("SendCommand", cmd).get();
    }
}

void RNetProjectClient::SendHeartbeat()
{
    if (_connection)
    {
        _connection->invoke<void>("Heartbeat").get();
    }
}

