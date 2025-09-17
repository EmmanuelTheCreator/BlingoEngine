# Film Loops

Film loops are reusable animations composed of sprites or sounds that play in a loop independent of the score timeline. They are roughly equivalent to movie clips in other engines and act like mini‑movies that can be dropped onto the stage or nested inside other film loops.

## Using a Film Loop

Create a `BlingoFilmLoopMember` in a cast and add it to a sprite just like any other member. Each sprite entry describes a channel, start/end frames and an optional transform.

```csharp
var birdBitmap = cast.Add<BlingoBitmapMember>(1, "Bird");
var birdLoop = cast.Add<BlingoFilmLoopMember>(2, "BirdAnim", fl =>
{
    fl.Loop = true;
    fl.FrameCount = 4;
    fl.AddSprite(new BlingoFilmLoopMemberSprite(birdBitmap)
        { Channel = 1, Begin = 1, End = 4 });
});

movie.AddSprite(10, 1, 120, 200, 150,
    sprite => sprite.SetMember(birdLoop));
```

The loop above plays its internal frames continuously while the main movie advances. Sprites that reference a film loop member can be positioned, scaled or rotated like any other sprite.

## Inner Workings

At runtime each framework implementation converts the film loop into textures. `BlingoFilmLoopComposer` prepares every frame by layering the member sprites and applying transforms. The engine then advances an independent frame counter for the loop and draws the resulting texture whenever the owning sprite is shown on stage.

Because the film loop renders to its own off‑screen buffer, expensive drawing work happens only when the loop changes frame. Static frames are cached and reused which keeps rendering fast even with many loops on stage.

## Nested Film Loops

A film loop entry can itself reference another film loop member. This allows building complex animations from small pieces—an animation inside an animation.

```csharp
var wingLoop = cast.Add<BlingoFilmLoopMember>(3, "Wing",
    fl => fl.AddSprite(new BlingoFilmLoopMemberSprite(wingBitmap)
        { Channel = 1, Begin = 1, End = 6 }));

var birdLoop = cast.Add<BlingoFilmLoopMember>(4, "Bird",
    fl => fl.AddSprite(new BlingoFilmLoopMemberSprite(wingLoop)
        { Channel = 1, Begin = 1, End = 6 }));
```

When the outer `Bird` loop plays, the inner `Wing` loop keeps its own timing. Bounding boxes are calculated recursively so the outer loop expands to contain the full area covered by the nested loop.

Nested loops make it easy to compose animations: a character can consist of separate film loops for limbs or facial expressions and those loops can be swapped or reused across different characters.


