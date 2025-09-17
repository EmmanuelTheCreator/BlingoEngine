namespace BlingoEngine.Sprites.BehaviorLibrary;

public class BlingoBehaviorLibrary : IBlingoBehaviorLibrary
{
    private readonly List<BlingoBehaviorDefinition> _behaviors = new();

    public void Register(BlingoBehaviorDefinition definition)
    {
        if (definition == null) throw new ArgumentNullException(nameof(definition));
        _behaviors.Add(definition);
    }

    public IEnumerable<BlingoBehaviorDefinition> GetAll() => _behaviors;

    public IEnumerable<BlingoBehaviorDefinition> Search(string searchTerm)
    {
        var term  = searchTerm?.ToLower() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(term))
            return new List<BlingoBehaviorDefinition>();
        return _behaviors.Where(b => b.Name.ToLower().Contains(term)
            || b.Category.Contains(term));
    }

    public IEnumerable<string> GetCategories()
        => _behaviors.Select(b => b.Category).Distinct().OrderBy(c => c);
}

