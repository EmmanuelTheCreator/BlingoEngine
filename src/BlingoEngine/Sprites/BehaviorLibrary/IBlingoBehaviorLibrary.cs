using System.Collections.Generic;

namespace BlingoEngine.Sprites.BehaviorLibrary;

public interface IBlingoBehaviorLibrary
{
    void Register(BlingoBehaviorDefinition definition);
    IEnumerable<BlingoBehaviorDefinition> GetAll();
    IEnumerable<BlingoBehaviorDefinition> Search(string searchTerm);
    IEnumerable<string> GetCategories();
}

