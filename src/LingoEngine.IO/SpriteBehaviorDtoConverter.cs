using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using AbstUI.Primitives;
using LingoEngine.IO.Data.DTO;
using LingoEngine.IO.Data.DTO.Members;
using LingoEngine.IO.Data.DTO.Sprites;
using LingoEngine.Members;
using LingoEngine.Primitives;
using LingoEngine.Scripts;
using LingoEngine.Sprites;

namespace LingoEngine.IO;

internal static class SpriteBehaviorDtoConverter
{
    public static LingoSpriteBehavior? Apply(this LingoSpriteBehaviorDTO dto, LingoSprite2D sprite, Dictionary<(int CastNum, int MemberNum), LingoMember> memberMap)
    {
        var behavior = CreateSpriteBehavior(sprite, dto, memberMap);
        if (behavior == null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            behavior.Name = dto.Name;
        }

        ApplyUserProperties(dto, behavior);

        return behavior;
    }

    public static LingoSpriteBehaviorDTO ToDto(this LingoSpriteBehavior behavior)
    {
        var dto = new LingoSpriteBehaviorDTO
        {
            BehaviorType = behavior.GetType().FullName ?? string.Empty,
            Type = LingoMemberTypeDTO.Script,
            Name = behavior.Name,
            RegPoint = new LingoPointDTO()
        };

        if (behavior.ScriptMember != null)
        {
            dto.CastLibNum = behavior.ScriptMember.CastLibNum;
            dto.NumberInCast = behavior.ScriptMember.NumberInCast;
            dto.Name = behavior.ScriptMember.Name;
            dto.FileName = behavior.ScriptMember.FileName;
            dto.Comments = behavior.ScriptMember.Comments;
            dto.PurgePriority = behavior.ScriptMember.PurgePriority;
            dto.RegPoint = new LingoPointDTO { X = behavior.ScriptMember.RegPoint.X, Y = behavior.ScriptMember.RegPoint.Y };
            dto.Width = behavior.ScriptMember.Width;
            dto.Height = behavior.ScriptMember.Height;
            dto.Size = behavior.ScriptMember.Size;
        }
        else if (string.IsNullOrWhiteSpace(dto.Name))
        {
            dto.Name = behavior.GetType().Name;
        }

        dto.UserProperties = behavior.UserProperties.ToDto();
        return dto;
    }

    private static void ApplyUserProperties(LingoSpriteBehaviorDTO dto, LingoSpriteBehavior behavior)
    {
        var userProperties = dto.ToBehaviorProperties();
        if (userProperties.Count <= 0)
        {
            return;
        }

        var targetProperties = behavior.UserProperties;
        foreach (var key in targetProperties.Keys.ToList())
        {
            targetProperties.Remove(key);
        }

        foreach (var property in userProperties)
        {
            targetProperties.Add(property.Key, property.Value);
        }

        if (behavior is ILingoPropertyDescriptionList descriptionProvider)
        {
            var definitions = descriptionProvider.GetPropertyDescriptionList();
            if (definitions != null)
            {
                targetProperties.Apply(definitions);
            }
        }
    }

    public static List<LingoSpriteBehaviorPropertyDTO> ToDto(this BehaviorPropertiesContainer container)
    {
        var list = new List<LingoSpriteBehaviorPropertyDTO>();
        foreach (var item in container)
        {
            if (string.IsNullOrEmpty(item.Key.Name))
            {
                continue;
            }

            var dto = new LingoSpriteBehaviorPropertyDTO
            {
                Key = item.Key.Name
            };

            switch (item.Value)
            {
                case null:
                    dto.Type = LingoSymbol.String.Name;
                    dto.Value = null;
                    break;
                case string str:
                    dto.Type = LingoSymbol.String.Name;
                    dto.Value = str;
                    break;
                case bool boolean:
                    dto.Type = LingoSymbol.Boolean.Name;
                    dto.Value = boolean ? bool.TrueString : bool.FalseString;
                    break;
                case int intValue:
                    dto.Type = LingoSymbol.Int.Name;
                    dto.Value = intValue.ToString(CultureInfo.InvariantCulture);
                    break;
                case long longValue:
                    dto.Type = LingoSymbol.Int.Name;
                    dto.Value = longValue.ToString(CultureInfo.InvariantCulture);
                    break;
                case float floatValue:
                    dto.Type = LingoSymbol.Float.Name;
                    dto.Value = floatValue.ToString(CultureInfo.InvariantCulture);
                    break;
                case double doubleValue:
                    dto.Type = LingoSymbol.Float.Name;
                    dto.Value = doubleValue.ToString(CultureInfo.InvariantCulture);
                    break;
                case AColor colorValue:
                    dto.Type = LingoSymbol.Color.Name;
                    dto.ColorValue = colorValue.ToDTO();
                    break;
                default:
                    dto.Type = item.Value.GetType().FullName ?? string.Empty;
                    dto.Value = item.Value?.ToString();
                    break;
            }

            list.Add(dto);
        }

        return list;
    }

    public static BehaviorPropertiesContainer ToBehaviorProperties(this LingoSpriteBehaviorDTO dto)
    {
        var container = new BehaviorPropertiesContainer();
        if (dto.UserProperties == null)
        {
            return container;
        }

        foreach (var property in dto.UserProperties)
        {
            if (property == null || string.IsNullOrWhiteSpace(property.Key))
            {
                continue;
            }

            var symbol = LingoSymbol.New(property.Key);
            var type = property.Type?.Trim() ?? string.Empty;
            object? value;

            if (type.Equals(LingoSymbol.Color.Name, StringComparison.OrdinalIgnoreCase))
            {
                if (!property.ColorValue.HasValue)
                {
                    continue;
                }

                value = property.ColorValue.Value.ToAColor();
            }
            else if (type.Equals(LingoSymbol.Int.Name, StringComparison.OrdinalIgnoreCase))
            {
                if (property.Value == null || !int.TryParse(property.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
                {
                    continue;
                }

                value = parsed;
            }
            else if (type.Equals(LingoSymbol.Float.Name, StringComparison.OrdinalIgnoreCase))
            {
                if (property.Value == null || !float.TryParse(property.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedFloat))
                {
                    continue;
                }

                value = parsedFloat;
            }
            else if (type.Equals(LingoSymbol.Boolean.Name, StringComparison.OrdinalIgnoreCase))
            {
                if (property.Value == null || !bool.TryParse(property.Value, out var parsedBool))
                {
                    continue;
                }

                value = parsedBool;
            }
            else
            {
                value = property.Value;
            }

            container.Add(symbol, value);
        }

        return container;
    }

    private static LingoSpriteBehavior? CreateSpriteBehavior(LingoSprite2D sprite, LingoSpriteBehaviorDTO dto, Dictionary<(int CastNum, int MemberNum), LingoMember> memberMap)
    {
        LingoSpriteBehavior? behavior = null;

        if (dto.CastLibNum > 0 && dto.NumberInCast > 0)
        {
            var reference = new LingoMemberRefDTO
            {
                CastLibNum = dto.CastLibNum,
                MemberNum = dto.NumberInCast
            };

            if (TryResolveMember<LingoMemberScript>(memberMap, reference, out var scriptMember) && scriptMember != null)
            {
                behavior = sprite.AddBehavior(scriptMember);
            }
        }

        if (behavior != null)
        {
            return behavior;
        }

        var behaviorType = ResolveBehaviorType(dto.BehaviorType);
        if (behaviorType == null)
        {
            return null;
        }

        return CreateBehaviorByType(sprite, behaviorType);
    }

    private static LingoSpriteBehavior? CreateBehaviorByType(LingoSprite2D sprite, Type behaviorType)
    {
        if (!typeof(LingoSpriteBehavior).IsAssignableFrom(behaviorType))
        {
            return null;
        }

        var method = typeof(LingoSprite2D).GetMethod(nameof(LingoSprite2D.SetBehavior), BindingFlags.Instance | BindingFlags.Public);
        if (method == null)
        {
            return null;
        }

        return method.MakeGenericMethod(behaviorType).Invoke(sprite, new object?[] { null }) as LingoSpriteBehavior;
    }

    private static Type? ResolveBehaviorType(string? behaviorTypeName)
    {
        if (string.IsNullOrWhiteSpace(behaviorTypeName))
        {
            return null;
        }

        var direct = Type.GetType(behaviorTypeName);
        if (direct != null && typeof(LingoSpriteBehavior).IsAssignableFrom(direct))
        {
            return direct;
        }

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type? found;
            try
            {
                found = assembly
                    .GetTypes()
                    .FirstOrDefault(t => typeof(LingoSpriteBehavior).IsAssignableFrom(t) && (t.FullName == behaviorTypeName || t.Name == behaviorTypeName));
            }
            catch
            {
                continue;
            }

            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static bool TryResolveMember<TMember>(Dictionary<(int CastNum, int MemberNum), LingoMember> memberMap, LingoMemberRefDTO? reference, out TMember? member)
        where TMember : LingoMember
    {
        member = null;
        if (!TryResolveMember(memberMap, reference, out var baseMember))
        {
            return false;
        }

        if (baseMember is TMember typed)
        {
            member = typed;
            return true;
        }

        return false;
    }

    private static bool TryResolveMember(Dictionary<(int CastNum, int MemberNum), LingoMember> memberMap, LingoMemberRefDTO? reference, out LingoMember? member)
    {
        member = null;
        if (reference == null || reference.CastLibNum <= 0 || reference.MemberNum <= 0)
        {
            return false;
        }

        if (memberMap.TryGetValue((reference.CastLibNum, reference.MemberNum), out var found))
        {
            member = found;
            return true;
        }

        return false;
    }
}
