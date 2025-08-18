namespace UnityEngine.UI;

using System;
using UnityEngine;

public class Image : MonoBehaviour
{
    public Sprite? sprite { get; set; }
}

public class Button : MonoBehaviour
{
    public class ButtonClickedEvent
    {
        private event Action? _handlers;
        public void AddListener(Action handler) => _handlers += handler;
        public void RemoveListener(Action handler) => _handlers -= handler;
        public void Invoke() => _handlers?.Invoke();
    }

    public bool interactable { get; set; } = true;
    public ButtonClickedEvent onClick { get; } = new();
}

public class Text : MonoBehaviour
{
    public string text { get; set; } = string.Empty;
}

public class InputField : MonoBehaviour
{
    public class SubmitEvent
    {
        private event Action<string>? _handlers;
        public void AddListener(Action<string> handler) => _handlers += handler;
        public void RemoveListener(Action<string> handler) => _handlers -= handler;
        public void Invoke(string value) => _handlers?.Invoke(value);
    }

    public enum LineType
    {
        SingleLine,
        MultiLineNewline,
        MultiLineSubmit
    }

    public bool interactable { get; set; } = true;
    public string text { get; set; } = string.Empty;
    public int characterLimit { get; set; }
    public LineType lineType { get; set; } = LineType.SingleLine;
    public Text? textComponent { get; set; }
    public SubmitEvent onValueChanged { get; } = new();
}
