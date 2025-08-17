using System;
using System.Collections.Generic;
using System.Numerics;
using AbstUI.Components;
using AbstUI.Primitives;

namespace AbstUI.SDL2.Components
{
    internal class AbstSdlInputCombobox : AbstSdlComponent, IAbstFrameworkInputCombobox, IDisposable
    {
        public AbstSdlInputCombobox(AbstSdlComponentFactory factory) : base(factory)
        {
        }
        public bool Enabled { get; set; } = true;
        public AMargin Margin { get; set; } = AMargin.Zero;

        private readonly List<KeyValuePair<string, string>> _items = new();
        public IReadOnlyList<KeyValuePair<string, string>> Items => _items;
        public int SelectedIndex { get; set; } = -1;
        public string? SelectedKey { get; set; }
        public string? SelectedValue { get; set; }

        public event Action? ValueChanged;
        public object FrameworkNode => this;
        public void AddItem(string key, string value)
        {
            _items.Add(new KeyValuePair<string, string>(key, value));
        }

        public void ClearItems()
        {
            _items.Clear();
            SelectedIndex = -1;
            SelectedKey = null;
            SelectedValue = null;
        }

        public override AbstSDLRenderResult Render(AbstSDLRenderContext context)
        {
            if (!Visibility) return default;
            return default;
        }

        public override void Dispose() => base.Dispose();
    }
}
