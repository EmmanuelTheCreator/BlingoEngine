using AbstUI.Inputs;
using AbstUI.LGodot;

namespace AbstUI.LGodot.Inputs
{
    public class GlobalAbstKey : AbstKey, IAbstGlobalKey
    {
        public GlobalAbstKey(IAbstGodotRootNode rootNode)
        {
            var framework = new AbstGodotKey(rootNode.RootNode, new Lazy<AbstKey>(() => this));
            Init(framework);
            framework.SetKeyObj(this);
        }
    }

}
