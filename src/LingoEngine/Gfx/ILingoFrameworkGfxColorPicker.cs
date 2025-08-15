namespace LingoEngine.Gfx
{
    /// <summary>
    /// Framework specific color picker input.
    /// </summary>
    public interface ILingoFrameworkGfxColorPicker : ILingoFrameworkGfxNodeInput
    {
        /// <summary>The currently selected color.</summary>

/* Unmerged change from project 'LingoEngine (net8.0)'
Before:
        Primitives.LingoColor Color { get; set; }
    }
After:
        LingoColor Color { get; set; }
    }
*/
        LingoEngine.AbstUI.Primitives.AColor Color { get; set; }
    }
}
