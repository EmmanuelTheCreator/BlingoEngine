namespace AbstUI.Components
{
    /// <summary>
    /// Framework specific scroll container implementation.
    /// </summary>
    public interface IAbstFrameworkScrollContainer : IAbstFrameworkLayoutNode
    {
        void AddItem(IAbstFrameworkLayoutNode child);
        void RemoveItem(IAbstFrameworkLayoutNode child);
        IEnumerable<IAbstFrameworkLayoutNode> GetItems();

        float ScrollHorizontal { get; set; }
        float ScrollVertical { get; set; }
        bool ClipContents { get; set; }
    }
}
