namespace AbstUI.Components
{
    /// <summary>
    /// Represents a single tab item with a title and content node.
    /// </summary>
    public class AbstTabItem : AbstNodeLayoutBase<IAbstFrameworkTabItem>
    {
        public static int TopTabHeaderHeight = 22;
        public string Title { get => _framework.Title; set => _framework.Title = value; }
        public IAbstNode? Content { get => _framework.Content; set => _framework.Content = value; }
        public float TopHeight { get => _framework.TopHeight; set => _framework.TopHeight = value; }

        public AbstTabItem()
        {
        }
    }
}
