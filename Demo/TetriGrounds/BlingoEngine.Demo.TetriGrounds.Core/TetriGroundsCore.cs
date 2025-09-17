// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

namespace BlingoEngine.Demo.TetriGrounds.Core
{
    public interface IArkCore
    {
        void Resetgame();
    }
    internal class TetriGroundsCore : IArkCore
    {
        private readonly GlobalVars _global;

        public TetriGroundsCore(GlobalVars global)
        {
            _global = global;
        }

        public void Resetgame()
        {
        }
    }
}

