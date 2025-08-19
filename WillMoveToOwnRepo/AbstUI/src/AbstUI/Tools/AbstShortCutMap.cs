using AbstUI.Commands;

namespace AbstUI.Tools
{
    public class AbstShortCutMap
    {
        private readonly Func<AbstShortCutMap, IAbstCommand> commandCtor;

        public string ShortCut { get; set; }
        public string? Description { get; set; }
        public string KeyCombination { get; }

        public AbstShortCutMap(string shortCut, Func<AbstShortCutMap, IAbstCommand> commandCtor, string? description = null, string keyCombination = null)
        {
            ShortCut = shortCut;
            this.commandCtor = commandCtor;
            Description = description;
            KeyCombination = keyCombination;
        }

        public IAbstCommand GetCommand() => commandCtor(this);
    }
}
