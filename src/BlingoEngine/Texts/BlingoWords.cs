using System;
using System.Collections;
using System.Data;
using System.Linq;

namespace BlingoEngine.Texts
{
    /// <summary>
    /// Represents a parsed 1-based word accessor with optional character access for each word.
    /// </summary>
    public class BlingoWords : IEnumerable<BlingoWords> , IEnumerable
    {
        private bool _hasParsed = false;
        private string _text = "";
        private BlingoWords[] _words = [];

        /// <summary>
        /// Gets the word at the specified 1-based index.
        /// </summary>
        public BlingoWords this[int index]
        {
            get
            {
                if (!_hasParsed) Parse();
                return _words[index - 1];
            }
        }

        /// <summary>
        /// Gets the character at the specified word and character index (1-based).
        /// </summary>
        public BlingoChars this[int wordIndex, int charIndex]
        {
            get
            {
                var word = this[wordIndex];
                var ch = new BlingoChars();
                ch.SetText(word);
                return ch;
            }
        }

        /// <summary>
        /// Gets the number of parsed words.
        /// </summary>
        public int Count => _words.Length;

        public BlingoWords(string text) => SetText(text);
        public override string ToString() => _text;

        internal void SetText(string text)
        {
            _text = text.Replace("\r"," ").Replace("\n", " ");
            _hasParsed = false;
        }

        private void Parse()
        {
            _words = _text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(x => new BlingoWords(x)).ToArray();
            _hasParsed = true;
        }

        /// <summary>
        /// Gets the character accessor for a given word index.
        /// </summary>
        public BlingoChars GetChar(int wordIndex)
        {
            var word = this[wordIndex];
            var charObj = new BlingoChars();
            charObj.SetText(word);
            return charObj;
        }

        public IEnumerator<BlingoWords> GetEnumerator() => _words.AsEnumerable().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static implicit operator string(BlingoWords w) => w.ToString();
        public static implicit operator BlingoWords(string value) => new BlingoWords(value);

        public override bool Equals(object? obj) => obj is BlingoWords lw && ToString() == lw.ToString();
        public override int GetHashCode() => ToString().GetHashCode();
    }

}




