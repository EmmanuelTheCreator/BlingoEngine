using AbstUI.LGodot;
using AbstUI.LGodot.Inputs;

namespace AbstUI.Inputs
{
    public class GlobalAbstKey : AbstKey , IAbstGlobalKey
    {
        public GlobalAbstKey(IAbstGodotRootNode rootNode)
        {
            var framework = new AbstGodotKey(rootNode.RootNode, new Lazy<AbstKey>(() => this));
            Init(framework);
        }
    }
  
}
