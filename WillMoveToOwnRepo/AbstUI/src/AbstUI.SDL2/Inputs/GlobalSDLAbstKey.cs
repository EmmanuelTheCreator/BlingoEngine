using AbstUI.SDL2.Inputs;

namespace AbstUI.Inputs
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
