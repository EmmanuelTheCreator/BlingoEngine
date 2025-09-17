using System.Linq;
using AbstUI.Primitives;
using BlingoEngine.Core;
using BlingoEngine.FilmLoops;
using BlingoEngine.Movies;
using BlingoEngine.Net.RNetContracts;
using BlingoEngine.Animations;
using BlingoEngine.Scripts;
using BlingoEngine.Sprites;
using BlingoEngine.Texts;
using BlingoEngine.Transitions;

namespace BlingoEngine.Net.RNetClientPlayer;

/// <summary>
/// Applies incoming RNet DTOs to a player instance.
/// </summary>
internal sealed class RNetClientPlayerApplier
{
    private readonly IBlingoPlayer _player;

    public RNetClientPlayerApplier(IBlingoPlayer player)
    {
        _player = player;
    }

    public void ApplyFrame(StageFrameDto frame)
    {
        _player.Stage.Width = frame.Width;
        _player.Stage.Height = frame.Height;
        if (frame.Argb32.Length >= 4)
        {
            var color = AColor.FromRGBA(frame.Argb32[1], frame.Argb32[2], frame.Argb32[3], frame.Argb32[0]);
            _player.Stage.BackgroundColor = color;
        }
    }

    public void ApplyDelta(SpriteDeltaDto delta)
    {
        if (_player.ActiveMovie is not BlingoMovie movie)
        {
            return;
        }
        var manager = movie.Sprite2DManager;
        var sprite = manager.GetAllSpritesByChannel(delta.SpriteNum).FirstOrDefault(s => s.BeginFrame == delta.BeginFrame)
                     ?? manager.Add(delta.SpriteNum, delta.BeginFrame, delta.BeginFrame);
        ApplySpriteState(sprite, new RNetSpriteDto(delta.SpriteNum, delta.BeginFrame, delta.Z, delta.CastLibNum, delta.MemberNum, delta.LocH, delta.LocV, delta.Width, delta.Height, delta.Rotation, delta.Skew, delta.Blend, delta.Ink));
    }

    public void ApplyKeyframe(KeyframeDto keyframe)
    {
        if (GetSprite(keyframe.SpriteNum, keyframe.BeginFrame) is not BlingoSprite2D sprite)
        {
            return;
        }

        switch (keyframe.Prop)
        {
            case "Move":
                if (int.TryParse(keyframe.Value, out var to))
                {
                    sprite.MoveKeyFrame(keyframe.Frame, to);
                }
                break;
            case "Delete":
                sprite.DeleteKeyFrame(keyframe.Frame);
                break;
            default:
                if (TryBuildSetting(sprite, keyframe, out var setting))
                {
                    var exists = sprite.GetKeyframes()?.Any(k => k.Frame == keyframe.Frame) ?? false;
                    if (exists)
                    {
                        sprite.UpdateKeyframe(setting);
                    }
                    else
                    {
                        sprite.AddKeyframes(setting);
                    }
                }
                break;
        }
    }

    private static bool TryBuildSetting(BlingoSprite2D sprite, KeyframeDto dto, out BlingoKeyFrameSetting setting)
    {
        setting = new BlingoKeyFrameSetting(dto.Frame);

        switch (dto.Prop)
        {
            case nameof(BlingoSprite2D.LocH):
                if (RNetPropertySetter.TryConvert(typeof(float), dto.Value, out var x))
                {
                    setting.Position = new APoint((float)x!, sprite.LocV);
                    return true;
                }
                break;
            case nameof(BlingoSprite2D.LocV):
                if (RNetPropertySetter.TryConvert(typeof(float), dto.Value, out var y))
                {
                    setting.Position = new APoint(sprite.LocH, (float)y!);
                    return true;
                }
                break;
            case nameof(BlingoSprite2D.Width):
                if (RNetPropertySetter.TryConvert(typeof(float), dto.Value, out var w))
                {
                    setting.Size = new APoint((float)w!, sprite.Height);
                    return true;
                }
                break;
            case nameof(BlingoSprite2D.Height):
                if (RNetPropertySetter.TryConvert(typeof(float), dto.Value, out var h))
                {
                    setting.Size = new APoint(sprite.Width, (float)h!);
                    return true;
                }
                break;
            case nameof(BlingoSprite2D.Rotation):
                if (RNetPropertySetter.TryConvert(typeof(float), dto.Value, out var rot))
                {
                    setting.Rotation = (float)rot!;
                    return true;
                }
                break;
            case nameof(BlingoSprite2D.Blend):
                if (RNetPropertySetter.TryConvert(typeof(float), dto.Value, out var blend))
                {
                    setting.Blend = (float)blend!;
                    return true;
                }
                break;
            case nameof(BlingoSprite2D.Skew):
                if (RNetPropertySetter.TryConvert(typeof(float), dto.Value, out var skew))
                {
                    setting.Skew = (float)skew!;
                    return true;
                }
                break;
        }

        setting = default;
        return false;
    }

    public void ApplyFilmLoop(FilmLoopDto loop)
    {
        var member = _player.CastLibs.GetMember<BlingoFilmLoopMember>(loop.MemberNum, loop.CastLibNum);
        if (member == null)
        {
            return;
        }
        foreach (var entry in member.SpriteEntries)
        {
            entry.BeginFrame = loop.StartFrame;
            entry.EndFrame = loop.EndFrame;
        }
        member.Loop = loop.IsPlaying;
    }

    public void ApplySoundEvent(SoundEventDto sound)
    {
        var member = _player.CastLibs.GetMember(sound.MemberNum, sound.CastLibNum);
        if (sound.IsPlaying && member != null)
        {
            _player.Sound.PuppetSound(1, member.Name);
        }
        else
        {
            _player.Sound.StopAll();
        }
    }

    public void ApplyTempo(TempoDto tempo)
    {
        if (_player.ActiveMovie is BlingoMovie movie)
        {
            movie.Tempo = tempo.Tempo;
        }
    }

    public void ApplyColorPalette(ColorPaletteDto palette)
    {
        if (palette.Argb32.Length >= 4)
        {
            var color = AColor.FromRGBA(palette.Argb32[1], palette.Argb32[2], palette.Argb32[3], palette.Argb32[0]);
            _player.Stage.BackgroundColor = color;
        }
    }

    public void ApplyFrameScript(FrameScriptDto script)
    {
        if (_player.ActiveMovie is BlingoMovie movie)
        {
            movie.FrameScripts.Add(script.Frame, fs => fs.Name = script.Script);
        }
    }

    public void ApplyTransition(TransitionDto transition)
    {
        if (_player.ActiveMovie is BlingoMovie movie)
        {
            var settings = new BlingoTransitionFrameSettings { TransitionName = transition.Type };
            movie.Transitions.Add(transition.Frame, settings);
        }
    }

    public void ApplyMemberProperty(RNetMemberPropertyDto prop)
    {
        var member = _player.CastLibs.GetMember(prop.MemberNum, prop.CastLibNum);
        if (member != null)
        {
            RNetPropertySetter.TrySet(member, prop.Prop, prop.Value);
        }
    }

    public void ApplyMovieProperty(RNetMoviePropertyDto prop)
    {
        if (_player.ActiveMovie is BlingoMovie movie)
        {
            RNetPropertySetter.TrySet(movie, prop.Prop, prop.Value);
        }
    }

    public void ApplyStageProperty(RNetStagePropertyDto prop)
    {
        RNetPropertySetter.TrySet(_player.Stage, prop.Prop, prop.Value);
    }

    public void ApplySpriteCollectionEvent(RNetSpriteCollectionEventDto evt)
    {
        if (_player.ActiveMovie is not BlingoMovie movie || evt.Manager != "Sprite2D")
        {
            return;
        }
        var manager = movie.Sprite2DManager;
        switch (evt.Event)
        {
            case RNetSpriteCollectionEventType.Added when evt.Sprite is not null:
                var sprite = manager.Add(evt.Sprite.SpriteNum, evt.Sprite.BeginFrame, movie.FrameCount);
                ApplySpriteState(sprite, evt.Sprite);
                break;
            case RNetSpriteCollectionEventType.Removed:
                var existing = manager.GetAllSpritesByChannel(evt.SpriteNum).FirstOrDefault(s => s.BeginFrame == evt.BeginFrame);
                existing?.RemoveMe();
                break;
            case RNetSpriteCollectionEventType.Cleared:
                foreach (var s in manager.GetAllSprites().ToArray())
                {
                    s.RemoveMe();
                }
                break;
        }
    }

    public void ApplyTextStyle(TextStyleDto style)
    {
        if (_player.CastLibs.GetMember(style.MemberNum, style.CastLibNum) is IBlingoMemberTextBase text)
        {
            RNetPropertySetter.TrySet(text, style.Style, style.Value);
        }
    }

    private BlingoSprite2D? GetSprite(int spriteNum, int beginFrame)
    {
        if (_player.ActiveMovie is not BlingoMovie movie)
        {
            return null;
        }
        return movie.Sprite2DManager.GetAllSpritesByChannel(spriteNum).FirstOrDefault(s => s.BeginFrame == beginFrame);
    }

    private void ApplySpriteState(BlingoSprite2D sprite, RNetSpriteDto dto)
    {
        sprite.LocZ = dto.Z;
        if (dto.MemberNum != 0)
        {
            var mem = _player.CastLibs.GetMember(dto.MemberNum, dto.CastLibNum);
            if (mem != null)
            {
                sprite.SetMember(mem);
            }
        }
        sprite.LocH = dto.LocH;
        sprite.LocV = dto.LocV;
        sprite.Width = dto.Width;
        sprite.Height = dto.Height;
        sprite.Rotation = dto.Rotation;
        sprite.Skew = dto.Skew;
        sprite.Blend = dto.Blend;
        sprite.Ink = dto.Ink;
    }
}


