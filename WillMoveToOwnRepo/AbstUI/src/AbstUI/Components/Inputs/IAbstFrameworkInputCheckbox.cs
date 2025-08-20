namespace AbstUI.Components.Inputs
{
    /// <summary>
    /// Framework specific checkbox input.
    /// </summary>
    public interface IAbstFrameworkInputCheckbox : IAbstFrameworkNodeInput
    {
        bool Checked { get; set; }
    }
}
