using System;
using System.Linq;
using AbstUI.Primitives;
using BlingoEngine.Casts;
using BlingoEngine.Members;
using BlingoEngine.Sprites;

namespace BlingoEngine.Scripts
{
    public class BlingoMemberScript : BlingoMember
    {
        private BlingoScriptType _scriptType = BlingoScriptType.Behavior;
        public BlingoScriptType ScriptType
        {
            get => _scriptType;
            set => SetProperty(ref _scriptType, value);
        }
        public string? BehaviorTypeName { get; private set; }

        public BlingoMemberScript(IBlingoFrameworkMember frameworkMember, BlingoCast cast, int numberInCast, string name = "", string fileName = "", APoint regPoint = default)
            : base(frameworkMember, BlingoMemberType.Script, cast, numberInCast, name, fileName, regPoint)
        {
        }

        public BlingoMemberScript SetBehaviorType<TBehavior>() where TBehavior : BlingoSpriteBehavior
           => SetBehaviorType(typeof(TBehavior));

        public BlingoMemberScript SetBehaviorType(Type behaviorType)
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


