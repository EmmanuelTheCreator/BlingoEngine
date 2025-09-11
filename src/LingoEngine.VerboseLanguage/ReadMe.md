# LingoEngine.VerboseLanguage
Package if you want to use the Verbose Lingo Language. Know that this is much slower than the standard .NET Language.

## examples

```csharp
// The verbose language
Set(The().WidthMember.Of.Member("T_data")).To(191);
Set(The().WidthMember.Of.Member(56,2)).To(473); // text memberLoading

// Lingo : put round into field "round"
var round = 20;
Put(round).Into.Field("round");
var roundText = Get(The().Text.Of.Member("round"));

// put the Text of member "Paul Robeson" into member "How Deep"
Put(The().Text.Of.Member("Paul Robeson")).Into.Field("How Deep");

// not:
var whichSprite = 3;

// lingo
set the visibility of sprite whichSprite = not the visibility of sprite whichSprite
// c#
Set(The().Visibility.Of.Sprite(whichSprite)).Value = Not(The().Visibility.Of.Sprite(whichSprite));

``` 