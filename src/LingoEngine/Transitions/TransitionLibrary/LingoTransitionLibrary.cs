using System.Collections.Generic;

namespace LingoEngine.Transitions.TransitionLibrary;

public class LingoTransitionLibrary : ILingoTransitionLibrary
{
    private readonly Dictionary<int, LingoBaseTransition> _transitions =
        new()
        {
            { 0, new FadeTransition() },
            { 1, new WipeTransition(1, "Wipe Right", "wipeRight", "Reveal from left to right", WipeDirection.Right) },
            { 2, new WipeTransition(2, "Wipe Left", "wipeLeft", "Reveal from right to left", WipeDirection.Left) },
            { 3, new WipeTransition(3, "Wipe Down", "wipeDown", "Reveal from top to bottom", WipeDirection.Down) },
            { 4, new WipeTransition(4, "Wipe Up", "wipeUp", "Reveal from bottom to top", WipeDirection.Up) },
            { 5, new CenterSplitTransition(5, "Center Out Horizontal", "centerOutHorizontal", "Reveal from center horizontally", SplitOrientation.Horizontal, SplitDirection.Out) },
            { 6, new CenterSplitTransition(6, "Edges In Horizontal", "edgesInHorizontal", "Hide toward center horizontally", SplitOrientation.Horizontal, SplitDirection.In) },
            { 7, new CenterSplitTransition(7, "Center Out Vertical", "centerOutVertical", "Reveal from center vertically", SplitOrientation.Vertical, SplitDirection.Out) },
            { 8, new CenterSplitTransition(8, "Edges In Vertical", "edgesInVertical", "Hide toward center vertically", SplitOrientation.Vertical, SplitDirection.In) },
            { 9, new BoxTransition(9, "Box Out", "boxOut", "Expand from the center", BoxDirection.Out) },
            { 10, new BoxTransition(10, "Box In", "boxIn", "Converge to the center", BoxDirection.In) },
            { 11, new PushTransition(11, "Push Left", "pushLeft", "Slide new frame in from right", PushDirection.Left) },
            { 12, new PushTransition(12, "Push Right", "pushRight", "Slide new frame in from left", PushDirection.Right) },
            { 13, new PushTransition(13, "Push Down", "pushDown", "Slide new frame in from top", PushDirection.Down) },
            { 14, new PushTransition(14, "Push Up", "pushUp", "Slide new frame in from bottom", PushDirection.Up) },
            { 15, new RevealTransition(15, "Reveal Up", "revealUp", "Reveal from bottom to top", RevealDirection.Up) },
            { 16, new RevealTransition(16, "Reveal Up Right", "revealUpRight", "Reveal from bottom-left to top-right", RevealDirection.UpRight) },
            { 17, new RevealTransition(17, "Reveal Right", "revealRight", "Reveal from left to right", RevealDirection.Right) },
            { 18, new RevealTransition(18, "Reveal Down Right", "revealDownRight", "Reveal from top-left to bottom-right", RevealDirection.DownRight) },
            { 19, new RevealTransition(19, "Reveal Down", "revealDown", "Reveal from top to bottom", RevealDirection.Down) },
            { 20, new RevealTransition(20, "Reveal Down Left", "revealDownLeft", "Reveal from top-right to bottom-left", RevealDirection.DownLeft) },
            { 21, new RevealTransition(21, "Reveal Left", "revealLeft", "Reveal from right to left", RevealDirection.Left) },
            { 22, new RevealTransition(22, "Reveal Up Left", "revealUpLeft", "Reveal from bottom-right to top-left", RevealDirection.UpLeft) },
            { 23, new DissolveTransition(23, "Dissolve", "dissolve", "Random pixel dissolve") },
            { 27, new RandomLinesTransition(27, "Random Rows", "randomRows", "Reveal random horizontal lines", RandomLineOrientation.Horizontal) },
            { 28, new RandomLinesTransition(28, "Random Columns", "randomColumns", "Reveal random vertical lines", RandomLineOrientation.Vertical) },
            { 29, new CoverTransition(29, "Cover Down", "coverDown", "Slide new frame from top", CoverDirection.Down) },
            { 33, new CoverTransition(33, "Cover Right", "coverRight", "Slide new frame from left", CoverDirection.Right) },
            { 37, new BlindsTransition(37, "Venetian Blinds", "venetianBlinds", "Horizontal blind reveal", BlindOrientation.Horizontal) },
            { 38, new CheckerboardTransition(38, "Checkerboard", "checkerboard", "Reveal in checkerboard pattern") },
            { 39, new StripsTransition(39, "Strips Left to Right", "stripsLeftRight", "Reveal vertical strips from left", StripsDirection.LeftToRight) },
            { 45, new StripsTransition(45, "Strips Top to Bottom", "stripsTopBottom", "Reveal horizontal strips from top", StripsDirection.TopToBottom) },
            { 47, new ZoomTransition(47, "Zoom Open", "zoomOpen", "Zoom into the new frame", true) },
            { 48, new ZoomTransition(48, "Zoom Close", "zoomClose", "Zoom out to the new frame", false) },
            { 49, new BlindsTransition(49, "Vertical Blinds", "verticalBlinds", "Vertical blind reveal", BlindOrientation.Vertical) }
        };

    public LingoBaseTransition Get(int id)
    {
        if (_transitions.TryGetValue(id, out var transition))
            return transition;
        return _transitions[0];
    }

    public IEnumerable<LingoBaseTransition> GetAll() => _transitions.Values;
}

