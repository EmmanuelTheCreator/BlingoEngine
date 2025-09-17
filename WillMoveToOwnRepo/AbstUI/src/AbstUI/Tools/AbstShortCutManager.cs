using AbstUI.Commands;

namespace AbstUI.Tools
{
    public interface IAbstShortCutManager
    {
        event Action<AbstShortCutMap>? ShortCutAdded;
        event Action<AbstShortCutMap>? ShortCutRemoved;
        bool Execute(string keyCombination);
        AbstShortCutMap CreateShortCut(string name, string keyCombination, Func<AbstShortCutMap, IAbstCommand> command, string? description = null);
        void RemoveShortCut(string name);
        IEnumerable<AbstShortCutMap> GetShortCuts();
    }
    public class AbstShortCutManager : IAbstShortCutManager
    {
        private readonly Dictionary<string, AbstShortCutMap> _shortCuts = new();
        private readonly IAbstCommandManager _blingoCommandManager;

        public event Action<AbstShortCutMap>? ShortCutAdded;
        public event Action<AbstShortCutMap>? ShortCutRemoved;

        public AbstShortCutManager(IAbstCommandManager blingoCommandManager)
        {
            _blingoCommandManager = blingoCommandManager;
        }

        public AbstShortCutMap CreateShortCut(string name, string keyCombination, Func<AbstShortCutMap, IAbstCommand> command, string? description = null)
        {
            var shortcut = new AbstShortCutMap(name, command, description, keyCombination);
            _shortCuts[name] = shortcut;
            ShortCutAdded?.Invoke(shortcut);
            return shortcut;
        }

        public void RemoveShortCut(string name)
        {

            if (_shortCuts.TryGetValue(name, out var map))
            {
                _shortCuts.Remove(name);
                ShortCutRemoved?.Invoke(map);
            }
        }

        public bool Execute(string keyCombination)
        {
            // Implementation for calling a shortcut by its key combination
            foreach (var shortcut in _shortCuts.Values)
            {
                if (shortcut.KeyCombination == keyCombination)
                {
                    // Execute the action associated with this shortcut
                    return _blingoCommandManager.Handle(shortcut.GetCommand());
                }
            }
            throw new KeyNotFoundException($"Shortcut with key combination '{keyCombination}' not found.");
        }

        public IEnumerable<AbstShortCutMap> GetShortCuts() => _shortCuts.Values;
    }
}

