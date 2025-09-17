using System.Collections;
using BlingoEngine.Primitives;
using BlingoEngine.Tools;
using static BlingoEngine.Sprites.BehaviorPropertiesContainer;

namespace BlingoEngine.Sprites;
/// <summary>
/// Container for behavior properties set by the user. Holds the values
/// and, optionally, the description list returned by
/// <c>getPropertyDescriptionList</c>.
/// </summary>
public class BehaviorPropertiesContainer : IEnumerable<BlingoPropertyItem>
{
    /// <summary>A single property entry.</summary>
    public class BlingoPropertyItem
    {
        public BlingoSymbol Key { get; set; } = BlingoSymbol.Empty;
        public object? Value { get; set; }
    }

    private readonly List<BlingoPropertyItem> _items = new();

    public void Apply(BehaviorPropertyDescriptionList definitions)
    {
        foreach (var definition in definitions)
        {
            var newValue = this[definition.Key];
            if (newValue != null)
                definition.Value.ApplyValue(newValue);
        }
    }

    /// <summary>Gets or sets a property by key.</summary>
    public object? this[BlingoSymbol key]
    {
        get => _items.FirstOrDefault(i => i.Key.Equals(key))?.Value;
        set
        {
            var idx = _items.FindIndex(i => i.Key.Equals(key));
            if (idx >= 0)
                _items[idx].Value = value;
            else
                _items.Add(new BlingoPropertyItem { Key = key, Value = value });
        }
    }
    public BehaviorPropertiesContainer Add(BlingoSymbol key, object? value)
    {
        _items.Add(new BlingoPropertyItem { Key = key, Value = value });
        return this;
    }
    public BehaviorPropertiesContainer Remove(BlingoSymbol key)
    {
        var idx = _items.FindIndex(i => i.Key.Equals(key));
        if (idx >= 0)
            _items.RemoveAt(idx);
        return this;
    }

    /// <summary>Enumerates all property keys.</summary>
    public IEnumerable<BlingoSymbol> Keys => _items.Select(i => i.Key);

    /// <summary>The number of stored properties.</summary>
    public int Count => _items.Count;

    public IEnumerator<BlingoPropertyItem> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public BehaviorPropertiesContainer Clone()
    {
        var clone = new BehaviorPropertiesContainer();
        foreach (var item in _items)
            clone.Add(item.Key, item.Value);
        
        return clone;
    }

   
}

