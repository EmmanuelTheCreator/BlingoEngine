namespace AbstUI.Components
{
    /// <summary>
    /// Engine level wrapper for a checkbox input.
    /// </summary>
    public class AbstUIGfxInputCheckbox : AbstUIGfxInputBase<IAbstUIFrameworkGfxInputCheckbox>
    {

        public bool Checked { get => _framework.Checked; set => _framework.Checked = value; }
    }
}
