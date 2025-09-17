using Microsoft.Extensions.Logging;
using System.Reflection;

namespace BlingoEngine.Core
{
    public class BlingoGlobalVars
    {
        public BlingoGlobalVars()
        {
            
        }

        public void ClearGlobals()
        {
            OnClearGlobals();
        }

        protected virtual void OnClearGlobals()
        {
        }

        public void ShowGlobals(ILogger logger)
        {
            var type = GetType();
            while (type != null && typeof(BlingoGlobalVars).IsAssignableFrom(type))
            {
                foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                {
                    if (prop.GetIndexParameters().Length > 0)
                        continue;
                    object? value = null;
                    try
                    {
                        value = prop.GetValue(this);
                    }
                    catch
                    {
                    }
                    logger.LogInformation("{Property} = {Value}", prop.Name, value);
                }
                type = type.BaseType;
            }
        }
    }
}

