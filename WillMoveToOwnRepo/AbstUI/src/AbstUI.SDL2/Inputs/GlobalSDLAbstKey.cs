using AbstUI.Inputs;

namespace AbstUI.SDL2.Inputs
{
    public class GlobalSDLAbstKey : AbstKey, IAbstGlobalKey
    {
        private readonly SdlKey _framework;

        public GlobalSDLAbstKey()
        {
            _framework = new SdlKey();
            Init(_framework);
        }

        internal SdlKey Framework => _framework;
    }

}
