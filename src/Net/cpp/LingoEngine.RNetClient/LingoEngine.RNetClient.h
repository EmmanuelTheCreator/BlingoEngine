#pragma once

#include <string>
#include <vector>
#include <variant>
#include <functional>
#include <memory>

#include <signalrclient/hub_connection.h>
#include <signalrclient/hub_connection_builder.h>
#include <nlohmann/json.hpp>

// Data Transfer Objects ------------------------------------------------------

struct HelloDto {
    std::string ProjectId;
    std::string ClientId;
    std::string Version;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(HelloDto, ProjectId, ClientId, Version);

struct StageFrameDto {
    int Width;
    int Height;
    long long FrameId;
    std::string TimestampUtc;
    std::vector<uint8_t> Argb32;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(StageFrameDto, Width, Height, FrameId, TimestampUtc, Argb32);

struct SpriteDeltaDto {
    int Frame;
    int SpriteNum;
    int Z;
    int MemberId;
    int LocH;
    int LocV;
    int Width;
    int Height;
    int Rotation;
    int Skew;
    int Blend;
    int Ink;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(SpriteDeltaDto, Frame, SpriteNum, Z, MemberId, LocH, LocV, Width, Height, Rotation, Skew, Blend, Ink);

struct KeyframeDto {
    int Frame;
    int SpriteNum;
    std::string Prop;
    std::string Value;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(KeyframeDto, Frame, SpriteNum, Prop, Value);

struct FilmLoopDto {
    std::string Name;
    int StartFrame;
    int EndFrame;
    bool IsPlaying;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(FilmLoopDto, Name, StartFrame, EndFrame, IsPlaying);

struct SoundEventDto {
    int Frame;
    std::string SoundName;
    bool IsPlaying;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(SoundEventDto, Frame, SoundName, IsPlaying);

struct TempoDto {
    int Frame;
    int Tempo;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(TempoDto, Frame, Tempo);

struct ColorPaletteDto {
    int Frame;
    std::vector<uint8_t> Argb32;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(ColorPaletteDto, Frame, Argb32);

struct FrameScriptDto {
    int Frame;
    std::string Script;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(FrameScriptDto, Frame, Script);

struct TransitionDto {
    int Frame;
    std::string Type;
    int Duration;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(TransitionDto, Frame, Type, Duration);

struct MemberPropertyDto {
    std::string MemberName;
    std::string Prop;
    std::string Value;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(MemberPropertyDto, MemberName, Prop, Value);

struct TextStyleDto {
    std::string MemberName;
    int Start;
    int End;
    std::string Style;
    std::string Value;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(TextStyleDto, MemberName, Start, End, Style, Value);

struct MovieStateDto {
    int Frame;
    int Tempo;
    bool IsPlaying;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(MovieStateDto, Frame, Tempo, IsPlaying);

// Debug command hierarchy -----------------------------------------------------

struct SetSpritePropCmd {
    int SpriteNum;
    std::string Prop;
    std::string Value;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(SetSpritePropCmd, SpriteNum, Prop, Value);

struct SetMemberPropCmd {
    std::string MemberName;
    std::string Prop;
    std::string Value;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(SetMemberPropCmd, MemberName, Prop, Value);

struct GoToFrameCmd {
    int Frame;
};
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(GoToFrameCmd, Frame);

struct PauseCmd {};
inline void to_json(nlohmann::json& j, const PauseCmd&) { j = nlohmann::json::object(); }
inline void from_json(const nlohmann::json&, PauseCmd&) {}

struct ResumeCmd {};
inline void to_json(nlohmann::json& j, const ResumeCmd&) { j = nlohmann::json::object(); }
inline void from_json(const nlohmann::json&, ResumeCmd&) {}

struct DebugCommandDto {
    std::variant<SetSpritePropCmd, SetMemberPropCmd, GoToFrameCmd, PauseCmd, ResumeCmd> Command;
};
inline void to_json(nlohmann::json& j, const DebugCommandDto& cmd)
{
    std::visit([&j](const auto& v) { j = v; }, cmd.Command);
}

// RNetClient --------------------------------------------------------------

class RNetClient {
public:
    void Connect(const std::string& hubUrl, const HelloDto& hello);
    void Disconnect();

    void StreamFrames(std::function<void(const StageFrameDto&)> handler);
    void StreamDeltas(std::function<void(const SpriteDeltaDto&)> handler);
    void StreamKeyframes(std::function<void(const KeyframeDto&)> handler);
    void StreamFilmLoops(std::function<void(const FilmLoopDto&)> handler);
    void StreamSounds(std::function<void(const SoundEventDto&)> handler);
    void StreamTempos(std::function<void(const TempoDto&)> handler);
    void StreamColorPalettes(std::function<void(const ColorPaletteDto&)> handler);
    void StreamFrameScripts(std::function<void(const FrameScriptDto&)> handler);
    void StreamTransitions(std::function<void(const TransitionDto&)> handler);
    void StreamMemberProperties(std::function<void(const MemberPropertyDto&)> handler);
    void StreamTextStyles(std::function<void(const TextStyleDto&)> handler);

    MovieStateDto GetMovieSnapshot();
    void SendCommand(const DebugCommandDto& cmd);
    void SendHeartbeat();

private:
    std::shared_ptr<signalr::hub_connection> _connection;
};

