using System;
using System.Collections.Generic;
using LingoEngine.Events;
using LingoEngine.Sprites;
using LingoEngine.Director.Core.Sprites;
using AbstUI.Primitives;
using AbstUI.Inputs;

namespace LingoEngine.Director.Core.Scores
{
    /// <summary>
    /// Handles multi-channel rectangle selection logic for the score manager.
    /// </summary>
    internal class DirScoreRectangleSelection
    {
        private readonly DirScoreManager _manager;
        private bool _selecting;
        private int _startChannel;
        private int _startFrame;
        private int _prevMinChannel = -1;
        private int _prevMaxChannel = -1;

        public DirScoreRectangleSelection(DirScoreManager manager)
        {
            _manager = manager;
        }

        public void Begin(int channelNumber, int frameNumber)
        {
            var sm = _manager.SpritesManagerNullable;
            if (sm == null || !sm.Key.ControlDown)
                _manager.ClearSelection();

            _selecting = true;
            _startChannel = channelNumber;
            _startFrame = frameNumber;
            _prevMinChannel = _prevMaxChannel = -1;
        }

        public bool Update(int channelNumber, int frameNumber)
        {
            if (!_selecting || !_manager.LastMouseLeftDown)
                return false;

            if (_prevMinChannel != -1)
            {
                for (int ch = _prevMinChannel; ch <= _prevMaxChannel; ch++)
                    if (_manager.Channels.TryGetValue(ch, out var chPrev))
                        chPrev.ClearSelectionRect();
            }

            int chDelta = channelNumber - _startChannel;
            int frDelta = frameNumber - _startFrame;
            if (Math.Abs(chDelta) < 1 && Math.Abs(frDelta) < 1)
            {
                _prevMinChannel = _prevMaxChannel = -1;
                return true;
            }

            int minCh = Math.Min(_startChannel, channelNumber);
            int maxCh = Math.Max(_startChannel, channelNumber);
            int minFr = Math.Min(_startFrame, frameNumber);
            int maxFr = Math.Max(_startFrame, frameNumber);
            for (int ch = minCh; ch <= maxCh; ch++)
                if (_manager.Channels.TryGetValue(ch, out var chObj))
                    chObj.SetSelectionRect(minFr, maxFr);

            _prevMinChannel = minCh;
            _prevMaxChannel = maxCh;
            return true;
        }

        public bool Complete(AbstMouseEvent mouseEvent, int channelNumber, int frameNumber)
        {
            if (!_selecting)
                return false;

            if (_prevMinChannel != -1)
            {
                for (int ch = _prevMinChannel; ch <= _prevMaxChannel; ch++)
                    if (_manager.Channels.TryGetValue(ch, out var chPrev))
                        chPrev.ClearSelectionRect();
            }

            int chDelta = channelNumber - _startChannel;
            int frDelta = frameNumber - _startFrame;
            if (Math.Abs(chDelta) >= 1 || Math.Abs(frDelta) >= 1)
            {
                int minCh = Math.Min(_startChannel, channelNumber);
                int maxCh = Math.Max(_startChannel, channelNumber);
                int minFr = Math.Min(_startFrame, frameNumber);
                int maxFr = Math.Max(_startFrame, frameNumber);
                var toSelect = new List<DirScoreSprite>();
                for (int ch = minCh; ch <= maxCh; ch++)
                {
                    if (_manager.Channels.TryGetValue(ch, out var chObj))
                    {
                        foreach (var sp in chObj.GetSprites())
                        {
                            if (sp.Sprite.BeginFrame >= minFr && sp.Sprite.EndFrame <= maxFr)
                                toSelect.Add(sp);
                        }
                    }
                }
                var sm = _manager.SpritesManagerNullable;
                if (sm != null)
                {
                    if (!sm.Key.ControlDown)
                        _manager.ClearSelection();
                    foreach (var sp in toSelect)
                    {
                        if (_manager.Selected.Contains(sp))
                            continue;
                        _manager.AddSelection(sp, false);
                        sm.SpritesSelection.Add(sp.Sprite);
                        sm.Mediator.RaiseSpriteSelected(sp.Sprite);
                    }
                }
            }

            _selecting = false;
            _prevMinChannel = _prevMaxChannel = -1;
            _manager.LastMouseLeftDown = false;
            mouseEvent.Mouse.SetCursor(AMouseCursor.Arrow);
            return true;
        }
    }
}
