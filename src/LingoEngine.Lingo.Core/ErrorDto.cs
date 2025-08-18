namespace LingoEngine.Lingo.Core;

public record ErrorDto(string File, int LineNumber, string LineText, string Error);
