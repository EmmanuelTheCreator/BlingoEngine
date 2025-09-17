using AbstUI.Primitives;
using BlingoEngine.Animations;
using BlingoEngine.FilmLoops;
using BlingoEngine.IO.Data.DTO;
using BlingoEngine.IO.Data.DTO.Members;
using BlingoEngine.IO.Data.DTO.Sprites;
using BlingoEngine.Sprites;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BlingoEngine.IO;

internal static class SpriteDtoConverter
{
    public static Blingo2DSpriteDTO ToDto(this BlingoSprite2D sprite)
    {
        var state = sprite.InitialState as BlingoSprite2DState ?? (BlingoSprite2DState)sprite.GetState();
        var dto = new Blingo2DSpriteDTO
        {
            Name = state.Name,
            SpriteNum = sprite.SpriteNum,
            Member = state.Member.ToDto()
                ?? sprite.MemberNum.ToDto(sprite.Cast?.Number ?? sprite.Member?.CastLibNum ?? 0),
            DisplayMember = state.DisplayMember,
            SpritePropertiesOffset = state.SpritePropertiesOffset,
            Lock = sprite.Lock,
            Visibility = sprite.Visibility,
            LocH = state.LocH,
            LocV = state.LocV,
            LocZ = state.LocZ,
            Rotation = state.Rotation,
            Skew = state.Skew,
            RegPoint = new BlingoPointDTO { X = state.RegPoint.X, Y = state.RegPoint.Y },
            Ink = state.Ink,
            ForeColor = state.ForeColor.ToDto(),
            BackColor = state.BackColor.ToDto(),
            Blend = state.Blend,
            Editable = state.Editable,
            FlipH = state.FlipH,
            FlipV = state.FlipV,
            Width = state.Width,
            Height = state.Height,
            BeginFrame = sprite.BeginFrame,
            EndFrame = sprite.EndFrame
        };

        foreach (var actor in GetSpriteActors(sprite))
        {
            switch (actor)
            {
                case BlingoSpriteAnimator anim:
                    dto.Animator = ToDto(anim);
                    break;
                case BlingoFilmLoopPlayer:
                    break;
            }
        }

        foreach (var behavior in sprite.Behaviors)
        {
            dto.Behaviors.Add(behavior.ToDto());
        }

        return dto;
    }

    public static Blingo2DSpriteDTO ToDto(this BlingoSprite2DVirtual sprite)
    {
        var state = sprite.InitialState as BlingoSprite2DVirtualState ?? (BlingoSprite2DVirtualState)sprite.GetState();
        return new Blingo2DSpriteDTO
        {
            Name = state.Name,
            SpriteNum = sprite.SpriteNum,
            Member = state.Member.ToDto()
                ?? sprite.MemberNum.ToDto(sprite.Member?.CastLibNum ?? 0),
            DisplayMember = state.DisplayMember,
            Lock = sprite.Lock,
            LocH = state.LocH,
            LocV = state.LocV,
            LocZ = state.LocZ,
            Rotation = state.Rotation,
            Skew = state.Skew,
            RegPoint = new BlingoPointDTO { X = state.RegPoint.X, Y = state.RegPoint.Y },
            Ink = state.Ink,
            ForeColor = state.ForeColor.ToDto(),
            BackColor = state.BackColor.ToDto(),
            Blend = state.Blend,
            Width = state.Width,
            Height = state.Height,
            FlipH = state.FlipH,
            FlipV = state.FlipV,
            BeginFrame = sprite.BeginFrame,
            EndFrame = sprite.EndFrame
        };
    }

    public static BlingoFilmLoopMemberSpriteDTO ToDto(this BlingoFilmLoopMemberSprite sprite)
    {
        return new BlingoFilmLoopMemberSpriteDTO
        {
            Name = sprite.Name,
            SpriteNum = sprite.SpriteNum,
            Member = sprite.MemberNumberInCast.ToDto(sprite.CastNum) ?? new BlingoMemberRefDTO(),
            DisplayMember = sprite.DisplayMember,
            LocH = sprite.LocH,
            LocV = sprite.LocV,
            LocZ = sprite.LocZ,
            Rotation = sprite.Rotation,
            Skew = sprite.Skew,
            RegPoint = new BlingoPointDTO { X = sprite.RegPoint.X, Y = sprite.RegPoint.Y },
            Ink = sprite.Ink,
            ForeColor = sprite.ForeColor.ToDto(),
            BackColor = sprite.BackColor.ToDto(),
            Blend = sprite.Blend,
            Width = sprite.Width,
            Height = sprite.Height,
            BeginFrame = sprite.BeginFrame,
            EndFrame = sprite.EndFrame,
            Channel = sprite.Channel,
            FlipH = sprite.FlipH,
            FlipV = sprite.FlipV,
            Hilite = sprite.Hilite,
            Animator = sprite.AnimatorProperties.ToDto()
        };
    }

    public static BlingoSpriteAnimatorDTO ToDto(this BlingoSpriteAnimator animator)
    {
        return animator.Properties.ToDto();
    }

    public static BlingoSpriteAnimatorDTO ToDto(this BlingoSpriteAnimatorProperties animProps)
    {
        return new BlingoSpriteAnimatorDTO
        {
            Position = animProps.Position.KeyFrames.Select(k => new BlingoPointKeyFrameDTO
            {
                Frame = k.Frame,
                Value = new BlingoPointDTO { X = k.Value.X, Y = k.Value.Y },
                Ease = (BlingoEaseTypeDTO)k.Ease
            }).ToList(),
            PositionOptions = animProps.Position.Options.ToDto(),
            Rotation = animProps.Rotation.KeyFrames.Select(k => new BlingoFloatKeyFrameDTO
            {
                Frame = k.Frame,
                Value = k.Value,
                Ease = (BlingoEaseTypeDTO)k.Ease
            }).ToList(),
            RotationOptions = animProps.Rotation.Options.ToDto(),
            Skew = animProps.Skew.KeyFrames.Select(k => new BlingoFloatKeyFrameDTO
            {
                Frame = k.Frame,
                Value = k.Value,
                Ease = (BlingoEaseTypeDTO)k.Ease
            }).ToList(),
            SkewOptions = animProps.Skew.Options.ToDto(),
            ForegroundColor = animProps.ForegroundColor.KeyFrames.Select(k => new BlingoColorKeyFrameDTO
            {
                Frame = k.Frame,
                Value = k.Value.ToDto(),
                Ease = (BlingoEaseTypeDTO)k.Ease
            }).ToList(),
            ForegroundColorOptions = animProps.ForegroundColor.Options.ToDto(),
            BackgroundColor = animProps.BackgroundColor.KeyFrames.Select(k => new BlingoColorKeyFrameDTO
            {
                Frame = k.Frame,
                Value = k.Value.ToDto(),
                Ease = (BlingoEaseTypeDTO)k.Ease
            }).ToList(),
            BackgroundColorOptions = animProps.BackgroundColor.Options.ToDto(),
            Blend = animProps.Blend.KeyFrames.Select(k => new BlingoFloatKeyFrameDTO
            {
                Frame = k.Frame,
                Value = k.Value,
                Ease = (BlingoEaseTypeDTO)k.Ease
            }).ToList(),
            BlendOptions = animProps.Blend.Options.ToDto()
        };
    }

    private static IEnumerable<object> GetSpriteActors(BlingoSprite2D sprite)
    {
        var field = typeof(BlingoSprite2D).GetField("_spriteActors", BindingFlags.NonPublic | BindingFlags.Instance);
        if (field?.GetValue(sprite) is IEnumerable<object> actors)
        {
            return actors;
        }

        return Enumerable.Empty<object>();
    }

    private static BlingoTweenOptionsDTO ToDto(this BlingoTweenOptions options)
    {
        return new BlingoTweenOptionsDTO
        {
            Enabled = options.Enabled,
            Curvature = options.Curvature,
            ContinuousAtEndpoints = options.ContinuousAtEndpoints,
            SpeedChange = (BlingoSpeedChangeTypeDTO)options.SpeedChange,
            EaseIn = options.EaseIn,
            EaseOut = options.EaseOut
        };
    }
}

