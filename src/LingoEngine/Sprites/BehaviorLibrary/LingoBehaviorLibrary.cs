namespace LingoEngine.Sprites.BehaviorLibrary;

public class LingoBehaviorLibrary : ILingoBehaviorLibrary
{
    private readonly List<LingoBehaviorDefinition> _behaviors = new();

    public void Register(LingoBehaviorDefinition definition)
    {
        if (definition == null) throw new ArgumentNullException(nameof(definition));
        _behaviors.Add(definition);
    }

    public IEnumerable<LingoBehaviorDefinition> GetAll() => _behaviors;

    public IEnumerable<LingoBehaviorDefinition> Search(string searchTerm)
    {
        var term  = searchTerm?.ToLower() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(term))
            return new List<LingoBehaviorDefinition>();
        return _behaviors.Where(b => b.Name.ToLower().Contains(term)
            || b.Category.Contains(term));
    }

    public IEnumerable<string> GetCategories()
        => _behaviors.Select(b => b.Category).Distinct().OrderBy(c => c);
}
