namespace AbstUI.Components
{
    /// <summary>
    /// Framework specific color picker input.
    /// </summary>
    public interface IAbstFrameworkColorPicker : IAbstFrameworkNodeInput
    {
        /// <summary>The currently selected color.</summary>

        /* Unmerged change from project 'AbstUIEngine (net8.0)'
        Before:
                Primitives.AbstUIColor Color { get; set; }
            }
        After:
                AbstUIColor Color { get; set; }
            }
        */
        Primitives.AColor Color { get; set; }
    }
}
