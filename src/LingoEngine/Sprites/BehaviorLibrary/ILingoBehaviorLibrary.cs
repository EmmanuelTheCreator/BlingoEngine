using System.Collections.Generic;

namespace LingoEngine.Sprites.BehaviorLibrary;

public interface ILingoBehaviorLibrary
{
    void Register(LingoBehaviorDefinition definition);
    IEnumerable<LingoBehaviorDefinition> GetAll();
    IEnumerable<LingoBehaviorDefinition> Search(string searchTerm);
    IEnumerable<string> GetCategories();
}
