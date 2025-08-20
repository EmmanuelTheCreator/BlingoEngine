using AbstUI.Inputs;

namespace AbstUI.SDL2.Inputs
{
    public class GlobalSDLAbstKey : AbstKey , IAbstGlobalKey
    {
        public GlobalSDLAbstKey()
        {
            var framework = new SdlKey();
            Init(framework);
        }
    }
  
}
