using AbstUI.Components;
using AbstUI.Inputs;
using AbstUI.Primitives;
using Microsoft.Extensions.DependencyInjection;

namespace AbstUI.Windowing
{
    public class AbstDialog : AbstNodeLayoutBase<IAbstFrameworkDialog>, IAbstDialog , IAbstMouseRectProvider, IAbstKeyEventHandler<AbstKeyEvent>
    {
        T IAbstDialog.FrameworkObj<T>() => (T)_framework;
        public string Title { get => _framework.Title; set => _framework.Title = value; }
        public AColor BackgroundColor { get => _framework.BackgroundColor; set => _framework.BackgroundColor = value; }
        public bool IsPopup { get => _framework.IsPopup; set => _framework.IsPopup = value; }
        public bool Borderless { get => _framework.Borderless; set => _framework.Borderless = value; }
        public ARect MouseOffset => ARect.New(X, Y, Width, Height);
        public bool IsActivated => Visibility;

        public bool IsOpen  { get => _framework.IsOpen; }
        //public IAbstMouse Mouse => _mouse;
        //public IAbstKey Key => _key;

        public event Action<bool>? OnWindowStateChanged;
        public event Action<float, float>? OnResize;



        public bool IsActiveWindow { get; set; }

        public IAbstMouse Mouse { get; private set; }

        public IAbstKey Key { get; private set; }


        public AbstDialog(IAbstGlobalMouse mouse, IAbstGlobalKey key)
#pragma warning restore CS8618
        {
            Mouse = mouse.CreateNewInstance(this);
            Key = key.CreateNewInstance(this);
            //Mouse = serviceProvider.GetRequiredService<IAbstGlobalMouse>().CreateNewInstance(this);
            //Key = serviceProvider.GetRequiredService<IAbstGlobalKey>().CreateNewInstance(this);
            Key.Subscribe(this);
        }

        public virtual void RaiseKeyDown(AbstKeyEvent key)
        {
            
        }

        public virtual void RaiseKeyUp(AbstKeyEvent key)
        {
            
        }
        public IAbstDialog AddItem(IAbstNode node)
        {
            _framework.AddItem(node.Framework<IAbstFrameworkLayoutNode>());
            return this;
        }

        public IAbstDialog AddItem(IAbstFrameworkLayoutNode node)
        {
            _framework.AddItem(node);
            return this;
        }

        public void RemoveItem(IAbstNode node) => _framework.RemoveItem(node.Framework<IAbstFrameworkLayoutNode>());
        public IAbstDialog RemoveItem(IAbstFrameworkLayoutNode node)
        {
            _framework.RemoveItem(node);
            return this;
        }

        public IEnumerable<IAbstFrameworkLayoutNode> GetItems() => _framework.GetItems();

        public void Popup() => _framework.Popup();

        public void PopupCentered() => _framework.PopupCentered();

        public void Hide() => _framework.Hide();

        

        public void Resize(int width, int height) => OnResize?.Invoke(width, height);

        public void RaiseWindowStateChanged(bool state) => OnWindowStateChanged?.Invoke(state);

       
    }
}

