using System;
using AbstUI.Primitives;
using LingoEngine.Core;
using LingoEngine.Stages;
using LingoEngine.Transitions.TransitionLibrary;

namespace LingoEngine.Transitions;

public sealed class LingoTransitionPlayer : ILingoTransitionPlayer, IDisposable
{
    private readonly LingoStage _stage;
    private readonly LingoClock _clock;
    private readonly ILingoTransitionLibrary _library;
    private IAbstTexture2D? _from;
    private IAbstTexture2D? _to;
    private IAbstTexture2D? _current;
    private byte[]? _fromPixels;
    private byte[]? _toPixels;
    private byte[]? _targetPixels;
    private ARect _targetRect;
    private bool _computeDiffRect;
    private LingoBaseTransition? _transition;
    private int _tick;
    private int _duration;
    private bool _waitingForToFrame;
    private bool _isCapturingToFrame;
    private bool _isPlaying;

    public bool IsActive => _isPlaying || _waitingForToFrame;

    public LingoTransitionPlayer(LingoStage stage, LingoClock clock, ILingoTransitionLibrary library)
    {
        _stage = stage;
        _clock = clock;
        _library = library;
    }

    public bool Start(LingoTransitionSprite sprite)
    {
        _from?.Dispose();
        _to?.Dispose();
        _from = _to = _current = null;
        _fromPixels = _toPixels = _targetPixels = null;
        _waitingForToFrame = false;
        _isPlaying = false;

        _transition = _library.Get(sprite.Member?.TransitionId ?? 1);
        if (_transition == null)
            return false;

        _from = _stage.GetScreenshot();
        _fromPixels = _from.GetPixels();

        var affects = sprite.Member?.Affects ?? LingoTransitionAffects.EntireStage;
        _computeDiffRect = affects == LingoTransitionAffects.ChangingAreaOnly;
        if (affects == LingoTransitionAffects.Custom)
        {
            _targetRect = ClampRect(sprite.Member!.Rect, _from.Width, _from.Height);
        }
        else
        {
            _targetRect = ARect.New(0, 0, _from.Width, _from.Height);
        }

        // make a copy we can mutate
        _current = _from.Clone();

        _stage.ShowTransition(_from);

        var seconds = sprite.Member?.Duration ?? 1f;
        _duration = Math.Max(1, (int)(seconds * _clock.FrameRate));
        _tick = 0;
        _waitingForToFrame = true;
        _isPlaying = false;
        _isCapturingToFrame = false;
        return true;
    }

    private void CaptureToFrame(IAbstTexture2D texture2D)
    {
        if (!_waitingForToFrame)
            return;
        _to = texture2D;
        _toPixels = _to.GetPixels();
        if (_computeDiffRect)
        {
            _targetRect = ComputeDiffRect(_from!.Width, _from.Height, _fromPixels!, _toPixels);
            if (_targetRect.Width <= 0 || _targetRect.Height <= 0)
                _targetRect = ARect.New(0, 0, _from.Width, _from.Height);
        }
        _targetPixels = PrepareTargetPixels();
        _waitingForToFrame = false;
        _isPlaying = true;
    }

    private byte[] PrepareTargetPixels()
    {
        if (_fromPixels == null || _toPixels == null)
            return _toPixels ?? _fromPixels ?? Array.Empty<byte>();

        var width = _from!.Width;
        var rect = ClampRect(_targetRect, width, _from.Height);
        var result = (byte[])_fromPixels.Clone();
        CopyRectPixels(_toPixels, result, width, rect);
        return result;
    }

    private static void CopyRectPixels(byte[] src, byte[] dest, int width, ARect rect)
    {
        int x = (int)rect.Left;
        int y = (int)rect.Top;
        int w = (int)rect.Width;
        int h = (int)rect.Height;
        for (int row = 0; row < h; row++)
        {
            int srcIdx = ((y + row) * width + x) * 4;
            Buffer.BlockCopy(src, srcIdx, dest, srcIdx, w * 4);
        }
    }

    private static ARect ComputeDiffRect(int width, int height, byte[] from, byte[] to)
    {
        int minX = width;
        int minY = height;
        int maxX = -1;
        int maxY = -1;
        for (int y = 0; y < height; y++)
        {
            int rowIndex = y * width * 4;
            for (int x = 0; x < width; x++)
            {
                int idx = rowIndex + x * 4;
                if (from[idx] != to[idx] || from[idx + 1] != to[idx + 1] || from[idx + 2] != to[idx + 2] || from[idx + 3] != to[idx + 3])
                {
                    if (x < minX) minX = x;
                    if (y < minY) minY = y;
                    if (x > maxX) maxX = x;
                    if (y > maxY) maxY = y;
                }
            }
        }
        if (maxX < minX || maxY < minY)
            return ARect.New(0, 0, width, height);
        return ARect.New(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }

    private static ARect ClampRect(ARect rect, int width, int height)
    {
        int x = (int)Math.Clamp(rect.Left, 0, width);
        int y = (int)Math.Clamp(rect.Top, 0, height);
        int w = (int)Math.Clamp(rect.Width, 0, width - x);
        int h = (int)Math.Clamp(rect.Height, 0, height - y);
        return ARect.New(x, y, w, h);
    }

    public void Tick()
    {
        if (_toPixels == null)
        {
            if (_isCapturingToFrame) return;
            _isCapturingToFrame = true;
            _stage.RequestNextFrameScreenshot(CaptureToFrame);
            return;
        }
        if (!_isPlaying || _fromPixels == null || _targetPixels == null || _from == null || _transition == null)
            return;
        _tick++;
        float progress = (float)_tick / _duration;
        if (progress >= 1f) progress = 1f;
        var blended = _transition.StepFrame(_from.Width, _from.Height, _fromPixels, _targetPixels, progress);
        _current!.SetRGBAPixels(blended);
        _stage.UpdateTransitionFrame(_current, ARect.New(0, 0, _from.Width, _from.Height));
        if (progress >= 1f)
        {
            _stage.HideTransition();
            _from?.Dispose();
            _to?.Dispose();
            _current?.Dispose();
            _current = _from = _to = null;
            _fromPixels = _toPixels = _targetPixels = null;
            _transition = null;
            _computeDiffRect = false;
            _targetRect = ARect.New(0, 0, 0, 0);
            _isPlaying = false;
        }
    }

    public void Dispose()
    {
        _from?.Dispose();
        _to?.Dispose();
        _current?.Dispose();
    }
}
