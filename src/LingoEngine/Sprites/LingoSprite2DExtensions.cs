using LingoEngine.Scripts;
using System.Reflection;

namespace LingoEngine.Sprites
{
    public static class LingoSprite2DExtensions
    {
        public static LingoSpriteBehavior? AddBehavior(this LingoSprite2D sprite, LingoMemberScript script)
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
                return null;

            var behavior = sprite.SetBehavior(type, script);
            return behavior;
        }

        internal static LingoSpriteBehavior? SetBehavior(this LingoSprite2D sprite, Type type, LingoMemberScript script)
        {
            var method = typeof(LingoSprite2D).GetMethod(nameof(LingoSprite2D.SetBehavior), BindingFlags.Instance | BindingFlags.Public);
            if (method == null) return null;
            var generic = method.MakeGenericMethod(type);
            var behavior = (LingoSpriteBehavior?)generic.Invoke(sprite, new object?[] { null })!;
            behavior.Name = script.Name;
            behavior.ScriptMember = script;
            return behavior;
        }
    }
}
