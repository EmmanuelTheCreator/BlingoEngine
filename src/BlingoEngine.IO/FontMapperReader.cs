using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using AbstUI.Primitives;
using AbstUI.Styles;

namespace BlingoEngine.IO;

/// <summary>
/// Reads Director font map definitions and exposes the parsed mappings for consumers.
/// </summary>
public static class FontMapperReader
{
    public static FontMapDocument ReadFromFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty", nameof(path));
        using var stream = File.OpenRead(path);
        return Read(stream);
    }

    public static FontMapDocument Read(Stream stream)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));
        using var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true);
        return Read(reader);
    }

    public static FontMapDocument Read(TextReader reader)
    {
        if (reader == null)
            throw new ArgumentNullException(nameof(reader));

        var fontMappings = new List<AbstFontMap>();
        var keyMappings = new Dictionary<(ARuntimePlatform Source, ARuntimePlatform Target), Dictionary<int, int>>();
        bool inFontMappings = false;
        bool inCharacterMappings = false;

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var trimmed = line.Trim();
            if (trimmed.StartsWith(";", StringComparison.Ordinal))
            {
                var header = trimmed[1..].Trim();
                if (header.Equals("FONT MAPPINGS", StringComparison.OrdinalIgnoreCase))
                {
                    inFontMappings = true;
                    inCharacterMappings = false;
                }
                else if (header.Equals("CHARACTER MAPPINGS", StringComparison.OrdinalIgnoreCase))
                {
                    inFontMappings = false;
                    inCharacterMappings = true;
                }
                continue;
            }

            if (inFontMappings)
            {
                if (TryParseFontMapping(trimmed, out var mapping))
                    fontMappings.Add(mapping);
                continue;
            }

            if (inCharacterMappings)
            {
                TryParseCharacterMapping(trimmed, keyMappings);
            }
        }

        var inputMaps = new List<AbstInputKeyMap>(keyMappings.Count);
        foreach (var kvp in keyMappings)
        {
            inputMaps.Add(new AbstInputKeyMap(kvp.Key.Source, kvp.Key.Target, kvp.Value));
        }

        return new FontMapDocument(fontMappings, inputMaps);
    }

    public static FontMapDocument ReadFromDirectory(string directory, int version)
    {
        if (directory == null)
            throw new ArgumentNullException(nameof(directory));
        var fileName = GetFontMapFileName(version);
        var path = Path.Combine(directory, fileName);
        if (!File.Exists(path))
            return FontMapDocument.Empty;
        return ReadFromFile(path);
    }

    public static FontMapDocument ReadFromApplicationDirectory(int version)
    {
        var baseDir = Path.Combine(AppContext.BaseDirectory, "fontmaps");
        return ReadFromDirectory(baseDir, version);
    }

    public static string GetFontMapFileName(int version) => version switch
    {
        >= 1150 => "fontmap_D11_5.txt",
        >= 1100 => "fontmap_D11.txt",
        >= 1000 => "fontmap_D10.txt",
        >= 900 => "fontmap_D9.txt",
        >= 850 => "fontmap_D8_5.txt",
        >= 800 => "fontmap_D8.txt",
        >= 700 => "fontmap_D7.txt",
        _ => "fontmap_D6.txt",
    };

    private static bool TryParseFontMapping(string line, out AbstFontMap map)
    {
        map = null!;
        int arrowIndex = line.IndexOf("=>", StringComparison.Ordinal);
        if (arrowIndex < 0)
            return false;

        var left = line[..arrowIndex];
        var right = line[(arrowIndex + 2)..];

        if (!TryParseEndpoint(left, out var sourcePlatform, out var sourceFont, out _))
            return false;
        if (!TryParseEndpoint(right, out var targetPlatform, out var targetFont, out var remainder))
            return false;

        bool mapCharacters = true;
        var sizeMappings = new Dictionary<int, int>();
        var rest = remainder.TrimStart();
        while (!string.IsNullOrWhiteSpace(rest))
        {
            rest = rest.TrimStart();
            if (rest.Length == 0)
                break;

            if (rest.StartsWith("Map", StringComparison.OrdinalIgnoreCase))
            {
                rest = rest[3..].TrimStart();
                if (rest.StartsWith("None", StringComparison.OrdinalIgnoreCase))
                {
                    mapCharacters = false;
                    rest = rest.Length > 4 ? rest[4..] : string.Empty;
                }
                continue;
            }

            int splitIndex = 0;
            while (splitIndex < rest.Length && !char.IsWhiteSpace(rest[splitIndex]))
                splitIndex++;
            var token = splitIndex > 0 ? rest[..splitIndex] : rest;
            rest = splitIndex < rest.Length ? rest[splitIndex..] : string.Empty;
            token = token.Trim();
            if (token.Length == 0)
                continue;
            var arrow = token.IndexOf("=>", StringComparison.Ordinal);
            if (arrow < 0)
                continue;
            var fromPart = token[..arrow].Trim();
            var toPart = token[(arrow + 2)..].Trim();
            if (int.TryParse(fromPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out var fromSize)
                && int.TryParse(toPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out var toSize))
            {
                sizeMappings[fromSize] = toSize;
            }
        }

        map = new AbstFontMap(sourcePlatform, sourceFont, targetPlatform, targetFont, mapCharacters, sizeMappings);
        return true;
    }

    private static void TryParseCharacterMapping(
        string line,
        Dictionary<(ARuntimePlatform Source, ARuntimePlatform Target), Dictionary<int, int>> accumulator)
    {
        int arrowIndex = line.IndexOf("=>", StringComparison.Ordinal);
        if (arrowIndex < 0)
            return;

        var left = line[..arrowIndex];
        var right = line[(arrowIndex + 2)..];
        if (!TryParsePlatformName(left, out var sourcePlatform))
            return;
        if (!TryParsePlatformAndRemainder(right, out var targetPlatform, out var remainder))
            return;
        remainder = remainder.Trim();
        if (remainder.Length == 0)
            return;

        if (!accumulator.TryGetValue((sourcePlatform, targetPlatform), out var map))
        {
            map = new Dictionary<int, int>();
            accumulator[(sourcePlatform, targetPlatform)] = map;
        }

        var tokens = remainder.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (var token in tokens)
        {
            var arrow = token.IndexOf("=>", StringComparison.Ordinal);
            if (arrow < 0)
                continue;
            var fromPart = token[..arrow].Trim();
            var toPart = token[(arrow + 2)..].Trim();
            if (int.TryParse(fromPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out var from)
                && int.TryParse(toPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out var to))
            {
                map[from] = to;
            }
        }
    }

    private static bool TryParseEndpoint(string text, out ARuntimePlatform platform, out string fontName, out string remainder)
    {
        platform = default;
        fontName = string.Empty;
        remainder = string.Empty;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        text = text.Trim();
        int colonIndex = text.IndexOf(':');
        if (colonIndex < 0)
            return false;

        var platformToken = text[..colonIndex].Trim();
        if (!TryGetPlatform(platformToken, out platform))
            return false;

        var afterColon = text[(colonIndex + 1)..].TrimStart();
        if (afterColon.Length == 0)
        {
            fontName = string.Empty;
            remainder = string.Empty;
            return true;
        }

        if (afterColon[0] == '\"')
        {
            int endQuote = 1;
            while (endQuote < afterColon.Length && afterColon[endQuote] != '\"')
                endQuote++;
            if (endQuote >= afterColon.Length)
                return false;
            fontName = afterColon.Substring(1, endQuote - 1);
            remainder = afterColon[(endQuote + 1)..];
            return true;
        }

        int index = 0;
        while (index < afterColon.Length && !char.IsWhiteSpace(afterColon[index]))
        {
            if (afterColon[index] == '=' && index + 1 < afterColon.Length && afterColon[index + 1] == '>')
                break;
            index++;
        }
        fontName = afterColon[..index].TrimEnd();
        remainder = afterColon[index..];
        return true;
    }

    private static bool TryParsePlatformName(string text, out ARuntimePlatform platform)
    {
        platform = default;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        text = text.Trim();
        int colonIndex = text.IndexOf(':');
        var token = colonIndex < 0 ? text : text[..colonIndex].Trim();
        return TryGetPlatform(token, out platform);
    }

    private static bool TryParsePlatformAndRemainder(string text, out ARuntimePlatform platform, out string remainder)
    {
        platform = default;
        remainder = string.Empty;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        text = text.Trim();
        int colonIndex = text.IndexOf(':');
        if (colonIndex < 0)
            return TryGetPlatform(text, out platform);

        var token = text[..colonIndex].Trim();
        if (!TryGetPlatform(token, out platform))
            return false;
        remainder = text[(colonIndex + 1)..];
        return true;
    }

    private static bool TryGetPlatform(string value, out ARuntimePlatform platform)
    {
        platform = default;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        switch (value.Trim().ToLowerInvariant())
        {
            case "mac":
            case "macintosh":
                platform = ARuntimePlatform.Mac;
                return true;
            case "win":
            case "windows":
                platform = ARuntimePlatform.Win;
                return true;
            default:
                return false;
        }
    }
}

public sealed record class FontMapDocument(IReadOnlyList<AbstFontMap> FontMappings, IReadOnlyList<AbstInputKeyMap> InputKeyMappings)
{
    public static FontMapDocument Empty { get; } = new(Array.Empty<AbstFontMap>(), Array.Empty<AbstInputKeyMap>());
}
