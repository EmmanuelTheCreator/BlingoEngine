using BlingoEngine.Scripts;
using System.Reflection;

namespace BlingoEngine.Sprites
{
    public static class BlingoSprite2DExtensions
    {
        public static BlingoSpriteBehavior? AddBehavior(this BlingoSprite2D sprite, BlingoMemberScript script)
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
                    .FirstOrDefault(t => typeof(BlingoSpriteBehavior).IsAssignableFrom(t) && t.Name == script.Name);
                if (type != null)
                {
                    var setMethod = typeof(BlingoMemberScript).GetMethod(nameof(BlingoMemberScript.SetBehaviorType));
                    setMethod?.MakeGenericMethod(type).Invoke(script, null);
                }
            }

            if (type == null)
                return null;

            var behavior = sprite.SetBehavior(type, script);
            return behavior;
        }

        internal static BlingoSpriteBehavior? SetBehavior(this BlingoSprite2D sprite, Type type, BlingoMemberScript script)
        {
            var method = typeof(BlingoSprite2D).GetMethod(nameof(BlingoSprite2D.SetBehavior), BindingFlags.Instance | BindingFlags.Public);
            if (method == null) return null;
            var generic = method.MakeGenericMethod(type);
            var behavior = (BlingoSpriteBehavior?)generic.Invoke(sprite, new object?[] { null })!;
            behavior.Name = script.Name;
            behavior.ScriptMember = script;
            return behavior;
        }
    }
}

