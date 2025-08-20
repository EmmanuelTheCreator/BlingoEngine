﻿using LingoEngine.Inputs;
using LingoEngine.Inputs.Events;
using LingoEngine.Movies.Events;
using LingoEngine.Sprites;
using AbstUI.Inputs;

namespace LingoEngine.Events
{

    public class LingoEventMediator : ILingoEventMediator, ILingoMouseEventHandler, ILingoKeyEventHandler, ILingoSpriteEventHandler
    {
        private const int DefaultPriority = 5000;

        private readonly Dictionary<object, int> _priorities = new();

        private readonly List<IHasPrepareMovieEvent> _prepareMovies = new();
        private readonly List<IHasStartMovieEvent> _startMovies = new();
        private readonly List<IHasStopMovieEvent> _stopMovies = new();
        private readonly List<IHasMouseDownEvent> _mouseDowns = new();
        private readonly List<IHasMouseUpEvent> _mouseUps = new();
        private readonly List<IHasMouseMoveEvent> _mouseMoves = new();
        private readonly List<IHasMouseWheelEvent> _mouseWheels = new();
        private readonly List<IHasMouseEnterEvent> _mouseEnters = new();
        private readonly List<IHasMouseExitEvent> _mouseExits = new();
        //private readonly List<IHasBeginSpriteEvent> _beginSprites = new();
        //private readonly List<IHasEndSpriteEvent> _endSprites = new();
        private readonly List<IHasStepFrameEvent> _stepFrames = new(); // must be handled by actorlist
        private readonly List<IHasPrepareFrameEvent> _prepareFrames = new();
        private readonly List<IHasEnterFrameEvent> _enterFrames = new();
        private readonly List<IHasExitFrameEvent> _exitFrames = new();
        private readonly List<IHasFocusEvent> _focuss = new();
        private readonly List<IHasBlurEvent> _blurs = new();
        private readonly List<IHasKeyUpEvent> _keyUps = new();
        private readonly List<IHasKeyDownEvent> _keyDowns = new();


        private void Insert<T>(List<T> list, T item) where T : class
        {
            var priority = _priorities[item!];
            var index = list.FindIndex(e => _priorities[(object)e] > priority);
            if (index < 0)
                list.Add(item);
            else
                list.Insert(index, item);
        }
        // Must be separated and handled by actorlist
        public void SubscribeStepFrame(IHasStepFrameEvent ms, int priority = DefaultPriority)
        {
            _priorities[ms] = priority;
            Insert(_stepFrames, ms);
        }
        public void UnsubscribeStepFrame(IHasStepFrameEvent ms, int priority = DefaultPriority)
        {
            _priorities[ms] = priority;
            _stepFrames.Remove(ms);
        }
        public void Subscribe(object ms, int priority = DefaultPriority, bool ignoreMouse = false)
        {
            _priorities[ms] = priority;

            if (ms is IHasPrepareMovieEvent prepareMovieEvent) Insert(_prepareMovies, prepareMovieEvent);
            if (ms is IHasStartMovieEvent startMovieEvent) Insert(_startMovies, startMovieEvent);
            if (ms is IHasStopMovieEvent stopMovieEvent) Insert(_stopMovies, stopMovieEvent);
            if (!ignoreMouse)
            {
                // for behaviors, events lay only fire when the mouse is in the boundingbox of the sprite
                if (ms is IHasMouseDownEvent mouseDownEvent) Insert(_mouseDowns, mouseDownEvent);
                if (ms is IHasMouseUpEvent mouseUpEvent) Insert(_mouseUps, mouseUpEvent);
                if (ms is IHasMouseMoveEvent mouseMoveEvent) Insert(_mouseMoves, mouseMoveEvent);
                if (ms is IHasMouseWheelEvent mouseWheelEvent) Insert(_mouseWheels, mouseWheelEvent);
                if (ms is IHasMouseEnterEvent mouseEnterEvent) Insert(_mouseEnters, mouseEnterEvent);
                if (ms is IHasMouseExitEvent mouseExitEvent) Insert(_mouseExits, mouseExitEvent);
            }
            //if (ms is IHasBeginSpriteEvent beginSpriteEvent) Insert(_beginSprites, beginSpriteEvent);
            //if (ms is IHasEndSpriteEvent endSpriteEvent) Insert(_endSprites, endSpriteEvent);
            // NOT stepframe, it seems stepframe is only used through the actor list.

            if (ms is IHasPrepareFrameEvent prepareFrameEvent) Insert(_prepareFrames, prepareFrameEvent);
            if (ms is IHasEnterFrameEvent enterFrameEvent) Insert(_enterFrames, enterFrameEvent);
            if (ms is IHasExitFrameEvent exitFrameEvent) Insert(_exitFrames, exitFrameEvent);
            if (ms is IHasFocusEvent focusEvent) Insert(_focuss, focusEvent);
            if (ms is IHasBlurEvent blurEvent) Insert(_blurs, blurEvent);
            if (ms is IHasKeyUpEvent keyUpEvent) Insert(_keyUps, keyUpEvent);
            if (ms is IHasKeyDownEvent keyDownEvent) Insert(_keyDowns, keyDownEvent);
        }
        public void Unsubscribe(object ms, bool ignoreMouse = false)
        {
            if (ms is IHasPrepareMovieEvent prepareMovieEvent) _prepareMovies.Remove(prepareMovieEvent);
            if (ms is IHasStartMovieEvent startMovieEvent) _startMovies.Remove(startMovieEvent);
            if (ms is IHasStopMovieEvent stopMovieEvent) _stopMovies.Remove(stopMovieEvent);
            if (!ignoreMouse)
            {
                if (ms is IHasMouseDownEvent mouseDownEvent) _mouseDowns.Remove(mouseDownEvent);
                if (ms is IHasMouseUpEvent mouseUpEvent) _mouseUps.Remove(mouseUpEvent);
                if (ms is IHasMouseMoveEvent mouseMoveEvent) _mouseMoves.Remove(mouseMoveEvent);
                if (ms is IHasMouseWheelEvent mouseWheelEvent) _mouseWheels.Remove(mouseWheelEvent);
                if (ms is IHasMouseEnterEvent mouseEnterEvent) _mouseEnters.Remove(mouseEnterEvent);
                if (ms is IHasMouseExitEvent mouseExitEvent) _mouseExits.Remove(mouseExitEvent);
            }
            //if (ms is IHasBeginSpriteEvent beginSpriteEvent) _beginSprites.Remove(beginSpriteEvent);
            //if (ms is IHasEndSpriteEvent endSpriteEvent) _endSprites.Remove(endSpriteEvent);
            
            // Not stepframe it seems stepframe is only used through the actor list.
            if (ms is IHasPrepareFrameEvent prepareFrameEvent) _prepareFrames.Remove(prepareFrameEvent);
            if (ms is IHasEnterFrameEvent enterFrameEvent) _enterFrames.Remove(enterFrameEvent);
            if (ms is IHasExitFrameEvent exitFrameEvent) _exitFrames.Remove(exitFrameEvent);
            if (ms is IHasFocusEvent focusEvent) _focuss.Remove(focusEvent);
            if (ms is IHasBlurEvent blurEvent) _blurs.Remove(blurEvent);
            if (ms is IHasKeyUpEvent keyUpEvent) _keyUps.Remove(keyUpEvent);
            if (ms is IHasKeyDownEvent keyDownEvent) _keyDowns.Remove(keyDownEvent);

            _priorities.Remove(ms);
        }

        public void Clear(string? preserveNamespaceFragment = null)
        {
            bool ShouldRemove(object obj) => string.IsNullOrEmpty(preserveNamespaceFragment) ||
                obj.GetType().Namespace?.IndexOf(preserveNamespaceFragment, StringComparison.OrdinalIgnoreCase) < 0;

            foreach (var key in _priorities.Keys.ToList())
                if (ShouldRemove(key))
                    _priorities.Remove(key);

            void FilterList<T>(List<T> list) where T : class => list.RemoveAll(x => ShouldRemove(x!));

            FilterList(_prepareMovies);
            FilterList(_startMovies);
            FilterList(_stopMovies);
            FilterList(_mouseDowns);
            FilterList(_mouseUps);
            FilterList(_mouseMoves);
            FilterList(_mouseWheels);
            FilterList(_mouseEnters);
            FilterList(_mouseExits);
            FilterList(_prepareFrames);
            FilterList(_enterFrames);
            FilterList(_exitFrames);
            FilterList(_focuss);
            FilterList(_blurs);
            FilterList(_keyUps);
            FilterList(_keyDowns);
        }
        internal void RaisePrepareMovie() => _prepareMovies.ForEach(x => x.PrepareMovie());
        internal void RaiseStartMovie() => _startMovies.ForEach(x => x.StartMovie());
        internal void RaiseStopMovie() => _stopMovies.ForEach(x => x.StopMovie());
        public void RaiseMouseDown(LingoMouseEvent mouse) => _mouseDowns.ForEach(x => x.MouseDown(mouse));
        public void RaiseMouseUp(LingoMouseEvent mouse) => _mouseUps.ForEach(x => x.MouseUp(mouse));
        public void RaiseMouseMove(LingoMouseEvent mouse) => _mouseMoves.ForEach(x => x.MouseMove(mouse));
        public void RaiseMouseWheel(LingoMouseEvent mouse) => _mouseWheels.ForEach(x => x.MouseWheel(mouse));
        internal void RaiseMouseEnter(LingoMouseEvent mouse) => _mouseEnters.ForEach(x => x.MouseEnter(mouse));
        internal void RaiseMouseExit(LingoMouseEvent mouse) => _mouseExits.ForEach(x => x.MouseExit(mouse));
        //internal void RaiseBeginSprite() => _beginSprites.ForEach(x => x.BeginSprite());
        //internal void RaiseEndSprite() => _endSprites.ForEach(x => x.EndSprite());
        internal void RaiseStepFrame() => _stepFrames.ForEach(x => x.StepFrame());
        internal void RaisePrepareFrame() => _prepareFrames.ForEach(x => x.PrepareFrame());
        internal void RaiseEnterFrame() => _enterFrames.ForEach(x => x.EnterFrame());
        internal void RaiseExitFrame() => _exitFrames.ForEach(x => x.ExitFrame());
        public void RaiseFocus() => _focuss.ForEach(x => x.Focus());
        public void RaiseBlur() => _blurs.ForEach(x => x.Blur());
        public void RaiseKeyUp(LingoKeyEvent key) => _keyUps.ForEach(x => x.KeyUp(key));
        public void RaiseKeyDown(LingoKeyEvent key) => _keyDowns.ForEach(x => x.KeyDown(key));

        void IAbstKeyEventHandler<LingoKeyEvent>.RaiseKeyDown(LingoKeyEvent key) => RaiseKeyDown(key);
        void IAbstKeyEventHandler<LingoKeyEvent>.RaiseKeyUp(LingoKeyEvent key) => RaiseKeyUp(key);


    }
}
