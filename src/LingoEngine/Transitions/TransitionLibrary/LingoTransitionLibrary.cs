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
            { 23, new DissolveTransition(23, "Dissolve Pixels Fast", "dissolvePixelsFast", "Fast pixel dissolve") },
            { 24, new DissolveTransition(24, "Dissolve Boxy Rects", "dissolveBoxRects", "Boxy rectangle dissolve") },
            { 25, new DissolveTransition(25, "Dissolve Boxy Squares", "dissolveBoxSquares", "Boxy square dissolve") },
            { 26, new DissolveTransition(26, "Dissolve Patterns", "dissolvePatterns", "Pattern based dissolve") },
            { 27, new RandomLinesTransition(27, "Random Rows", "randomRows", "Reveal random horizontal lines", RandomLineOrientation.Horizontal) },
            { 28, new RandomLinesTransition(28, "Random Columns", "randomColumns", "Reveal random vertical lines", RandomLineOrientation.Vertical) },
            { 29, new CoverTransition(29, "Cover Down", "coverDown", "Slide new frame from top", CoverDirection.Down) },
            { 30, new CoverTransition(30, "Cover Down Left", "coverDownLeft", "Slide new frame from top-right", CoverDirection.DownLeft) },
            { 31, new CoverTransition(31, "Cover Down Right", "coverDownRight", "Slide new frame from top-left", CoverDirection.DownRight) },
            { 32, new CoverTransition(32, "Cover Left", "coverLeft", "Slide new frame from right", CoverDirection.Left) },
            { 33, new CoverTransition(33, "Cover Right", "coverRight", "Slide new frame from left", CoverDirection.Right) },
            { 34, new CoverTransition(34, "Cover Up", "coverUp", "Slide new frame from bottom", CoverDirection.Up) },
            { 35, new CoverTransition(35, "Cover Up Left", "coverUpLeft", "Slide new frame from bottom-right", CoverDirection.UpLeft) },
            { 36, new CoverTransition(36, "Cover Up Right", "coverUpRight", "Slide new frame from bottom-left", CoverDirection.UpRight) },
            { 37, new BlindsTransition(37, "Venetian Blinds", "venetianBlinds", "Horizontal blind reveal", BlindOrientation.Horizontal) },
            { 38, new CheckerboardTransition(38, "Checkerboard", "checkerboard", "Reveal in checkerboard pattern") },
            { 39, new StripsTransition(39, "Strips Bottom Build Left", "stripsBottomBuildLeft", "Reveal strips from bottom left", StripsDirection.LeftToRight) },
            { 40, new StripsTransition(40, "Strips Bottom Build Right", "stripsBottomBuildRight", "Reveal strips from bottom right", StripsDirection.RightToLeft) },
            { 41, new StripsTransition(41, "Strips Left Build Down", "stripsLeftBuildDown", "Reveal strips from left downward", StripsDirection.TopToBottom) },
            { 42, new StripsTransition(42, "Strips Left Build Up", "stripsLeftBuildUp", "Reveal strips from left upward", StripsDirection.BottomToTop) },
            { 43, new StripsTransition(43, "Strips Right Build Down", "stripsRightBuildDown", "Reveal strips from right downward", StripsDirection.TopToBottom) },
            { 44, new StripsTransition(44, "Strips Right Build Up", "stripsRightBuildUp", "Reveal strips from right upward", StripsDirection.BottomToTop) },
            { 45, new StripsTransition(45, "Strips Top Build Left", "stripsTopBuildLeft", "Reveal strips from top left", StripsDirection.LeftToRight) },
            { 46, new StripsTransition(46, "Strips Top Build Right", "stripsTopBuildRight", "Reveal strips from top right", StripsDirection.RightToLeft) },
            { 47, new ZoomTransition(47, "Zoom Open", "zoomOpen", "Zoom into the new frame", true) },
            { 48, new ZoomTransition(48, "Zoom Close", "zoomClose", "Zoom out to the new frame", false) },
            { 49, new BlindsTransition(49, "Vertical Blinds", "verticalBlinds", "Vertical blind reveal", BlindOrientation.Vertical) },
            { 50, new DissolveTransition(50, "Dissolve Bits Fast", "dissolveBitsFast", "Fast bit dissolve") },
            { 51, new DissolveTransition(51, "Dissolve Pixels", "dissolvePixels", "Pixel dissolve") },
            { 52, new DissolveTransition(52, "Dissolve Bits", "dissolveBits", "Bitwise dissolve") }
        };

    public LingoBaseTransition Get(int id)
    {
        if (_transitions.TryGetValue(id, out var transition))
            return transition;
        return _transitions[0];
    }

    public IEnumerable<LingoBaseTransition> GetAll() => _transitions.Values;
}

