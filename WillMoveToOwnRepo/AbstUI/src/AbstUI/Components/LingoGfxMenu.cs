namespace AbstUI.Components
{
    /// <summary>
    /// Engine level wrapper around a framework menu object.
    /// </summary>
    public class AbstUIGfxMenu : AbstUIGfxNodeLayoutBase<IAbstUIFrameworkGfxMenu>
    {
        public void AddItem(AbstUIGfxMenuItem item) => _framework.AddItem(item.Framework);
        public void ClearItems() => _framework.ClearItems();
        public void Popup() => _framework.Popup();
        public void PositionPopup(AbstUIGfxButton button)
            => _framework.PositionPopup(button.Framework<IAbstUIFrameworkGfxButton>());
    }
}
