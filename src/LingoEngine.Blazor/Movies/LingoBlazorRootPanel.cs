using Microsoft.AspNetCore.Components;

namespace LingoEngine.Blazor.Movies;

/// <summary>
/// Holds a reference to the root DOM element where Lingo movies
/// should insert their canvases.
/// </summary>
public class LingoBlazorRootPanel
{
    private ElementReference _root;

    /// <summary>
    /// Gets the root element reference.
    /// </summary>
    public ElementReference Root => _root;

    /// <summary>
    /// Updates the root element reference. Called by the root component
    /// once it has been rendered.
    /// </summary>
    /// <param name="element">The element that should host movie canvases.</param>
    public void SetRoot(ElementReference element) => _root = element;
}

