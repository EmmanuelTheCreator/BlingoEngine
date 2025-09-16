// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using LingoEngine.Core;
using LingoEngine.Movies;

namespace LingoEngine.Demo.TetriGrounds.Core.ParentScripts
{
    // Converted from 19_ClassSubscibe.ls
    public class ClassSubscribeScript : LingoParentScript
    {
        private readonly List<object> mySubscribers = new();
        private readonly List<Dictionary<string, Action<object?>>> mySubscribersData = new();

        public ClassSubscribeScript(ILingoMovieEnvironment env) : base(env) { }

        public int Subscribe(object obj, Action<object?> function)
        {
            if (mySubscribers.Contains(obj))
                return -1;
            mySubscribers.Add(obj);
            mySubscribersData.Add(new Dictionary<string, Action<object?>> { ["function"] = function });
            return mySubscribers.Count; // 1-based like Lingo
        }

        public IReadOnlyList<object> SubscribersGetAll() => mySubscribers;

        public object? SubscribersGetById(int val)
        {
            if (val < 1 || val > mySubscribers.Count) return null;
            return mySubscribers[val - 1];
        }

        public void ExecuteAllSubscribed(object? data)
        {
            for (int i = 0; i < mySubscribers.Count; i++)
            {
                object obj = mySubscribers[i];
                Action<object?> function = mySubscribersData[i]["function"];
                function(data);
            }
        }

        public void SubscribersDestroy()
        {
            mySubscribers.Clear();
            mySubscribersData.Clear();
        }
    }
}
