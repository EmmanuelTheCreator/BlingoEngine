using AbstUI.Inputs;

namespace AbstUI.Blazor.Inputs
{
    public class GlobalBlazorKey : AbstKey, IAbstGlobalKey
    {
        private readonly BlazorKey _framework;

        public GlobalBlazorKey()
        {
            _framework = new BlazorKey();
            Init(_framework);
        }

        internal BlazorKey Framework => _framework;
    }

}
