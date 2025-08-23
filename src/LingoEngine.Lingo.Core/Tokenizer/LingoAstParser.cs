using System;
using System.Collections.Generic;

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
                else if (_currentToken.Type == LingoTokenType.Comment)
                {
                    block.Children.Add(new LingoCommentNode { Text = _currentToken.Lexeme });
                    AdvanceToken();
                }
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
            var args = new List<string>();
            while (_currentToken.Line == declLine && _currentToken.Type != LingoTokenType.Eof)
            {
                if (_currentToken.Type == LingoTokenType.Identifier || _currentToken.Type == LingoTokenType.Me)
                    args.Add(_currentToken.Lexeme);
                AdvanceToken();
            }
            var body = new LingoBlockNode();
            while (_currentToken.Type != LingoTokenType.End && _currentToken.Type != LingoTokenType.Eof)
                body.Children.Add(ParseStatement());
            if (_currentToken.Type == LingoTokenType.End)
                AdvanceToken();
            return new LingoHandlerNode
            {
                Handler = new LingoHandler { Name = nameTok.Lexeme, ArgumentNames = args },
                Block = body
            };
        }

        private LingoNode ParseStatement()
        {
            switch (_currentToken.Type)
            {
                case LingoTokenType.Comment:
                    var text = _currentToken.Lexeme;
                    AdvanceToken();
                    return new LingoCommentNode { Text = text };
                case LingoTokenType.Identifier:
                case LingoTokenType.Me:
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
                case LingoTokenType.Return:
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
                var hasParen = Match(LingoTokenType.LeftParen);
                var sprite = ParseExpression();
                Expect(LingoTokenType.Comma);
                var message = ParseExpression();
                var args = new List<LingoNode>();
                while (Match(LingoTokenType.Comma))
                {
                    args.Add(ParseExpression());
                }
                if (hasParen)
                    Expect(LingoTokenType.RightParen);
                LingoNode? argList = null;
                if (args.Count > 0)
                    argList = new LingoDatumNode(new LingoDatum(args, LingoDatum.DatumType.ArgList));
                return new LingoSendSpriteStmtNode { Sprite = sprite, Message = message, Arguments = argList };
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
            {
                if (_currentToken.Type != LingoTokenType.End &&
                    _currentToken.Type != LingoTokenType.Else &&
                    _currentToken.Type != LingoTokenType.Eof &&
                    _currentToken.Type != LingoTokenType.Return)
                {
                    var args = new List<LingoNode>
                    {
                        ParseExpression()
                    };
                    while (Match(LingoTokenType.Comma))
                    {
                        args.Add(ParseExpression());
                    }
                    var argList = new LingoDatumNode(new LingoDatum(args, LingoDatum.DatumType.ArgList));
                    return new LingoCallNode { Callee = v, Arguments = argList };
                }

                return new LingoCallNode { Name = v.VarName };
            }

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

                case LingoTokenType.Return:
                    LingoNode? retValue = null;
                    if (_currentToken.Type != LingoTokenType.End &&
                        _currentToken.Type != LingoTokenType.Eof &&
                        _currentToken.Line == keywordToken.Line)
                    {
                        retValue = ParseExpression();
                    }
                    return new LingoReturnStmtNode(retValue);

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
            LingoNode ParseOperand()
            {
                var operand = ParsePrimary();
                while (true)
                {
                    if (operand is LingoVarNode sv && sv.VarName.Equals("sendSprite", StringComparison.OrdinalIgnoreCase))
                    {
                        var hasParen = Match(LingoTokenType.LeftParen);
                        var sprite = ParseExpression();
                        Expect(LingoTokenType.Comma);
                        var message = ParseExpression();
                        var args = new List<LingoNode>();
                        while (Match(LingoTokenType.Comma))
                        {
                            args.Add(ParseExpression());
                        }
                        if (hasParen)
                            Expect(LingoTokenType.RightParen);
                        LingoNode? argList = null;
                        if (args.Count > 0)
                            argList = new LingoDatumNode(new LingoDatum(args, LingoDatum.DatumType.ArgList));
                        operand = new LingoSendSpriteExprNode { Sprite = sprite, Message = message, Arguments = argList };
                    }
                    else if (Match(LingoTokenType.Dot))
                    {
                        var pTok = Expect(LingoTokenType.Identifier);
                        operand = new LingoObjPropExprNode
                        {
                            Object = operand,
                            Property = new LingoVarNode { VarName = pTok.Lexeme }
                        };
                    }
                    else if (Match(LingoTokenType.LeftBracket))
                    {
                        var start = ParseExpression();
                        LingoNode idx = start;
                        if (Match(LingoTokenType.Range))
                        {
                            var end = ParseExpression();
                            idx = new LingoRangeExprNode { Start = start, End = end };
                        }
                        Expect(LingoTokenType.RightBracket);
                        operand = new LingoObjBracketExprNode
                        {
                            Object = operand,
                            Index = idx
                        };
                    }
                    else if (Match(LingoTokenType.LeftParen))
                    {
                        var args = new List<LingoNode>();
                        if (_currentToken.Type != LingoTokenType.RightParen)
                        {
                            args.Add(ParseExpression());
                            while (Match(LingoTokenType.Comma))
                            {
                                args.Add(ParseExpression());
                            }
                        }
                        Expect(LingoTokenType.RightParen);
                        LingoNode argExpr;
                        if (args.Count == 0)
                            argExpr = new LingoBlockNode();
                        else if (args.Count == 1)
                            argExpr = args[0];
                        else
                            argExpr = new LingoDatumNode(new LingoDatum(args, LingoDatum.DatumType.ArgList));

                        operand = new LingoCallNode
                        {
                            Callee = operand,
                            Arguments = argExpr
                        };
                    }
                    else
                    {
                        break;
                    }
                }

                return operand;
            }

            var expr = ParseOperand();

            while (true)
            {
                if (_currentToken.Type == LingoTokenType.Plus)
                {
                    AdvanceToken();
                    var right = ParseOperand();
                    expr = new LingoBinaryOpNode
                    {
                        Left = expr,
                        Right = right,
                        Opcode = LingoBinaryOpcode.Add
                    };
                }
                else if (_currentToken.Type == LingoTokenType.Minus)
                {
                    AdvanceToken();
                    var right = ParseOperand();
                    expr = new LingoBinaryOpNode
                    {
                        Left = expr,
                        Right = right,
                        Opcode = LingoBinaryOpcode.Subtract
                    };
                }
                else if (_currentToken.Type == LingoTokenType.Asterisk)
                {
                    AdvanceToken();
                    var right = ParseOperand();
                    expr = new LingoBinaryOpNode
                    {
                        Left = expr,
                        Right = right,
                        Opcode = LingoBinaryOpcode.Multiply
                    };
                }
                else if (_currentToken.Type == LingoTokenType.Slash)
                {
                    AdvanceToken();
                    var right = ParseOperand();
                    expr = new LingoBinaryOpNode
                    {
                        Left = expr,
                        Right = right,
                        Opcode = LingoBinaryOpcode.Divide
                    };
                }
                else if (_currentToken.Type == LingoTokenType.Ampersand)
                {
                    AdvanceToken();
                    var right = ParseOperand();
                    expr = new LingoBinaryOpNode
                    {
                        Left = expr,
                        Right = right,
                        Opcode = LingoBinaryOpcode.Concat
                    };
                }
                else if (_currentToken.Type == LingoTokenType.NotEquals)
                {
                    AdvanceToken();
                    var right = ParseOperand();
                    expr = new LingoBinaryOpNode
                    {
                        Left = expr,
                        Right = right,
                        Opcode = LingoBinaryOpcode.NotEquals
                    };
                }
                else if (_currentToken.Type == LingoTokenType.GreaterThan)
                {
                    AdvanceToken();
                    var right = ParseOperand();
                    expr = new LingoBinaryOpNode
                    {
                        Left = expr,
                        Right = right,
                        Opcode = LingoBinaryOpcode.GreaterThan
                    };
                }
                else if (_currentToken.Type == LingoTokenType.GreaterOrEqual)
                {
                    AdvanceToken();
                    var right = ParseOperand();
                    expr = new LingoBinaryOpNode
                    {
                        Left = expr,
                        Right = right,
                        Opcode = LingoBinaryOpcode.GreaterOrEqual
                    };
                }
                else if (_currentToken.Type == LingoTokenType.LessThan)
                {
                    AdvanceToken();
                    var right = ParseOperand();
                    expr = new LingoBinaryOpNode
                    {
                        Left = expr,
                        Right = right,
                        Opcode = LingoBinaryOpcode.LessThan
                    };
                }
                else if (_currentToken.Type == LingoTokenType.LessOrEqual)
                {
                    AdvanceToken();
                    var right = ParseOperand();
                    expr = new LingoBinaryOpNode
                    {
                        Left = expr,
                        Right = right,
                        Opcode = LingoBinaryOpcode.LessOrEqual
                    };
                }
                else if (_currentToken.Type == LingoTokenType.And)
                {
                    AdvanceToken();
                    var right = ParseExpression();
                    expr = new LingoBinaryOpNode
                    {
                        Left = expr,
                        Right = right,
                        Opcode = LingoBinaryOpcode.And
                    };
                }
                else if (_currentToken.Type == LingoTokenType.Or)
                {
                    AdvanceToken();
                    var right = ParseExpression();
                    expr = new LingoBinaryOpNode
                    {
                        Left = expr,
                        Right = right,
                        Opcode = LingoBinaryOpcode.Or
                    };
                }
                else if (allowEquals && _currentToken.Type == LingoTokenType.Equals)
                {
                    AdvanceToken();
                    var right = ParseExpression(false);
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

                case LingoTokenType.Not:
                    AdvanceToken();
                    var notExpr = ParsePrimary();
                    expr = new LingoNotOpNode { Expr = notExpr };
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
                        var operand = ParsePrimary();
                        expr = new LingoBinaryOpNode
                        {
                            Left = new LingoDatumNode(new LingoDatum(0)),
                            Right = operand,
                            Opcode = LingoBinaryOpcode.Subtract
                        };
                    }
                    break;

                case LingoTokenType.String:
                    expr = new LingoDatumNode(new LingoDatum(_currentToken.Lexeme));
                    AdvanceToken();
                    break;

                case LingoTokenType.LeftBracket:
                    AdvanceToken();
                    var elements = new List<LingoNode>();
                    var isPropList = false;
                    if (Match(LingoTokenType.Colon))
                    {
                        Expect(LingoTokenType.RightBracket);
                        expr = new LingoDatumNode(new LingoDatum(elements, LingoDatum.DatumType.PropList));
                        break;
                    }
                    if (_currentToken.Type != LingoTokenType.RightBracket)
                    {
                        do
                        {
                            var keyOrValue = ParseExpression();
                            if (Match(LingoTokenType.Colon))
                            {
                                var value = ParseExpression();
                                elements.Add(keyOrValue);
                                elements.Add(value);
                                isPropList = true;
                            }
                            else
                            {
                                elements.Add(keyOrValue);
                            }
                        } while (Match(LingoTokenType.Comma));
                    }
                    Expect(LingoTokenType.RightBracket);
                    expr = new LingoDatumNode(new LingoDatum(elements, isPropList ? LingoDatum.DatumType.PropList : LingoDatum.DatumType.List));
                    break;

                case LingoTokenType.Symbol:
                    expr = new LingoDatumNode(new LingoDatum(_currentToken.Lexeme, isSymbol: true));
                    AdvanceToken();
                    break;

                case LingoTokenType.Me:
                    expr = new LingoVarNode { VarName = "me" };
                    AdvanceToken();
                    break;

                case LingoTokenType.Return:
                    expr = new LingoDatumNode(new LingoDatum("\n"));
                    AdvanceToken();
                    break;

                case LingoTokenType.The:
                    AdvanceToken();
                    var propTok = Expect(LingoTokenType.Identifier);
                    expr = new LingoTheExprNode { Prop = propTok.Lexeme };
                    break;

                case LingoTokenType.LeftParen:
                    AdvanceToken();
                    expr = ParseExpression();
                    Expect(LingoTokenType.RightParen);
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
                        LingoNode? castExpr = null;
                        if (Match(LingoTokenType.Comma))
                        {
                            castExpr = ParseExpression();
                        }
                        Expect(LingoTokenType.RightParen);
                        expr = new LingoMemberExprNode { Expr = inner, CastLib = castExpr };
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
                    else if (name.Equals("field", StringComparison.OrdinalIgnoreCase))
                    {
                        var arg = ParseExpression();
                        expr = new LingoObjPropExprNode
                        {
                            Object = new LingoMemberExprNode { Expr = arg },
                            Property = new LingoVarNode { VarName = "Text" }
                        };
                    }
                    else
                    {
                        expr = new LingoVarNode { VarName = name };
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
            // A sprite index can be any valid expression (numbers, variables,
            // property chains, etc.). The previous implementation only allowed
            // identifiers or `me`, leaving the current token untouched when a
            // number was encountered. This resulted in a parser error such as
            // "Expected token RightParen, but got Number" for expressions like
            // `sprite(263).property`.

            // Reuse the general expression parser here so that any valid Lingo
            // expression can serve as the sprite index and the token stream is
            // advanced correctly.
            return ParseExpression();
        }

        private LingoNode ParsePutStatement()
        {
            // At this point, the "put" keyword has already been consumed

            // Parse the value expression (what to put)
            var value = ParseExpression();

            // If followed by "into" parse target; otherwise treat as message output
            if (_currentToken.Type == LingoTokenType.Into)
            {
                var tokType = _currentToken.Type;
                AdvanceToken();
                var target = ParseExpression(false);
                var node = new LingoPutStmtNode(value, target)
                {
                    Type = LingoPutType.Into
                };
                return node;
            }

            return new LingoPutStmtNode(value, null) { Type = LingoPutType.Message };
        }


        private LingoNode ParseIfStatement()
        {
            // "if" already consumed

            var condition = ParseExpression();
            var thenTok = Expect(LingoTokenType.Then);

            while (_currentToken.Type == LingoTokenType.Comment &&
                   _currentToken.Line == thenTok.Line)
            {
                AdvanceToken();
            }

            if (_currentToken.Line == thenTok.Line &&
                _currentToken.Type != LingoTokenType.Comment)
            {
                var stmt = ParseStatement();
                var single = new LingoBlockNode();
                single.Children.Add(stmt);
                return new LingoIfStmtNode(condition, single, null);
            }

            var thenBlock = ParseBlock();

            LingoNode? elseBlock = null;
            if (_currentToken.Type == LingoTokenType.Else)
            {
                var elseTok = _currentToken;
                AdvanceToken(); // consume 'else'
                if (_currentToken.Type == LingoTokenType.If && _currentToken.Line == elseTok.Line)
                {
                    elseBlock = ParseElseIfChain();
                }
                else
                {
                    elseBlock = ParseBlock();
                }
            }

            Expect(LingoTokenType.End);
            Expect(LingoTokenType.If);

            return new LingoIfStmtNode(condition, thenBlock, elseBlock);
        }

        private LingoBlockNode ParseElseIfChain()
        {
            // current token is 'if'
            AdvanceToken(); // consume 'if'
            var cond = ParseExpression();
            Expect(LingoTokenType.Then);
            var thenBlock = ParseBlock();
            LingoNode? elseBlock = null;
            if (_currentToken.Type == LingoTokenType.Else)
            {
                AdvanceToken();
                if (_currentToken.Type == LingoTokenType.If)
                    elseBlock = ParseElseIfChain();
                else
                    elseBlock = ParseBlock();
            }
            var block = new LingoBlockNode();
            block.Children.Add(new LingoIfStmtNode(cond, thenBlock, elseBlock));
            return block;
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
                if (Match(LingoTokenType.Equals))
                {
                    var start = ParseExpression();
                    Expect(LingoTokenType.To);
                    var end = ParseExpression();
                    var body = ParseBlock();
                    Expect(LingoTokenType.End);
                    Expect(LingoTokenType.Repeat);
                    return new LingoRepeatWithStmtNode(varToken.Lexeme, start, end, body);
                }
                else if (Match(LingoTokenType.In))
                {
                    var list = ParseExpression();
                    var body = ParseBlock();
                    Expect(LingoTokenType.End);
                    Expect(LingoTokenType.Repeat);
                    return new LingoRepeatWithInStmtNode
                    {
                        Variable = varToken.Lexeme,
                        List = list,
                        Body = body
                    };
                }
                else
                {
                    throw new Exception($"Expected token Equals or In, but got {_currentToken.Type} at line {_currentToken.Line}");
                }
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



