using System.Linq.Expressions;
using AbstUI.Primitives;
using BlingoEngine.Primitives;

namespace BlingoEngine.Sprites;

public class BehaviorPropertyDescriptionList : BlingoPropertyList<BlingoPropertyDescription>
{
    public BehaviorPropertyDescriptionList Add<TBehavior>(TBehavior behavior, Expression<Func<TBehavior, string?>> property, string? comment = null, string? @default = null)
        where TBehavior : BlingoSpriteBehavior
    {
        var stringProp = new BlingoPropertyDescription<TBehavior,string>(behavior, BlingoSymbol.String, comment, property, @default);
        Add(stringProp.Key, stringProp);
        return this;
    } 
    public BehaviorPropertyDescriptionList Add<TBehavior>(TBehavior behavior, Expression<Func<TBehavior, int>> property, string? comment = null, int @default = 0)
        where TBehavior : BlingoSpriteBehavior
    {
        var stringProp = new BlingoPropertyDescription<TBehavior, int>(behavior, BlingoSymbol.Int, comment, property, @default);
        Add(stringProp.Key, stringProp);
        return this;
    } 
    public BehaviorPropertyDescriptionList Add<TBehavior>(TBehavior behavior, Expression<Func<TBehavior, float>> property, string? comment = null, int @default = 0)
        where TBehavior : BlingoSpriteBehavior
    {
        var stringProp = new BlingoPropertyDescription<TBehavior, float>(behavior, BlingoSymbol.Float, comment, property, @default);
        Add(stringProp.Key, stringProp);
        return this;
    }
    public BehaviorPropertyDescriptionList Add<TBehavior>(TBehavior behavior, Expression<Func<TBehavior, bool>> property, string? comment = null, bool @default = false)
        where TBehavior : BlingoSpriteBehavior
    {
        var stringProp = new BlingoPropertyDescription<TBehavior, bool>(behavior, BlingoSymbol.Boolean, comment, property, @default);
        Add(stringProp.Key, stringProp);
        return this;
    }
    public BehaviorPropertyDescriptionList Add<TBehavior>(TBehavior behavior, Expression<Func<TBehavior, AColor>> property, string? comment = null, AColor @default =  new AColor())
        where TBehavior : BlingoSpriteBehavior
    {
        var stringProp = new BlingoPropertyDescription<TBehavior, AColor>(behavior, BlingoSymbol.Color, comment, property, @default);
        Add(stringProp.Key, stringProp);
        return this;
    }
}

