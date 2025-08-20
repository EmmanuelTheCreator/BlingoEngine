using AbstUI.Components.Buttons;

namespace AbstUI.Components.Menus
{
    /// <summary>
    /// Engine level wrapper around a framework menu object.
    /// </summary>
    public class AbstMenu : AbstNodeLayoutBase<IAbstFrameworkMenu>
    {
        public void AddItem(AbstMenuItem item) => _framework.AddItem(item.Framework);
        public void ClearItems() => _framework.ClearItems();
        public void Popup() => _framework.Popup();
        public void PositionPopup(AbstButton button)
            => _framework.PositionPopup(button.Framework<IAbstFrameworkButton>());
    }
}
