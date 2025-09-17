﻿// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using BlingoEngine.Sprites;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;
using BlingoEngine.Sprites.Events;
using BlingoEngine.Texts;
#pragma warning disable IDE1006 // Naming Styles

namespace BlingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors
{
    internal class TestTetrigroundsBehavior : BlingoSpriteBehavior, IHasExitFrameEvent, IHasBeginSpriteEvent
    {
        private readonly GlobalVars _global;
        private int _counter;
        private int _counterText;
        private IBlingoMemberTextBase? _member;

        public TestTetrigroundsBehavior(IBlingoMovieEnvironment env, GlobalVars global) : base(env)
        {
            _global = global;
        }
        public void BeginSprite()
        {
            _member = Member<IBlingoMemberTextBase>("T_data");
        }

        public void ExitFrame()
        {
            _counter++;
            if (_counter == 50)
            {
                _counter = 0;
                _counterText += 30;
                _member.FontStyle = BlingoTextStyle.Italic;
                _member!.Text = _counterText.ToString();

            }
            _Movie.GoTo(_Movie.CurrentFrame);
        }

     
    }
}

