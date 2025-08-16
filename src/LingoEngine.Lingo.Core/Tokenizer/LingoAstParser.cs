namespace LingoEngine.Lingo.Core.Tokenizer
{
    /// <summary>
    /// Parses a Lingo script source string into an abstract syntax tree (AST).
    /// </summary>
    public class LingoAstParser
    {
        private LingoTokenizer _tokenizer = null!;
        private LingoToken _currentToken;

        public LingoNode Parse(string source)
        {
            _tokenizer = new LingoTokenizer(source);
            AdvanceToken();
            var block = new LingoBlockNode();
            while (_currentToken.Type != LingoTokenType.Eof)
            {
                if (_currentToken.Type == LingoTokenType.On)
                    block.Children.Add(ParseHandler());
                else
                    block.Children.Add(ParseStatement());
            }
            return block;
        }
        private void AdvanceToken() => _currentToken = _tokenizer.NextToken();
        private bool Match(LingoTokenType type)
        {
            if (_currentToken.Type != type) return false;
            AdvanceToken();
            return true;
        }

        private LingoToken Expect(LingoTokenType type)
        {
            if (_currentToken.Type != type)
                throw new Exception($"Expected token {type}, but got {_currentToken.Type} at line {_currentToken.Line}");

            var token = _currentToken;
            AdvanceToken();
            return token;
        }


        private LingoNode ParseHandler()
        {
            Expect(LingoTokenType.On);
            var nameTok = Expect(LingoTokenType.Identifier);
            int declLine = nameTok.Line;
            while (_currentToken.Line == declLine && _currentToken.Type != LingoTokenType.Eof)
                AdvanceToken();
            var body = new LingoBlockNode();
            while (_currentToken.Type != LingoTokenType.End && _currentToken.Type != LingoTokenType.Eof)
                body.Children.Add(ParseStatement());
            if (_currentToken.Type == LingoTokenType.End)
                AdvanceToken();
            return new LingoHandlerNode
            {
                Handler = new LingoHandler { Name = nameTok.Lexeme },
                Block = body
            };
        }

        private LingoNode ParseStatement()
        {
            switch (_currentToken.Type)
            {
                case LingoTokenType.Identifier:
                    return ParseCallOrAssignment();
                case LingoTokenType.Global:
                case LingoTokenType.Property:
                case LingoTokenType.Instance:
                    return ParseDeclarationStatement(_currentToken.Type);
                case LingoTokenType.If:
                case LingoTokenType.Put:
                case LingoTokenType.Exit:
                case LingoTokenType.Next:
                case LingoTokenType.Repeat:
                    return ParseKeywordStatement();
                default:
                    AdvanceToken();
                    return new LingoErrorNode();
            }
        }


        private LingoNode ParseCallOrAssignment()
        {
            var ident = Expect(LingoTokenType.Identifier);

            var nameLower = ident.Lexeme.ToLowerInvariant();

            if (nameLower == "cursor")
            {
                var value = ParseExpression();
                return new LingoCursorStmtNode { Value = value };
            }

            if (nameLower == "go")
            {
                Match(LingoTokenType.To);
                var target = ParseExpression();
                return new LingoGoToStmtNode { Target = target };
            }

            if (string.Equals(ident.Lexeme, "sendSprite", System.StringComparison.OrdinalIgnoreCase))
            {
                var sprite = ParseExpression();
                if (Match(LingoTokenType.Comma))
                {
                    var message = ParseExpression();
                    return new LingoSendSpriteStmtNode { Sprite = sprite, Message = message };
                }
            }

            if (Match(LingoTokenType.Equals))
            {
                var value = ParseExpression();
                return new LingoAssignmentStmtNode
                {
                    Target = new LingoDatumNode(new LingoDatum(ident.Lexeme, isSymbol: true)),
                    Value = value
                };
            }

            return new LingoCallNode { Name = ident.Lexeme };
        }


        private LingoNode ParseKeywordStatement()
        {
            var keywordToken = _currentToken;
            AdvanceToken(); // consume the keyword

            switch (keywordToken.Type)
            {
                case LingoTokenType.Exit:
                    if (_currentToken.Type == LingoTokenType.Repeat)
                    {
                        AdvanceToken(); // consume 'repeat'

                        if (_currentToken.Type == LingoTokenType.If)
                        {
                            AdvanceToken(); // consume 'if'
                            var condition = ParseExpression();
                            return new LingoExitRepeatIfStmtNode(condition);
                        }

                        return new LingoExitRepeatStmtNode();
                    }
                    return new LingoExitStmtNode();

                case LingoTokenType.Next:
                    if (_currentToken.Type == LingoTokenType.Repeat)
                    {
                        AdvanceToken(); // consume 'repeat'

                        if (_currentToken.Type == LingoTokenType.If)
                        {
                            AdvanceToken(); // consume 'if'
                            var condition = ParseExpression();
                            return new LingoNextRepeatIfStmtNode(condition);
                        }

                        return new LingoNextRepeatStmtNode();
                    }

                    return new LingoNextStmtNode(); // fallback, if you want to allow generic "next"

                case LingoTokenType.Put:
                    return ParsePutStatement();

                case LingoTokenType.If:
                    return ParseIfStatement();

                case LingoTokenType.Repeat:
                    return ParseRepeatStatement();

                default:
                    return new LingoDatumNode(new LingoDatum(keywordToken.Lexeme));
            }
        }

        private LingoNode ParseDeclarationStatement(LingoTokenType keyword)
        {
            AdvanceToken(); // consume keyword
            var names = new List<string>();
            do
            {
                var ident = Expect(LingoTokenType.Identifier);
                names.Add(ident.Lexeme);
            } while (Match(LingoTokenType.Comma));

            switch (keyword)
            {
                case LingoTokenType.Global:
                    var g = new LingoGlobalDeclStmtNode();
                    g.Names.AddRange(names);
                    return g;
                case LingoTokenType.Property:
                    var p = new LingoPropertyDeclStmtNode();
                    p.Names.AddRange(names);
                    return p;
                case LingoTokenType.Instance:
                    var i = new LingoInstanceDeclStmtNode();
                    i.Names.AddRange(names);
                    return i;
                default:
                    return new LingoErrorNode();
            }
        }

        private LingoNode ParseExpression()
        {
            switch (_currentToken.Type)
            {
                case LingoTokenType.Number:
                    var text = _currentToken.Lexeme;
                    LingoDatum datum;
                    if (text.StartsWith("$"))
                    {
                        if (int.TryParse(text[1..], System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out var hex))
                            datum = new LingoDatum(hex);
                        else
                            datum = new LingoDatum(text);
                    }
                    else if (text.Contains('.'))
                    {
                        if (float.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var f))
                            datum = new LingoDatum(f);
                        else
                            datum = new LingoDatum(text);
                    }
                    else if (int.TryParse(text, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var i))
                    {
                        datum = new LingoDatum(i);
                    }
                    else
                    {
                        datum = new LingoDatum(text);
                    }
                    AdvanceToken();
                    return new LingoDatumNode(datum);

                case LingoTokenType.Minus:
                    AdvanceToken();
                    if (_currentToken.Type == LingoTokenType.Number)
                    {
                        var negText = "-" + _currentToken.Lexeme;
                        LingoDatum negDatum;
                        if (negText.Contains('.'))
                        {
                            if (float.TryParse(negText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var nf))
                                negDatum = new LingoDatum(nf);
                            else
                                negDatum = new LingoDatum(negText);
                        }
                        else if (int.TryParse(negText, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var ni))
                        {
                            negDatum = new LingoDatum(ni);
                        }
                        else
                        {
                            negDatum = new LingoDatum(negText);
                        }
                        AdvanceToken();
                        return new LingoDatumNode(negDatum);
                    }
                    return new LingoDatumNode(new LingoDatum("-"));

                case LingoTokenType.String:
                    var strLiteral = new LingoDatumNode(new LingoDatum(_currentToken.Lexeme));
                    AdvanceToken();
                    return strLiteral;

                case LingoTokenType.Identifier:
                    var ident = new LingoDatumNode(new LingoDatum(_currentToken.Lexeme, isSymbol: true));
                    AdvanceToken();
                    return ident;

                default:
                    // fallback: unknown expressions are treated as raw datum
                    var fallback = new LingoDatumNode(new LingoDatum(_currentToken.Lexeme));
                    AdvanceToken();
                    return fallback;
            }
        }

        private LingoNode ParsePutStatement()
        {
            // At this point, the "put" keyword has already been consumed

            // Parse the value expression (what to put)
            var value = ParseExpression();

            // Expect and consume the "into" keyword
            Expect(LingoTokenType.Into);

            // Parse the destination expression (where to put)
            var target = ParseExpression();

            return new LingoPutStmtNode(value, target);
        }


        private LingoNode ParseIfStatement()
        {
            // "if" already consumed

            var condition = ParseExpression();
            Expect(LingoTokenType.Then);

            var thenBlock = ParseBlock();

            LingoNode? elseBlock = null;
            if (_currentToken.Type == LingoTokenType.Else)
            {
                AdvanceToken(); // consume 'else'
                elseBlock = ParseBlock();
            }

            Expect(LingoTokenType.End);
            Expect(LingoTokenType.If);

            return new LingoIfStmtNode(condition, thenBlock, elseBlock);
        }

        private LingoBlockNode ParseBlock()
        {
            var block = new LingoBlockNode();

            while (_currentToken.Type != LingoTokenType.End &&
                   _currentToken.Type != LingoTokenType.Else &&
                   _currentToken.Type != LingoTokenType.Eof)
            {
                block.Children.Add(ParseStatement());
            }

            return block;
        }

        private LingoNode ParseRepeatStatement()
        {
            // "repeat" already consumed

            if (_currentToken.Type == LingoTokenType.While)
            {
                AdvanceToken();
                var condition = ParseExpression();
                var body = ParseBlock();
                Expect(LingoTokenType.End);
                Expect(LingoTokenType.Repeat);
                return new LingoRepeatWhileStmtNode(condition, body);
            }
            else if (_currentToken.Type == LingoTokenType.Until)
            {
                AdvanceToken();
                var condition = ParseExpression();
                var body = ParseBlock();
                Expect(LingoTokenType.End);
                Expect(LingoTokenType.Repeat);
                return new LingoRepeatUntilStmtNode(condition, body);
            }
            else if (_currentToken.Type == LingoTokenType.With)
            {
                AdvanceToken();
                var varToken = Expect(LingoTokenType.Identifier);
                Expect(LingoTokenType.Equals);
                var start = ParseExpression();
                Expect(LingoTokenType.To);
                var end = ParseExpression();
                var body = ParseBlock();
                Expect(LingoTokenType.End);
                Expect(LingoTokenType.Repeat);
                return new LingoRepeatWithStmtNode(varToken.Lexeme, start, end, body);
            }
            else if (_currentToken.Type == LingoTokenType.Number)
            {
                // repeat <number> times
                var countExpr = ParseExpression();
                Expect(LingoTokenType.Times);
                var body = ParseBlock();
                Expect(LingoTokenType.End);
                Expect(LingoTokenType.Repeat);
                return new LingoRepeatTimesStmtNode(countExpr, body);
            }
            else
            {
                // repeat, repeat forever, or loop
                var body = ParseBlock();
                Expect(LingoTokenType.End);
                Expect(LingoTokenType.Repeat);
                return new LingoRepeatForeverStmtNode(body);
            }
        }

    }



}



