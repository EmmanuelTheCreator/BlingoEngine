using System;
using System.Linq;
using AbstUI.Primitives;
using LingoEngine.Casts;
using LingoEngine.Members;
using LingoEngine.Sprites;

namespace LingoEngine.Scripts
{
    public class LingoMemberScript : LingoMember
    {
        public LingoScriptType ScriptType { get; set; } = LingoScriptType.Behavior;
        public string? BehaviorTypeName { get; private set; }

        public LingoMemberScript(ILingoFrameworkMember frameworkMember, LingoCast cast, int numberInCast, string name = "", string fileName = "", APoint regPoint = default)
            : base(frameworkMember, LingoMemberType.Script, cast, numberInCast, name, fileName, regPoint)
        {
        }

        public LingoMemberScript SetBehaviorType<TBehavior>() where TBehavior : LingoSpriteBehavior
           => SetBehaviorType(typeof(TBehavior));

        public LingoMemberScript SetBehaviorType(Type behaviorType)
        {
            BehaviorTypeName = behaviorType.FullName;
            return this;
        }

        public Type? GetBehaviorType()
            => BehaviorTypeName == null
                ? null
                : AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a =>
                    {
                        try { return a.GetTypes(); }
                        catch { return Array.Empty<Type>(); }
                    })
                    .FirstOrDefault(t => t.FullName == BehaviorTypeName);
    }
}

