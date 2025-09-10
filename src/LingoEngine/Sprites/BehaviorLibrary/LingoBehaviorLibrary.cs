using System;
using System.Collections.Generic;
using System.Linq;

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
        return _behaviors.Where(b => b.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
            || b.Category.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<string> GetCategories()
        => _behaviors.Select(b => b.Category).Distinct().OrderBy(c => c);
}
