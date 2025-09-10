# Fonts in AbstUI

AbstUI provides a font management system through the `IAbstFontManager` interface. Fonts are registered during application setup and loaded by the concrete backend implementations (SDL2, Godot, Unity, ImGui, and Blazor).

## Registering Fonts

Fonts are added via `AddFont`, which now accepts an optional `AbstFontStyle` flag parameter:

```csharp
fontManager.AddFont("Roboto", "Fonts/Roboto.ttf", style: AbstFontStyle.Bold);
fontManager.AddFont("Roboto", "Fonts/Roboto-Italic.ttf", style: AbstFontStyle.Italic);
```

Styles can be combined with bitwise OR, for example `AbstFontStyle.Bold | AbstFontStyle.Italic`.
If no style is specified, the font is treated as the default style for its name.

## Retrieving Fonts

Loaded fonts can be retrieved using the generic `Get` method. The same optional style flag can be supplied to request a specific styled variant:

```csharp
var boldFont = fontManager.Get<Font>("Roboto", style: AbstFontStyle.Bold);
```

If the style is omitted, the default style for the font name is returned.

## Default Fonts

Each font manager maintains a default font used when no name is provided. This default can be retrieved and changed with `GetDefaultFont` and `SetDefaultFont`.

## Measuring Text

All font managers expose methods to measure text width and obtain basic metrics:

- `MeasureTextWidth(string text, string fontName, int fontSize, AbstFontStyle style = AbstFontStyle.Regular)`
- `GetFontInfo(string fontName, int fontSize, AbstFontStyle style = AbstFontStyle.Regular)`

These APIs respect the previously added fonts and their styles, enabling consistent text layout across backends.
