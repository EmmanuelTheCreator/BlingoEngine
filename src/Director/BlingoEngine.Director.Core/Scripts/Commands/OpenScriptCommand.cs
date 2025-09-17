using AbstUI.Commands;
using BlingoEngine.Scripts;

namespace BlingoEngine.Director.Core.Scripts.Commands
{
    public class OpenScriptCommand : IAbstCommand
    {
        public BlingoMemberScript Script { get; }
        public int LineNumber { get; set; }

        public OpenScriptCommand(BlingoMemberScript script)
        {
            Script = script;
        }
    }
}

