namespace AbstUI.Components
{
    /// <summary>
    /// Engine level wrapper for a checkbox input.
    /// </summary>
    public class AbstInputCheckbox : AbstInputBase<IAbstFrameworkInputCheckbox>
    {

        public bool Checked { get => _framework.Checked; set => _framework.Checked = value; }
    }
}
