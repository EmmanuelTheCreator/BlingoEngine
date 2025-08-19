using AbstUI.Commands;
using LingoEngine.Scripts;

namespace LingoEngine.Director.Core.Scripts.Commands
{
    public class OpenScriptCommand : IAbstCommand
    {
        public LingoMemberScript Script { get; }
        public int LineNumber { get; set; }

        public OpenScriptCommand(LingoMemberScript script)
        {
            Script = script;
        }
    }
}
