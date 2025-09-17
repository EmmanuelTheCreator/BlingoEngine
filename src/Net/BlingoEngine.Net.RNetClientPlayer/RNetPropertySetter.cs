using System;
using System.Globalization;
using System.Reflection;
using BlingoEngine.Members;
using BlingoEngine.Sounds;
using BlingoEngine.Sprites;
using BlingoEngine.Texts;
using AbstUI.Texts;

namespace BlingoEngine.Net.RNetClientPlayer;

/// <summary>
/// Utility helpers for applying string-based property values to objects.
/// </summary>
public static class RNetPropertySetter
{
    /// <summary>Attempts to set a property on the target object.</summary>
    public static void TrySet(object target, string prop, string value)
    {
        switch (target)
        {
            case BlingoSprite2D sprite:
                TrySetSprite(sprite, prop, value);
                return;
            case IBlingoMember member:
                TrySetMember(member, prop, value);
                return;
            default:
                TrySetReflection(target, prop, value);
                return;
        }
    }

    private static void TrySetSprite(BlingoSprite2D sprite, string prop, string value)
    {
        switch (prop)
        {
            case nameof(BlingoSprite2D.LocH):
                if (TryConvert(typeof(float), value, out var locH)) sprite.LocH = (float)locH!;
                break;
            case nameof(BlingoSprite2D.LocV):
                if (TryConvert(typeof(float), value, out var locV)) sprite.LocV = (float)locV!;
                break;
            case nameof(BlingoSprite2D.LocZ):
                if (TryConvert(typeof(int), value, out var locZ)) sprite.LocZ = (int)locZ!;
                break;
            case nameof(BlingoSprite2D.Width):
                if (TryConvert(typeof(float), value, out var width)) sprite.Width = (float)width!;
                break;
            case nameof(BlingoSprite2D.Height):
                if (TryConvert(typeof(float), value, out var height)) sprite.Height = (float)height!;
                break;
            case nameof(BlingoSprite2D.Rotation):
                if (TryConvert(typeof(float), value, out var rotation)) sprite.Rotation = (float)rotation!;
                break;
            case nameof(BlingoSprite2D.Skew):
                if (TryConvert(typeof(float), value, out var skew)) sprite.Skew = (float)skew!;
                break;
            case nameof(BlingoSprite2D.Blend):
                if (TryConvert(typeof(float), value, out var blend)) sprite.Blend = (float)blend!;
                break;
            case nameof(BlingoSprite2D.Ink):
                if (TryConvert(typeof(int), value, out var ink)) sprite.Ink = (int)ink!;
                break;
            default:
                TrySetReflection(sprite, prop, value);
                break;
        }
    }

    private static void TrySetMember(IBlingoMember member, string prop, string value)
    {
        switch (member)
        {
            case IBlingoMemberTextBase text:
                TrySetTextMember(text, prop, value);
                break;
            case BlingoMemberSound sound:
                TrySetSoundMember(sound, prop, value);
                break;
            default:
                TrySetReflection(member, prop, value);
                break;
        }
    }

    private static void TrySetTextMember(IBlingoMemberTextBase text, string prop, string value)
    {
        switch (prop)
        {
            case "MarkDownText":
                text.SetTextMD(AbstMarkdownReader.Read(value));
                break;
            case nameof(IBlingoMemberTextBase.Font):
                text.Font = value;
                break;
            case nameof(IBlingoMemberTextBase.FontSize):
                if (TryConvert(typeof(int), value, out var fontSize)) text.FontSize = (int)fontSize!;
                break;
            case nameof(IBlingoMemberTextBase.Bold):
                if (TryConvert(typeof(bool), value, out var bold)) text.Bold = (bool)bold!;
                break;
            case nameof(IBlingoMemberTextBase.Italic):
                if (TryConvert(typeof(bool), value, out var italic)) text.Italic = (bool)italic!;
                break;
            case nameof(IBlingoMemberTextBase.Underline):
                if (TryConvert(typeof(bool), value, out var underline)) text.Underline = (bool)underline!;
                break;
            default:
                TrySetReflection(text, prop, value);
                break;
        }
    }

    private static void TrySetSoundMember(BlingoMemberSound sound, string prop, string value)
    {
        switch (prop)
        {
            case nameof(BlingoMemberSound.Loop):
                if (TryConvert(typeof(bool), value, out var loop)) sound.Loop = (bool)loop!;
                break;
            case nameof(BlingoMemberSound.IsLinked):
                if (TryConvert(typeof(bool), value, out var linked)) sound.IsLinked = (bool)linked!;
                break;
            case nameof(BlingoMemberSound.LinkedFilePath):
                sound.LinkedFilePath = value;
                break;
            default:
                TrySetReflection(sound, prop, value);
                break;
        }
    }

    private static void TrySetReflection(object target, string prop, string value)
    {
        var property = target.GetType().GetProperty(prop, BindingFlags.Public | BindingFlags.Instance);
        if (property == null || !property.CanWrite)
        {
            return;
        }

        if (TryConvert(property.PropertyType, value, out var converted))
        {
            property.SetValue(target, converted);
        }
    }

    /// <summary>Attempts to convert a string to the specified type.</summary>
    public static bool TryConvert(Type type, string value, out object? result)
    {
        result = null;
        if (type == typeof(string))
        {
            result = value;
            return true;
        }

        if (type.IsEnum)
        {
            try
            {
                result = Enum.Parse(type, value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Int32:
                if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var i))
                {
                    result = i;
                    return true;
                }
                break;
            case TypeCode.Single:
                if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var f))
                {
                    result = f;
                    return true;
                }
                break;
            case TypeCode.Double:
                if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                {
                    result = d;
                    return true;
                }
                break;
            case TypeCode.Boolean:
                if (bool.TryParse(value, out var b))
                {
                    result = b;
                    return true;
                }
                break;
            case TypeCode.Int16:
                if (short.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var s))
                {
                    result = s;
                    return true;
                }
                break;
            case TypeCode.Byte:
                if (byte.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var by))
                {
                    result = by;
                    return true;
                }
                break;
        }
        return false;
    }
}


