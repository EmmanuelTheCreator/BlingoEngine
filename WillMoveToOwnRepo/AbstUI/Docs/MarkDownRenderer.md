# AbstMarkdownRenderer

`AbstMarkdownRenderer` draws a subset of Markdown on an `AbstGfxCanvas`. It understands normal Markdown constructs alongside a set of custom tags for font control and styling.

## Supported Markdown

- Headings using `#`, `##` and `###`
- Paragraph text
- **Bold** via `**bold**`
- *Italic* via `*italic*`
- __Underline__ via `__underline__`
- Images `![alt](path size=100x50)` or without the `size` parameter for natural dimensions

## Custom Tags

Custom tags are wrapped in double braces to avoid clashing with standard Markdown:

- `{{FONT-SIZE:20}}` – change font size
- `{{FONT-FAMILY:Arial}}` – switch font family
- `{{ALIGN:left|center|right|justify}}` – set text alignment
- `{{COLOR:#RRGGBB}}` – set text color
- `{{STYLE:name}}` and `{{/STYLE}}` – push and pop a named style supplied by the host application
- `{{PARA:id}}` – start a new paragraph using the style with identifier `id`; `{{PARA}}` starts a new paragraph without changing style

Example:

```
{{PARA:0}}First paragraph
{{PARA:1}}Second paragraph using style 1
{{PARA}}Third paragraph keeps the current style

{{FONT-FAMILY:Arial}}
{{FONT-SIZE:18}}
# Heading

Normal paragraph with **bold**, *italic* and __underline__.

{{COLOR:#00FF00}}Colored text{{COLOR:#000000}}

{{ALIGN:center}}
![logo](images/logo.png size=128x64)
{{ALIGN:left}}

{{STYLE:quote}}
This paragraph uses a predefined style.
{{/STYLE}}
```

Styles are supplied when preparing the renderer:

```csharp
var styles = new Dictionary<string, AbstTextStyle>
{
    ["quote"] = new AbstTextStyle { FontSize = 14, MarginLeft = 20, Italic = true }
};
var renderer = new AbstMarkdownRenderer(fontManager, imageLoader);
renderer.SetText(markdown, styles);
renderer.Render(canvas, new APoint(0, 0));
```

When only one style is provided and the text has no special tags, `DoFastRendering`
is enabled and the renderer draws the text in a single fast pass.
