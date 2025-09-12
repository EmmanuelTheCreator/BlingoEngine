# Fonts in LingoEngine

LingoEngine relies on the `IAbstFontManager` abstraction from the underlying AbstUI library to provide a uniform font API across SDL2, Godot, Unity, and Blazor backends. The manager registers font files, loads platform-specific assets, and exposes helpers for measuring text. Font styles use the flag-based `AbstFontStyle` enum so bold and italic styles can be combined.

## `IAbstFontManager` interface

```csharp
public interface IAbstFontManager
{
    IAbstFontManager AddFont(string name, string pathAndName, AbstFontStyle style = AbstFontStyle.Regular);
    void LoadAll();
    T? Get<T>(string name, AbstFontStyle style = AbstFontStyle.Regular) where T : class;
    T GetDefaultFont<T>() where T : class;
    void SetDefaultFont<T>(T font) where T : class;
    IEnumerable<string> GetAllNames();

    float MeasureTextWidth(string text, string fontName, int fontSize, AbstFontStyle style = AbstFontStyle.Regular);
    FontInfo GetFontInfo(string fontName, int fontSize, AbstFontStyle style = AbstFontStyle.Regular);
}
public readonly record struct FontInfo(int FontHeight, int TopIndentation);
```

## Registering and retrieving fonts

```csharp
var fonts = serviceProvider.GetRequiredService<IAbstFontManager>();
fonts.AddFont("Roboto", "Fonts/Roboto.ttf");
fonts.AddFont("Roboto", "Fonts/Roboto-Bold.ttf", AbstFontStyle.Bold);
fonts.LoadAll();

var width = fonts.MeasureTextWidth("Hello", "Roboto", 14, AbstFontStyle.Bold);
var info  = fonts.GetFontInfo("Roboto", 14);
var defaultFont = fonts.GetDefaultFont<object>(); // backend specific type
```

## Framework implementations

### SDL2
`SdlFontManager` loads fonts through SDL_ttf. Registering a regular font automatically queues bold, italic, and bold‑italic variants, and a built‑in Tahoma family is used as a fallback.

```csharp
fonts.AddFont("OpenSans", "Fonts/OpenSans.ttf");
fonts.LoadAll();
var sdlFont = fonts.Get<AbstSdlFont>("OpenSans");
```

### Godot
`AbstGodotFontManager` loads `FontFile` resources and converts `AbstFontStyle` flags to Godot's `TextServer` styles. The `LingoGodotStyle` theme uses `ARIAL.TTF` as the default font and defines default and per-control sizes.

```csharp
fonts.AddFont("ARIAL", "Fonts/ARIAL.TTF");
fonts.LoadAll();
var labelFont = fonts.Get<FontFile>("ARIAL", AbstFontStyle.Italic);
```

### Unity
`UnityFontManager` reads Unity `Font` assets, falling back to the built‑in Tahoma font when resources are missing. It exposes `MeasureTextWidth` for layout calculations.

```csharp
fonts.AddFont("Roboto", "Fonts/Roboto");
fonts.LoadAll();
Font uiFont = fonts.Get<Font>("Roboto", AbstFontStyle.Bold);
```

### Blazor
`AbstBlazorFontManager` maps font names to CSS font families. Loaded fonts are stored as string references, and a `sans-serif` default is used when no font is specified.

```csharp
fonts.AddFont("Roboto", "url('fonts/Roboto.woff2') format('woff2')");
fonts.LoadAll();
string cssFamily = fonts.Get<string>("Roboto")!;
```

## Font sizes and styles

Font sizes are specified in pixels. For example, the Godot theme sets a default size of 11 and custom sizes for common controls. Style variants such as bold or italic are chosen via the `AbstFontStyle` flags when retrieving fonts.
