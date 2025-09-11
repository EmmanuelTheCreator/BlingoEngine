using Microsoft.Extensions.Logging;
using System.Reflection;

namespace LingoEngine.Core
{
    public class LingoGlobalVars
    {
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
            while (type != null && typeof(LingoGlobalVars).IsAssignableFrom(type))
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
