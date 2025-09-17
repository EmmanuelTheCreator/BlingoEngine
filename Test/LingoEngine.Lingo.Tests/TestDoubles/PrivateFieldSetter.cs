using System;
using System.Reflection;

namespace LingoEngine.Lingo.Tests.TestDoubles;

internal static class PrivateFieldSetter
{
    internal static void SetField(object target, string fieldName, object? value)
    {
        var type = target.GetType();
        while (type != null)
        {
            var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field != null)
            {
                field.SetValue(target, value);
                return;
            }

            type = type.BaseType;
        }

        throw new InvalidOperationException($"Field '{fieldName}' not found on type '{target.GetType()}'.");
    }
}
