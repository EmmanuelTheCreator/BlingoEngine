namespace AbstUI.Components
{
    /// <summary>
    /// Framework specific scroll container implementation.
    /// </summary>
    public interface IAbstUIFrameworkGfxScrollContainer : IAbstUIFrameworkGfxLayoutNode
    {
        void AddItem(IAbstUIFrameworkGfxLayoutNode child);
        void RemoveItem(IAbstUIFrameworkGfxLayoutNode lingoFrameworkGfxNode);
        IEnumerable<IAbstUIFrameworkGfxLayoutNode> GetItems();

        float ScrollHorizontal { get; set; }
        float ScrollVertical { get; set; }
        bool ClipContents { get; set; }
    }
}
