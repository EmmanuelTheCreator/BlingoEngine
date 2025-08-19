using AbstUI.Commands;

namespace LingoEngine.Director.Core.Tools
{
    public class DirectorShortCutMap
    {
        private readonly Func<DirectorShortCutMap, IAbstCommand> commandCtor;

        public string ShortCut { get; set; }
        public string? Description { get; set; }
        public string KeyCombination { get; }

        public DirectorShortCutMap(string shortCut, Func<DirectorShortCutMap, IAbstCommand> commandCtor, string? description = null, string keyCombination = null)
        {
            ShortCut = shortCut;
            this.commandCtor = commandCtor;
            Description = description;
            KeyCombination = keyCombination;
        }

        public IAbstCommand GetCommand() => commandCtor(this);
    }
}
