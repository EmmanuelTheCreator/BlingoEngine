using System;
using System.Collections.Generic;

namespace LingoEngine.Director.Client.ConsoleTest;

/// <summary>
/// Provides sample cast data for the console client.
/// </summary>
public static class TestCastBuilder
{
    /// <summary>
    /// Builds a dictionary of sample cast members keyed by cast name.
    /// </summary>
    public static Dictionary<string, List<CastMemberInfo>> BuildCastData()
    {
        return new Dictionary<string, List<CastMemberInfo>>
        {
            ["TestCast"] = new List<CastMemberInfo>
            {
                new("Greeting", 1, "Text", DateTime.Now.ToShortDateString(), string.Empty),
                new("Info", 2, "Text", DateTime.Now.ToShortDateString(), string.Empty),
                new("Box", 3, "Shape", DateTime.Now.ToShortDateString(), string.Empty)
            },
            ["ExtraCast"] = new List<CastMemberInfo>
            {
                new("Note", 1, "Text", DateTime.Now.ToShortDateString(), string.Empty)
            }
        };
    }
}

