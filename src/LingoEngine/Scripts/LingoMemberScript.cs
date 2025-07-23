using LingoEngine.Casts;
using LingoEngine.Members;
using LingoEngine.Primitives;

namespace LingoEngine.Scripts
{
    public class LingoMemberScript : LingoMember
    {
        public LingoScriptType ScriptType { get; set; } = LingoScriptType.Behavior;
        public LingoMemberScript(ILingoFrameworkMember frameworkMember, LingoCast cast, int numberInCast, string name = "", string fileName = "", LingoPoint regPoint = default) : base(frameworkMember, LingoMemberType.Script, cast, numberInCast, name, fileName, regPoint)
        {
        }
    }
}
