using System;

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
            if (_currentToken.Type == LingoTokenType.Identifier &&
                _currentToken.Lexeme.Equals("cursor", StringComparison.OrdinalIgnoreCase))
            {
                AdvanceToken();
                var value = ParseExpression();
                return new LingoCursorStmtNode { Value = value };
            }

            if (_currentToken.Type == LingoTokenType.Identifier &&
                _currentToken.Lexeme.Equals("go", StringComparison.OrdinalIgnoreCase))
            {
                AdvanceToken();
                Match(LingoTokenType.To);
                var target = ParseExpression();
                return new LingoGoToStmtNode { Target = target };
            }

            if (_currentToken.Type == LingoTokenType.Identifier &&
                _currentToken.Lexeme.Equals("sendSprite", StringComparison.OrdinalIgnoreCase))
            {
                AdvanceToken();
                var sprite = ParseExpression();
                if (Match(LingoTokenType.Comma))
                {
                    var message = ParseExpression();
                    return new LingoSendSpriteStmtNode { Sprite = sprite, Message = message };
                }
            }
            var expr = ParseExpression(false);
            if (_currentToken.Type == LingoTokenType.The && expr is LingoVarNode methodVar)
            {
                AdvanceToken();
                var propTok = Expect(LingoTokenType.Identifier);
                LingoNode objExpr = new LingoTheExprNode { Prop = propTok.Lexeme };
                while (Match(LingoTokenType.Dot))
                {
                    var pTok = Expect(LingoTokenType.Identifier);
                    objExpr = new LingoObjPropExprNode
                    {
                        Object = objExpr,
                        Property = new LingoVarNode { VarName = pTok.Lexeme }
                    };
                }
                Expect(LingoTokenType.LeftParen);
                var argExpr = ParseExpression();
                Expect(LingoTokenType.RightParen);
                return new LingoCallNode
                {
                    Callee = new LingoObjPropExprNode
                    {
                        Object = objExpr,
                        Property = new LingoVarNode { VarName = methodVar.VarName }
                    },
                    Arguments = argExpr
                };
            }
            if (Match(LingoTokenType.Equals))
            {
                var value = ParseExpression();
                return new LingoAssignmentStmtNode { Target = expr, Value = value };
            }

            if (expr is LingoVarNode v)
                return new LingoCallNode { Name = v.VarName };

            return expr;
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

        private LingoNode ParseExpression(bool allowEquals = true)
        {
            var expr = ParsePrimary();
            while (true)
            {
                if (_currentToken.Type == LingoTokenType.Plus)
                {
                    AdvanceToken();
                    var right = ParsePrimary();
                    expr = new LingoBinaryOpNode
                    {
                        Left = expr,
                        Right = right,
                        Opcode = LingoBinaryOpcode.Add
                    };
                }
                else if (_currentToken.Type == LingoTokenType.NotEquals)
                {
                    AdvanceToken();
                    var right = ParsePrimary();
                    expr = new LingoBinaryOpNode
                    {
                        Left = expr,
                        Right = right,
                        Opcode = LingoBinaryOpcode.NotEquals
                    };
                }
                else if (allowEquals && _currentToken.Type == LingoTokenType.Equals)
                {
                    AdvanceToken();
                    var right = ParsePrimary();
                    expr = new LingoBinaryOpNode
                    {
                        Left = expr,
                        Right = right,
                        Opcode = LingoBinaryOpcode.Equals
                    };
                }
                else
                {
                    break;
                }
            }

            return expr;
        }

        private LingoNode ParsePrimary()
        {
            LingoNode expr;
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
                    expr = new LingoDatumNode(datum);
                    break;

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
                        expr = new LingoDatumNode(negDatum);
                    }
                    else
                    {
                        expr = new LingoDatumNode(new LingoDatum("-"));
                    }
                    break;

                case LingoTokenType.String:
                    expr = new LingoDatumNode(new LingoDatum(_currentToken.Lexeme));
                    AdvanceToken();
                    break;

                case LingoTokenType.Symbol:
                    expr = new LingoDatumNode(new LingoDatum(_currentToken.Lexeme, isSymbol: true));
                    AdvanceToken();
                    break;

                case LingoTokenType.Me:
                    expr = new LingoVarNode { VarName = "me" };
                    AdvanceToken();
                    break;

                case LingoTokenType.The:
                    AdvanceToken();
                    var propTok = Expect(LingoTokenType.Identifier);
                    expr = new LingoTheExprNode { Prop = propTok.Lexeme };
                    while (Match(LingoTokenType.Dot))
                    {
                        var pTok = Expect(LingoTokenType.Identifier);
                        expr = new LingoObjPropExprNode
                        {
                            Object = expr,
                            Property = new LingoVarNode { VarName = pTok.Lexeme }
                        };
                    }
                    if (Match(LingoTokenType.LeftParen))
                    {
                        var argExpr = ParseExpression();
                        Expect(LingoTokenType.RightParen);
                        expr = new LingoCallNode
                        {
                            Callee = expr,
                            Arguments = argExpr
                        };
                    }
                    break;

                case LingoTokenType.Identifier:
                    var name = _currentToken.Lexeme;
                    AdvanceToken();
                    if (name.Equals("sprite", StringComparison.OrdinalIgnoreCase) && Match(LingoTokenType.LeftParen))
                    {
                        var indexExpr = ParseSpriteIndex();
                        Expect(LingoTokenType.RightParen);
                        if (Match(LingoTokenType.Dot))
                        {
                            var pTok = Expect(LingoTokenType.Identifier);
                            expr = new LingoSpritePropExprNode
                            {
                                Sprite = indexExpr,
                                Property = new LingoVarNode { VarName = pTok.Lexeme }
                            };
                        }
                        else
                        {
                            expr = new LingoCallNode
                            {
                                Callee = new LingoVarNode { VarName = name },
                                Arguments = indexExpr
                            };
                        }
                    }
                    else if (name.Equals("member", StringComparison.OrdinalIgnoreCase) && Match(LingoTokenType.LeftParen))
                    {
                        var inner = ParseExpression();
                        Expect(LingoTokenType.RightParen);
                        expr = new LingoMemberExprNode { Expr = inner };
                        while (Match(LingoTokenType.Dot))
                        {
                            var pTok = Expect(LingoTokenType.Identifier);
                            expr = new LingoObjPropExprNode
                            {
                                Object = expr,
                                Property = new LingoVarNode { VarName = pTok.Lexeme }
                            };
                        }
                    }
                    else
                    {
                        expr = new LingoVarNode { VarName = name };
                        while (Match(LingoTokenType.Dot))
                        {
                            var pTok = Expect(LingoTokenType.Identifier);
                            expr = new LingoObjPropExprNode
                            {
                                Object = expr,
                                Property = new LingoVarNode { VarName = pTok.Lexeme }
                            };
                        }
                    }
                    break;

                default:
                    AdvanceToken();
                    return new LingoErrorNode();
            }

            return expr;
        }

        private LingoNode ParseSpriteIndex()
        {
            LingoNode expr;
            if (_currentToken.Type == LingoTokenType.Me)
            {
                expr = new LingoVarNode { VarName = "me" };
                AdvanceToken();
            }
            else if (_currentToken.Type == LingoTokenType.Identifier)
            {
                expr = new LingoVarNode { VarName = _currentToken.Lexeme };
                AdvanceToken();
            }
            else
            {
                expr = new LingoErrorNode();
            }

            while (Match(LingoTokenType.Dot))
            {
                var pTok = Expect(LingoTokenType.Identifier);
                expr = new LingoObjPropExprNode
                {
                    Object = expr,
                    Property = new LingoVarNode { VarName = pTok.Lexeme }
                };
            }

            return expr;
        }

        private LingoNode ParsePutStatement()
        {
            // At this point, the "put" keyword has already been consumed

            // Parse the value expression (what to put)
            var value = ParseExpression();

            // Expect and consume the "into" keyword
            Expect(LingoTokenType.Into);

            // Parse the destination expression (where to put)
            var target = ParseExpression(false);

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



