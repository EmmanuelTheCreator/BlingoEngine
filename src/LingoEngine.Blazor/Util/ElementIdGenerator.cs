using System;

namespace LingoEngine.Blazor.Util;

internal static class ElementIdGenerator
{
    public static string Create(string name)
        => $"{Guid.NewGuid():N}_{name}";
}

