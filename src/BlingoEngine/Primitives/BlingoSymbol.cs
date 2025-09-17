using System;
using System.Collections.Concurrent;

namespace BlingoEngine.Primitives
{


    public readonly struct BlingoSymbol : IEquatable<BlingoSymbol>
    {
        // Base types
        public static BlingoSymbol String = new BlingoSymbol("string");
        public static BlingoSymbol Int = new BlingoSymbol("int");
        public static BlingoSymbol Float = new BlingoSymbol("float");
        public static BlingoSymbol Boolean = new BlingoSymbol("Boolean");
        // member types
        public static BlingoSymbol Text = new BlingoSymbol("text");
        public static BlingoSymbol Video = new BlingoSymbol("video");
        public static BlingoSymbol Audio = new BlingoSymbol("audio");
        public static BlingoSymbol Bitmap = new BlingoSymbol("bitmap");
        public static BlingoSymbol Color = new BlingoSymbol("color");


        private static readonly ConcurrentDictionary<string, BlingoSymbol> _symbolTable = new();

        public string Name { get; }
        private static BlingoSymbol _empty = new BlingoSymbol("");
        public static BlingoSymbol Empty => _empty;
        public bool IsEmpty => Name =="";

        static BlingoSymbol()
        {
            _symbolTable.AddOrUpdate("", _empty, (key, oldValue) => _empty);
        }

        private BlingoSymbol(string name)
        {
            Name = name;
        }

        public static BlingoSymbol New(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Symbol name cannot be null or whitespace.", nameof(name));

            return _symbolTable.GetOrAdd(name, n => new BlingoSymbol(n));
        }

        public static implicit operator BlingoSymbol(string name) => New(name);
        public static implicit operator string(BlingoSymbol symbol) => symbol.Name;

        public override string ToString() => $"#{Name}";
        public override bool Equals(object? obj) => obj is BlingoSymbol s && Equals(s);
        public bool Equals(BlingoSymbol other) => string.Equals(Name, other.Name, StringComparison.Ordinal);
        public override int GetHashCode() => Name.GetHashCode();

        public static bool operator ==(BlingoSymbol left, BlingoSymbol right) => left.Equals(right);
        public static bool operator !=(BlingoSymbol left, BlingoSymbol right) => !left.Equals(right);
    }

}

