using System;
using System.Linq;
using System.Reflection;
using LingoEngine.Commands;
using LingoEngine.Director.Core.Inspector.Commands;
using LingoEngine.Scripts;
using LingoEngine.Sprites;

namespace LingoEngine.Director.Core.Sprites;

public static class LingoSprite2DExtensions
{
    public static void AttachBehavior(this LingoSprite2D sprite, LingoMemberScript script, ILingoCommandManager commandManager)
    {
        var type = script.GetBehaviorType();
        if (type == null)
        {
            type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return Array.Empty<Type>(); }
                })
                .FirstOrDefault(t => typeof(LingoSpriteBehavior).IsAssignableFrom(t) && t.Name == script.Name);
            if (type != null)
            {
                var setMethod = typeof(LingoMemberScript).GetMethod(nameof(LingoMemberScript.SetBehaviorType));
                setMethod?.MakeGenericMethod(type).Invoke(script, null);
            }
        }

        if (type == null)
            return;

        var behavior = sprite.SetBehavior(type);
        if (behavior == null) return;
        behavior.Name = script.Name;
        if (behavior is ILingoPropertyDescriptionList)
            commandManager.Handle(new OpenBehaviorPopupCommand(behavior));
        sprite.SpriteChannel?.RequireRedraw();
    }

    public static LingoSpriteBehavior? SetBehavior(this LingoSprite2D sprite, Type type)
    {
        var method = typeof(LingoSprite2D).GetMethod(nameof(LingoSprite2D.SetBehavior), BindingFlags.Instance | BindingFlags.Public);
        if (method == null) return null;
        var generic = method.MakeGenericMethod(type);
        return (LingoSpriteBehavior?)generic.Invoke(sprite, new object?[] { null });
    }
}
