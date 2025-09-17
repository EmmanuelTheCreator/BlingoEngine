﻿using System.Collections.Generic;

namespace BlingoEngine.Lingo.Core.Tokenizer
{
    /// <summary>
    /// Represents a Lingo handler (method or event).
    /// </summary>
    public class BlingoHandler
    {
        public BlingoScript Script { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public List<string> ArgumentNames { get; set; } = new();
        public List<string> GlobalNames { get; set; } = new();
        public bool IsGenericEvent { get; set; }
        public bool IsMethod { get; internal set; }

        /// <summary>
        /// The abstract syntax tree (AST) of this handler, if parsed.
        /// </summary>
        public HandlerAst? Ast { get; set; }

        public void Parse()
        {
            var parser = new BlingoAstParser();
            Ast = new HandlerAst(parser.Parse(Script.Source));
        }

    }

    /// <summary>
    /// Wraps the root AST node for a handler.
    /// </summary>
    public class HandlerAst
    {
        public BlingoNode? Root { get; set; }

        public HandlerAst(BlingoNode? root)
        {
            Root = root;
        }
    }

}



