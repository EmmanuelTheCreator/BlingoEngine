using AbstUI.Primitives;
using LingoEngine.Animations;
using LingoEngine.FilmLoops;
using LingoEngine.IO.Data.DTO;
using LingoEngine.Sprites;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LingoEngine.IO;

internal static class SpriteDtoConverter
{
    public static Lingo2DSpriteDTO ToDto(LingoSprite2D sprite)
    {
        var state = sprite.InitialState as LingoSprite2DState ?? (LingoSprite2DState)sprite.GetState();
        var dto = new Lingo2DSpriteDTO
        {
            Name = state.Name,
            SpriteNum = sprite.SpriteNum,
            Member = MemberRefDtoConverter.ToDto(state.Member)
                ?? MemberRefDtoConverter.ToDto(sprite.MemberNum, sprite.Cast?.Number ?? sprite.Member?.CastLibNum ?? 0),
            DisplayMember = state.DisplayMember,
            SpritePropertiesOffset = state.SpritePropertiesOffset,
            Lock = sprite.Lock,
            Visibility = sprite.Visibility,
            LocH = state.LocH,
            LocV = state.LocV,
            LocZ = state.LocZ,
            Rotation = state.Rotation,
            Skew = state.Skew,
            RegPoint = new LingoPointDTO { X = state.RegPoint.X, Y = state.RegPoint.Y },
            Ink = state.Ink,
            ForeColor = ColorDtoConverter.ToDto(state.ForeColor),
            BackColor = ColorDtoConverter.ToDto(state.BackColor),
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
                case LingoSpriteAnimator anim:
                    dto.Animator = ToDto(anim);
                    break;
                case LingoFilmLoopPlayer:
                    break;
            }
        }

        return dto;
    }

    public static Lingo2DSpriteDTO ToDto(LingoSprite2DVirtual sprite)
    {
        var state = sprite.InitialState as LingoSprite2DVirtualState ?? (LingoSprite2DVirtualState)sprite.GetState();
        return new Lingo2DSpriteDTO
        {
            Name = state.Name,
            SpriteNum = sprite.SpriteNum,
            Member = MemberRefDtoConverter.ToDto(state.Member)
                ?? MemberRefDtoConverter.ToDto(sprite.MemberNum, sprite.Member?.CastLibNum ?? 0),
            DisplayMember = state.DisplayMember,
            Lock = sprite.Lock,
            LocH = state.LocH,
            LocV = state.LocV,
            LocZ = state.LocZ,
            Rotation = state.Rotation,
            Skew = state.Skew,
            RegPoint = new LingoPointDTO { X = state.RegPoint.X, Y = state.RegPoint.Y },
            Ink = state.Ink,
            ForeColor = ColorDtoConverter.ToDto(state.ForeColor),
            BackColor = ColorDtoConverter.ToDto(state.BackColor),
            Blend = state.Blend,
            Width = state.Width,
            Height = state.Height,
            FlipH = state.FlipH,
            FlipV = state.FlipV,
            BeginFrame = sprite.BeginFrame,
            EndFrame = sprite.EndFrame
        };
    }

    public static LingoFilmLoopMemberSpriteDTO ToDto(LingoFilmLoopMemberSprite sprite)
    {
        return new LingoFilmLoopMemberSpriteDTO
        {
            Name = sprite.Name,
            SpriteNum = sprite.SpriteNum,
            Member = MemberRefDtoConverter.ToDto(sprite.MemberNumberInCast, sprite.CastNum) ?? new LingoMemberRefDTO(),
            DisplayMember = sprite.DisplayMember,
            LocH = sprite.LocH,
            LocV = sprite.LocV,
            LocZ = sprite.LocZ,
            Rotation = sprite.Rotation,
            Skew = sprite.Skew,
            RegPoint = new LingoPointDTO { X = sprite.RegPoint.X, Y = sprite.RegPoint.Y },
            Ink = sprite.Ink,
            ForeColor = ColorDtoConverter.ToDto(sprite.ForeColor),
            BackColor = ColorDtoConverter.ToDto(sprite.BackColor),
            Blend = sprite.Blend,
            Width = sprite.Width,
            Height = sprite.Height,
            BeginFrame = sprite.BeginFrame,
            EndFrame = sprite.EndFrame,
            Channel = sprite.Channel,
            FlipH = sprite.FlipH,
            FlipV = sprite.FlipV,
            Hilite = sprite.Hilite,
            Animator = ToDto(sprite.AnimatorProperties)
        };
    }

    public static LingoSpriteAnimatorDTO ToDto(LingoSpriteAnimator animator)
    {
        return ToDto(animator.Properties);
    }

    public static LingoSpriteAnimatorDTO ToDto(LingoSpriteAnimatorProperties animProps)
    {
        return new LingoSpriteAnimatorDTO
        {
            Position = animProps.Position.KeyFrames.Select(k => new LingoPointKeyFrameDTO
            {
                Frame = k.Frame,
                Value = new LingoPointDTO { X = k.Value.X, Y = k.Value.Y },
                Ease = (LingoEaseTypeDTO)k.Ease
            }).ToList(),
            PositionOptions = ToDto(animProps.Position.Options),
            Rotation = animProps.Rotation.KeyFrames.Select(k => new LingoFloatKeyFrameDTO
            {
                Frame = k.Frame,
                Value = k.Value,
                Ease = (LingoEaseTypeDTO)k.Ease
            }).ToList(),
            RotationOptions = ToDto(animProps.Rotation.Options),
            Skew = animProps.Skew.KeyFrames.Select(k => new LingoFloatKeyFrameDTO
            {
                Frame = k.Frame,
                Value = k.Value,
                Ease = (LingoEaseTypeDTO)k.Ease
            }).ToList(),
            SkewOptions = ToDto(animProps.Skew.Options),
            ForegroundColor = animProps.ForegroundColor.KeyFrames.Select(k => new LingoColorKeyFrameDTO
            {
                Frame = k.Frame,
                Value = ColorDtoConverter.ToDto(k.Value),
                Ease = (LingoEaseTypeDTO)k.Ease
            }).ToList(),
            ForegroundColorOptions = ToDto(animProps.ForegroundColor.Options),
            BackgroundColor = animProps.BackgroundColor.KeyFrames.Select(k => new LingoColorKeyFrameDTO
            {
                Frame = k.Frame,
                Value = ColorDtoConverter.ToDto(k.Value),
                Ease = (LingoEaseTypeDTO)k.Ease
            }).ToList(),
            BackgroundColorOptions = ToDto(animProps.BackgroundColor.Options),
            Blend = animProps.Blend.KeyFrames.Select(k => new LingoFloatKeyFrameDTO
            {
                Frame = k.Frame,
                Value = k.Value,
                Ease = (LingoEaseTypeDTO)k.Ease
            }).ToList(),
            BlendOptions = ToDto(animProps.Blend.Options)
        };
    }

    private static IEnumerable<object> GetSpriteActors(LingoSprite2D sprite)
    {
        var field = typeof(LingoSprite2D).GetField("_spriteActors", BindingFlags.NonPublic | BindingFlags.Instance);
        if (field?.GetValue(sprite) is IEnumerable<object> actors)
        {
            return actors;
        }

        return Enumerable.Empty<object>();
    }

    private static LingoTweenOptionsDTO ToDto(LingoTweenOptions options)
    {
        return new LingoTweenOptionsDTO
        {
            Enabled = options.Enabled,
            Curvature = options.Curvature,
            ContinuousAtEndpoints = options.ContinuousAtEndpoints,
            SpeedChange = (LingoSpeedChangeTypeDTO)options.SpeedChange,
            EaseIn = options.EaseIn,
            EaseOut = options.EaseOut
        };
    }
}
