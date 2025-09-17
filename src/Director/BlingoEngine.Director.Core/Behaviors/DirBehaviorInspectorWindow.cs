using System;
using System.Collections.Generic;
using System.Linq;
using AbstUI.Components.Containers;
using AbstUI.Components.Inputs;
using AbstUI.Components.Texts;
using AbstUI.Primitives;
using BlingoEngine.Director.Core.Windowing;
using BlingoEngine.Director.Core.UI;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Sprites;
using BlingoEngine.Sprites.BehaviorLibrary;
using BlingoEngine.Core;
using AbstUI.Windowing;

namespace BlingoEngine.Director.Core.Behaviors;

public class DirBehaviorInspectorWindow : DirectorWindow<IDirFrameworkBehaviorInspectorWindow>
{
    private readonly IBlingoBehaviorLibrary _library;
    private readonly BlingoPlayer _player;

    private readonly AbstWrapPanel _root;
    private readonly AbstInputCombobox _categories;
    private readonly AbstItemList _behaviors;
    private readonly AbstLabel _description;
    private readonly Dictionary<string, BlingoBehaviorDefinition> _behaviorMap = new();

    public DirBehaviorInspectorWindow(IServiceProvider serviceProvider,
        IBlingoBehaviorLibrary library,
        BlingoPlayer player) : base(serviceProvider, DirectorMenuCodes.BehaviorInspectorWindow)
    {
        _library = library;
        _player = player;

        MinimumWidth = 250;
        MinimumHeight = 200;
        Width = 300;
        Height = 400;

        _root = Factory.CreateWrapPanel(AOrientation.Vertical, "BehaviorLibraryRoot");
        _categories = Factory.CreateInputCombobox("BehaviorCategories", OnCategoryChanged);
        _categories.Width = 280;
        _root.AddItem(_categories);

        _behaviors = Factory.CreateItemList("BehaviorList", OnBehaviorSelected);
        _behaviors.Width = 280;
        _behaviors.Height = 250;
        _root.AddItem(_behaviors);

        _description = Factory.CreateLabel("BehaviorDescription", "");
        _description.Width = 280;
        _root.AddItem(_description);

        

        PopulateCategories();
        PopulateBehaviors(null);
    }
    protected override void OnInit(IAbstFrameworkWindow frameworkWindow)
    {
        base.OnInit(frameworkWindow);
        Content = _root;
    }

    private void PopulateCategories()
    {
        _categories.ClearItems();
        _categories.AddItem("All", "All");
        foreach (var cat in _library.GetCategories())
            _categories.AddItem(cat, cat);
    }

    private void OnCategoryChanged(string? category)
    {
        PopulateBehaviors(category);
    }

    private void PopulateBehaviors(string? category)
    {
        _behaviors.ClearItems();
        _behaviorMap.Clear();
        IEnumerable<BlingoBehaviorDefinition> defs = _library.GetAll();
        if (!string.IsNullOrEmpty(category) && category != "All")
            defs = defs.Where(b => b.Category == category);
        foreach (var def in defs.OrderBy(d => d.Name))
        {
            _behaviors.AddItem(def.Name, def.Name);
            _behaviorMap[def.Name] = def;
        }
    }

    private void OnBehaviorSelected(string? key)
    {
        _description.Text = string.Empty;
        if (key == null || !_behaviorMap.TryGetValue(key, out var def))
            return;
        var movie = _player.ActiveMovie;
        if (movie == null) return;
        try
        {
            var method = typeof(IBlingoFrameworkFactory).GetMethod("CreateBehavior")?.MakeGenericMethod(def.BehaviorType);
            if (method == null) return;
            var behavior = (BlingoSpriteBehavior?)method.Invoke(Factory, new object[] { movie });
            if (behavior is IBlingoPropertyDescriptionList desc)
            {
                _description.Text = desc.GetBehaviorDescription() ?? string.Empty;
            }
        }
        catch
        {
            // ignore
        }
    }
}

