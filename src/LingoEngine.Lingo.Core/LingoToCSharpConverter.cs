using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using LingoEngine.Lingo.Core.Tokenizer;

namespace LingoEngine.Lingo.Core;

/// <summary>
/// Converts Lingo source into C# following the mapping rules documented at
/// https://github.com/EmmanuelTheCreator/LingoEngine/blob/main/docs/Lingo_vs_CSharp.md.
/// </summary>
public class LingoToCSharpConverter
{
    public List<ErrorDto> Errors { get; } = new();

    public string Convert(string lingoSource, string methodAccessModifier = "public")
    {
        Errors.Clear();
        lingoSource = lingoSource.Replace("\r", "\n");
        var trimmed = lingoSource.Trim();

        var match = System.Text.RegularExpressions.Regex.Match(
            trimmed,
            @"^member\s*\(\s*""(?<name>[^""]+)""\s*\)\.text$",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return $"Member<LingoMemberText>(\"{match.Groups["name"].Value}\").Text";
        }

        if (TryConvertNewMember(trimmed, out var converted))
            return converted;

        var parser = new LingoAstParser();
        try
        {
            var ast = parser.Parse(lingoSource);
            return CSharpWriter.Write(ast, methodAccessModifier);
        }
        catch (Exception ex)
        {
            LogError(null, lingoSource, ex);
            throw;
        }
    }

    public string Convert(LingoScriptFile script, string methodAccessModifier = "public")
    {
        Errors.Clear();
        var source = script.Source.Replace("\r", "\n");
        var type = DetectScriptType(source);
        var file = new LingoScriptFile { Name = script.Name, Source = source, Type = type };

        var classCode = ConvertClass(file);

        var parser = new LingoAstParser();
        LingoNode ast;
        try
        {
            ast = parser.Parse(source);
        }
        catch (Exception ex)
        {
            LogError(script.Name, source, ex);
            throw;
        }

        var methods = CSharpWriter.Write(ast, methodAccessModifier);
        var insertIdx = classCode.LastIndexOf('}');
        if (insertIdx >= 0)
            classCode = classCode[..insertIdx] + methods + classCode[insertIdx..];
        else
            classCode += methods;
        return classCode;
    }

    public LingoBatchResult Convert(IEnumerable<LingoScriptFile> scripts, string methodAccessModifier = "public")
    {
        Errors.Clear();
        var result = new LingoBatchResult();
        var scriptList = scripts.ToList();
        var asts = new Dictionary<string, LingoNode>();
        var methodMap = new Dictionary<string, string>();
        var propInfo = new Dictionary<string, List<(string Name, string Type, string? Default)>>();

        // First pass: parse scripts, gather handler signatures and property names
        foreach (var file in scriptList)
        {
            var parser = new LingoAstParser();
            try
            {
                var ast = parser.Parse(file.Source);
                asts[file.Name] = ast;
            }
            catch (Exception ex)
            {
                LogError(file.Name, file.Source, ex);
                throw;
            }

            var signatures = ExtractHandlerSignatures(file.Source);
            var methodSigs = new List<MethodSignature>();
            foreach (var kv in signatures)
            {
                var paramInfos = kv.Value.Select(p => new ParameterInfo(p, "object")).ToList();
                methodSigs.Add(new MethodSignature(kv.Key, paramInfos));
                if (!DefaultMethods.Contains(kv.Key) && !methodMap.ContainsKey(kv.Key))
                {
                    methodMap[kv.Key] = file.Name;
                    result.CustomMethods.Add(kv.Key);
                }
            }
            result.Methods[file.Name] = methodSigs;

            var propDecls = ExtractPropertyDeclarations(file.Source);
            var propDescs = ExtractPropertyDescriptions(file.Source);
            var fieldMap = new Dictionary<string, (string Type, string? Default)>(StringComparer.OrdinalIgnoreCase);
            foreach (var n in propDecls)
                fieldMap[n] = ("object", null);
            foreach (var d in propDescs)
                fieldMap[d.Name] = (FormatToCSharpType(d.Format), d.Default);

            propInfo[file.Name] = fieldMap.Select(kv => (kv.Key, kv.Value.Type, kv.Value.Default)).ToList();
        }

        // Second pass: infer property and parameter types
        foreach (var file in asts.Keys.ToList())
        {
            var fields = propInfo.TryGetValue(file, out var list) ? list.ToDictionary(x => x.Name, x => (x.Type, x.Default), StringComparer.OrdinalIgnoreCase) : new Dictionary<string, (string Type, string? Default)>(StringComparer.OrdinalIgnoreCase);
            var source = scriptList.First(s => s.Name == file).Source;
            var inferredProps = InferPropertyTypes(source, fields.Keys);
            foreach (var kv in inferredProps)
            {
                if (fields.TryGetValue(kv.Key, out var existing) && existing.Type == "object")
                    fields[kv.Key] = (kv.Value, existing.Default);
            }
            result.Properties[file] = fields.Select(kv => new PropertyInfo(kv.Key, kv.Value.Type)).ToList();

            var methods = result.Methods.TryGetValue(file, out var msList) ? msList : new List<MethodSignature>();
            var paramTypes = InferParameterTypes(source, methods);
            foreach (var ms in methods)
            {
                if (!paramTypes.TryGetValue(ms.Name, out var map)) continue;
                for (int i = 0; i < ms.Parameters.Count; i++)
                {
                    var p = ms.Parameters[i];
                    if (map.TryGetValue(p.Name, out var type) && p.Type == "object")
                        ms.Parameters[i] = p with { Type = type };
                }
            }
        }

        // Link SendSprite methods
        var generatedBehaviors = new Dictionary<string, string>();
        var annotator = new SendSpriteTypeResolver(methodMap, generatedBehaviors);
        foreach (var ast in asts.Values)
            ast.Accept(annotator);

        // Generate final class code inserting methods
        foreach (var script in scriptList)
        {
            var classCode = ConvertClass(script);
            var methods = CSharpWriter.Write(asts[script.Name], methodAccessModifier);
            var insertIdx = classCode.LastIndexOf('}');
            if (insertIdx >= 0)
                classCode = classCode[..insertIdx] + methods + classCode[insertIdx..];
            else
                classCode += methods;
            result.ConvertedScripts[script.Name] = classCode;
        }

        foreach (var kvp in generatedBehaviors)
            result.ConvertedScripts[kvp.Value] = GenerateSendSpriteBehaviorClass(kvp.Value, kvp.Key);

        return result;
    }

    /// <summary>
    /// Generates a minimal C# class wrapper for a single Lingo script.
    /// Only the class declaration and constructor are emitted.
    /// The class name is derived from the file name plus the script type suffix.
    /// </summary>
    public string ConvertClass(LingoScriptFile script)
    {
        var handlers = ExtractHandlerNames(script.Source);
        var scriptType = handlers.Contains("getPropertyDescriptionList") ? LingoScriptType.Behavior : script.Type;

        var suffix = scriptType switch
        {
            LingoScriptType.Movie => "MovieScript",
            LingoScriptType.Parent => "ParentScript",
            LingoScriptType.Behavior => "Behavior",
            _ => "Script"
        };

        var baseType = scriptType switch
        {
            LingoScriptType.Movie => "LingoMovieScript",
            LingoScriptType.Parent => "LingoParentScript",
            LingoScriptType.Behavior => "LingoSpriteBehavior",
            _ => "LingoScriptBase"
        };

        var className = script.Name + suffix;
        bool hasPropDescHandler = handlers.Contains("getPropertyDescriptionList");
        var propDescs = ExtractPropertyDescriptions(script.Source);
        var propDecls = ExtractPropertyDeclarations(script.Source);

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"public class {className} : {baseType}{(hasPropDescHandler ? ", ILingoPropertyDescriptionList" : string.Empty)}");
        sb.AppendLine("{");

        var fieldMap = new Dictionary<string, (string Type, string? Default)>(StringComparer.OrdinalIgnoreCase);
        foreach (var n in propDecls)
            fieldMap[n] = ("object", null);
        foreach (var d in propDescs)
            fieldMap[d.Name] = (FormatToCSharpType(d.Format), d.Default);

        var inferredTypes = InferPropertyTypes(script.Source, fieldMap.Keys);
        foreach (var kv in inferredTypes)
        {
            if (fieldMap.TryGetValue(kv.Key, out var existing) && existing.Type == "object")
                fieldMap[kv.Key] = (kv.Value, existing.Default);
        }
        if (fieldMap.Count > 0)
        {
            foreach (var kv in fieldMap)
            {
                if (kv.Value.Default != null)
                    sb.AppendLine($"    public {kv.Value.Type} {kv.Key} = {kv.Value.Default};");
                else
                    sb.AppendLine($"    public {kv.Value.Type} {kv.Key};");
            }
            sb.AppendLine();
        }

        bool needsGlobal = scriptType == LingoScriptType.Movie || scriptType == LingoScriptType.Parent;
        if (needsGlobal)
        {
            sb.AppendLine("    private readonly GlobalVars _global;");
            sb.AppendLine();
        }

        sb.Append($"    public {className}(ILingoMovieEnvironment env");
        if (needsGlobal)
            sb.Append(", GlobalVars global");
        sb.Append(") : base(env)");

        if (needsGlobal)
        {
            sb.AppendLine();
            sb.AppendLine("    {");
            sb.AppendLine("        _global = global;");
            sb.AppendLine("    }");
        }
        else
        {
            sb.AppendLine(" { }");
        }

        if (hasPropDescHandler)
        {
            var props = propDescs;
            sb.AppendLine();
            sb.AppendLine("    public BehaviorPropertyDescriptionList? GetPropertyDescriptionList()");
            sb.AppendLine("    {");
            sb.AppendLine("        return new BehaviorPropertyDescriptionList()");
            for (int i = 0; i < props.Count; i++)
            {
                var p = props[i];
                sb.AppendLine($"            .Add(this, x => x.{p.Name}, \"{p.Comment}\", {p.Default})");
            }
            sb.AppendLine("        ;");
            sb.AppendLine("    }");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string GenerateSendSpriteBehaviorClass(string className, string method)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"public class {className} : LingoSpriteBehavior");
        sb.AppendLine("{");
        sb.AppendLine($"    public {className}(ILingoMovieEnvironment env) : base(env) {{ }}");
        sb.AppendLine($"    public object? {method}(params object?[] args) => null;");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static Dictionary<string, List<string>> ExtractHandlerSignatures(string source)
    {
        var dict = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        var regex = new Regex(@"(?im)^\s*on\s+(?<name>\w+)(?<params>[^\r\n]*)");
        foreach (Match m in regex.Matches(source))
        {
            var name = m.Groups["name"].Value;
            var paramStr = m.Groups["params"].Value.Trim();
            var list = new List<string>();
            if (!string.IsNullOrEmpty(paramStr))
            {
                var parts = paramStr.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var p in parts)
                    list.Add(p.Trim());
            }
            dict[name] = list;
        }
        return dict;
    }

    private static HashSet<string> ExtractHandlerNames(string source)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var regex = new System.Text.RegularExpressions.Regex(@"(?im)^\s*on\s+(\w+)");
        foreach (System.Text.RegularExpressions.Match m in regex.Matches(source))
            set.Add(m.Groups[1].Value);
        return set;
    }

    private static List<(string Name, string Default, string Comment, string Format)> ExtractPropertyDescriptions(string source)
    {
        var list = new List<(string Name, string Default, string Comment, string Format)>();
        var regex = new Regex(@"addProp\s+description,#(?<name>\w+),\s*\[(?<body>[^\]]*)\]", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        foreach (Match m in regex.Matches(source))
        {
            var name = m.Groups["name"].Value;
            var body = m.Groups["body"].Value;
            var defMatch = Regex.Match(body, @"#default:(?<val>[^,#\]]+)");
            var fmtMatch = Regex.Match(body, @"#format:#(?<fmt>\w+)");
            var commentMatch = Regex.Match(body, @"#comment:""(.*?)""", RegexOptions.Singleline);
            var def = defMatch.Success ? defMatch.Groups["val"].Value.Trim() : "0";
            var fmt = fmtMatch.Success ? fmtMatch.Groups["fmt"].Value.Trim().ToLowerInvariant() : string.Empty;
            if (fmt == "string" || fmt == "symbol")
                def = $"\"{Escape(def)}\"";
            var comment = commentMatch.Success ? Escape(commentMatch.Groups[1].Value) : string.Empty;
            list.Add((name, def, comment, fmt));
        }
        return list;
    }

    private static List<string> ExtractPropertyDeclarations(string source)
    {
        var list = new List<string>();
        try
        {
            var parser = new LingoAstParser();
            var ast = parser.Parse(source);
            if (ast is LingoBlockNode block)
            {
                foreach (var child in block.Children)
                {
                    if (child is LingoPropertyDeclStmtNode prop)
                        list.AddRange(prop.Names);
                }
            }
        }
        catch
        {
            // ignore parse errors and return whatever was collected so far
        }
        return list;
    }

    private static Dictionary<string, string> InferPropertyTypes(string source, IEnumerable<string> propertyNames)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var targets = new HashSet<string>(propertyNames, StringComparer.OrdinalIgnoreCase);
        var lines = source.Split('\n');
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            foreach (var name in targets)
            {
                if (result.ContainsKey(name)) continue;
                var m = Regex.Match(trimmed, $"(?i)^{Regex.Escape(name)}\\s*=\\s*(.+)$");
                if (m.Success)
                {
                    var rhs = m.Groups[1].Value.Trim();
                    result[name] = InferTypeFromExpression(rhs);
                }
            }
        }
        return result;
    }

    private static Dictionary<string, Dictionary<string, string>> InferParameterTypes(string source, IEnumerable<MethodSignature> methods)
    {
        var result = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        var methodLookup = methods.ToDictionary(m => m.Name, m => m, StringComparer.OrdinalIgnoreCase);
        var lines = source.Split('\n');
        string? current = null;
        foreach (var raw in lines)
        {
            var trimmed = raw.Trim();
            var onMatch = Regex.Match(trimmed, @"(?i)^on\s+(\w+)");
            if (onMatch.Success)
            {
                current = onMatch.Groups[1].Value;
                if (!result.ContainsKey(current))
                    result[current] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                continue;
            }
            if (Regex.IsMatch(trimmed, @"(?i)^end\b"))
            {
                current = null;
                continue;
            }
            if (current == null) continue;
            if (!methodLookup.TryGetValue(current, out var sig)) continue;
            foreach (var param in sig.Parameters)
            {
                if (result[current].ContainsKey(param.Name)) continue;
                var assign = Regex.Match(trimmed, $"(?i)^{Regex.Escape(param.Name)}\\s*=\\s*(.+)$");
                if (assign.Success)
                {
                    result[current][param.Name] = InferTypeFromExpression(assign.Groups[1].Value.Trim());
                    continue;
                }
                if (Regex.IsMatch(trimmed, $"{Regex.Escape(param.Name)}\\.text", RegexOptions.IgnoreCase))
                    result[current][param.Name] = "string";
            }
        }
        return result;
    }

    private static string InferTypeFromExpression(string rhs)
    {
        if (Regex.IsMatch(rhs, @"^"".*""$")) return "string";
        if (rhs.Contains("member(", StringComparison.OrdinalIgnoreCase) && rhs.Contains(").text", StringComparison.OrdinalIgnoreCase)) return "string";
        if (Regex.IsMatch(rhs, @"^(true|false)$", RegexOptions.IgnoreCase)) return "bool";
        if (Regex.IsMatch(rhs, @"^[-+]?[0-9]+$")) return "int";
        if (Regex.IsMatch(rhs, @"^[-+]?[0-9]*\\.[0-9]+$")) return "float";
        return "object";
    }

    private static string FormatToCSharpType(string fmt) => fmt switch
    {
        "integer" => "int",
        "float" => "float",
        "boolean" => "bool",
        "string" or "symbol" => "string",
        _ => "object"
    };

    private static string Escape(string value) => value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n");

    private static bool TryConvertNewMember(string source, out string converted)
    {
        converted = string.Empty;
        var tokenizer = new LingoTokenizer(source);
        var tokens = new List<LingoToken>();
        LingoToken tok;
        do
        {
            tok = tokenizer.NextToken();
            tokens.Add(tok);
        } while (tok.Type != LingoTokenType.Eof && tokens.Count < 32);

        int idx = 0;

        // optional assignment prefix
        string? lhs = null;
        if (tokens.Count > 2 && tokens[0].Type == LingoTokenType.Identifier && tokens[1].Type == LingoTokenType.Equals)
        {
            lhs = tokens[0].Lexeme;
            idx = 2;
        }

        if (idx + 4 >= tokens.Count) return false;
        if (tokens[idx].Type != LingoTokenType.Identifier) return false;
        string obj = tokens[idx].Lexeme;
        if (tokens[idx + 1].Type != LingoTokenType.Dot) return false;
        if (tokens[idx + 2].Type != LingoTokenType.Identifier || !tokens[idx + 2].Lexeme.Equals("newMember", System.StringComparison.OrdinalIgnoreCase))
            return false;
        if (tokens[idx + 3].Type != LingoTokenType.LeftParen) return false;

        int pos = idx + 4;
        if (pos >= tokens.Count) return false;
        if (tokens[pos].Type != LingoTokenType.Symbol) return false;
        string type = tokens[pos].Lexeme.ToLowerInvariant();
        string method = type switch
        {
            "bitmap" => "Bitmap",
            "sound" => "Sound",
            "filmloop" => "FilmLoop",
            "text" => "Text",
            _ => "Member"
        };
        pos++;

        string rest = string.Empty;
        if (pos < tokens.Count && tokens[pos].Type == LingoTokenType.Comma)
        {
            pos++;
            var parts = new List<string>();
            while (pos < tokens.Count && tokens[pos].Type != LingoTokenType.RightParen)
            {
                parts.Add(tokens[pos].Lexeme);
                pos++;
            }
            rest = string.Join(" ", parts);
        }

        if (pos >= tokens.Count || tokens[pos].Type != LingoTokenType.RightParen)
            return false;
        pos++;
        if (pos < tokens.Count && tokens[pos].Type != LingoTokenType.Eof)
            return false;

        var call = $"{obj}.New.{method}({rest})".TrimEnd();
        converted = lhs != null ? $"{lhs} = {call};" : call;
        return true;
    }

    private void LogError(string? file, string source, Exception ex)
    {
        var lineNumber = 0;
        var lineText = string.Empty;
        try
        {
            var match = Regex.Match(ex.Message, @"line (\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out var line))
            {
                var lines = source.Split('\n');
                if (line - 1 >= 0 && line - 1 < lines.Length)
                    lineText = lines[line - 1];
                lineNumber = line;
            }
        }
        catch
        {
            // ignored
        }

        Errors.Add(new ErrorDto(file ?? string.Empty, lineNumber, lineText, ex.Message));
    }

    private static LingoScriptType DetectScriptType(string source)
    {
        if (Regex.IsMatch(source, @"(?im)^\s*on\s+getPropertyDescriptionList\b"))
            return LingoScriptType.Behavior;
        if (Regex.IsMatch(source, @"(?im)^\s*on\s+new\s+me\b"))
            return LingoScriptType.Parent;
        if (Regex.IsMatch(source, @"(?im)^\s*on\s+(startmovie|endmovie)\b"))
            return LingoScriptType.Movie;
        return LingoScriptType.Behavior;
    }

    private static readonly HashSet<string> DefaultMethods = new(
        new[]{"StepFrame","PrepareFrame","EnterFrame","ExitFrame","BeginSprite",
               "EndSprite","MouseDown","MouseUp","MouseMove","MouseEnter",
               "MouseLeave","MouseWithin","MouseExit","PrepareMovie","StartMovie",
               "StopMovie","KeyDown","KeyUp","Focus","Blur"});

    private class CustomMethodCollector : ILingoAstVisitor
    {
        public HashSet<string> Methods { get; } = new();
        public void Visit(LingoHandlerNode n) => n.Block.Accept(this);
        public void Visit(LingoCommentNode n) { }
        public void Visit(LingoNewObjNode n) { n.ObjArgs.Accept(this); }
        public void Visit(LingoLiteralNode n) { }
        public void Visit(LingoCallNode n) { if (!string.IsNullOrEmpty(n.Name)) Methods.Add(n.Name); }
        public void Visit(LingoObjCallNode n) { if (n.Name.Value != null) Methods.Add(n.Name.Value.AsString()); }
        public void Visit(LingoBlockNode n) { foreach (var c in n.Children) c.Accept(this); }
        public void Visit(LingoIfStmtNode n) { n.Condition.Accept(this); n.ThenBlock.Accept(this); if (n.HasElse) n.ElseBlock!.Accept(this); }
        public void Visit(LingoIfElseStmtNode n) { n.Condition.Accept(this); n.ThenBlock.Accept(this); n.ElseBlock.Accept(this); }
        public void Visit(LingoPutStmtNode n) { n.Value.Accept(this); n.Target.Accept(this); }
        public void Visit(LingoBinaryOpNode n) { n.Left.Accept(this); n.Right.Accept(this); }
        public void Visit(LingoCaseStmtNode n) { n.Value.Accept(this); n.Otherwise?.Accept(this); }
        public void Visit(LingoTheExprNode n) { }
        public void Visit(LingoExitStmtNode n) { }
        public void Visit(LingoReturnStmtNode n) { n.Value?.Accept(this); }
        public void Visit(LingoTellStmtNode n) { n.Block.Accept(this); }
        public void Visit(LingoOtherwiseNode n) { n.Block.Accept(this); }
        public void Visit(LingoCaseLabelNode n) { n.Value.Accept(this); n.Block?.Accept(this); }
        public void Visit(LingoChunkExprNode n) { n.Expr.Accept(this); }
        public void Visit(LingoInverseOpNode n) { n.Expr.Accept(this); }
        public void Visit(LingoObjCallV4Node n) { n.Object.Accept(this); if (n.Name.Value != null) Methods.Add(n.Name.Value.AsString()); n.ArgList.Accept(this); }
        public void Visit(LingoMemberExprNode n) { n.Expr.Accept(this); }
        public void Visit(LingoObjPropExprNode n) { n.Object.Accept(this); n.Property.Accept(this); }
        public void Visit(LingoPlayCmdStmtNode n) { n.Command.Accept(this); }
        public void Visit(LingoThePropExprNode n) { n.Property.Accept(this); }
        public void Visit(LingoMenuPropExprNode n) { n.Menu.Accept(this); n.Property.Accept(this); }
        public void Visit(LingoSoundCmdStmtNode n) { n.Command.Accept(this); }
        public void Visit(LingoSoundPropExprNode n) { n.Sound.Accept(this); n.Property.Accept(this); }
        public void Visit(LingoCursorStmtNode n) { n.Value.Accept(this); }
        public void Visit(LingoGoToStmtNode n) { n.Target.Accept(this); }
        public void Visit(LingoAssignmentStmtNode n) { n.Target.Accept(this); n.Value.Accept(this); }
        public void Visit(LingoSendSpriteStmtNode n) { n.Sprite.Accept(this); n.Message.Accept(this); n.Arguments?.Accept(this); }
        public void Visit(LingoSendSpriteExprNode n) { n.Sprite.Accept(this); n.Message.Accept(this); n.Arguments?.Accept(this); }
        public void Visit(LingoObjBracketExprNode n) { n.Object.Accept(this); n.Index.Accept(this); }
        public void Visit(LingoSpritePropExprNode n) { n.Sprite.Accept(this); n.Property.Accept(this); }
        public void Visit(LingoChunkDeleteStmtNode n) { n.Chunk.Accept(this); }
        public void Visit(LingoChunkHiliteStmtNode n) { n.Chunk.Accept(this); }
        public void Visit(LingoRepeatWhileStmtNode n) { n.Condition.Accept(this); n.Body.Accept(this); }
        public void Visit(LingoMenuItemPropExprNode n) { n.MenuItem.Accept(this); n.Property.Accept(this); }
        public void Visit(LingoObjPropIndexExprNode n) { n.Object.Accept(this); n.PropertyIndex.Accept(this); }
        public void Visit(LingoRepeatWithInStmtNode n) { n.List.Accept(this); n.Body.Accept(this); }
        public void Visit(LingoRepeatWithToStmtNode n) { n.Start.Accept(this); n.End.Accept(this); n.Body.Accept(this); }
        public void Visit(LingoSpriteWithinExprNode n) { n.SpriteA.Accept(this); n.SpriteB.Accept(this); }
        public void Visit(LingoLastStringChunkExprNode n) { n.Source.Accept(this); }
        public void Visit(LingoSpriteIntersectsExprNode n) { n.SpriteA.Accept(this); n.SpriteB.Accept(this); }
        public void Visit(LingoStringChunkCountExprNode n) { n.Source.Accept(this); }
        public void Visit(LingoNotOpNode n) { n.Expr.Accept(this); }
        public void Visit(LingoRepeatWithStmtNode n) { n.Start.Accept(this); n.End.Accept(this); n.Body.Accept(this); }
        public void Visit(LingoRepeatUntilStmtNode n) { n.Condition.Accept(this); n.Body.Accept(this); }
        public void Visit(LingoRepeatForeverStmtNode n) { n.Body.Accept(this); }
        public void Visit(LingoRepeatTimesStmtNode n) { n.Count.Accept(this); n.Body.Accept(this); }
        public void Visit(LingoExitRepeatIfStmtNode n) { n.Condition.Accept(this); }
        public void Visit(LingoNextRepeatIfStmtNode n) { n.Condition.Accept(this); }
        public void Visit(LingoErrorNode n) { }
        public void Visit(LingoEndCaseNode n) { }
        public void Visit(LingoWhenStmtNode n) { }
        public void Visit(LingoGlobalDeclStmtNode n) { }
        public void Visit(LingoPropertyDeclStmtNode n) { }
        public void Visit(LingoInstanceDeclStmtNode n) { }
        public void Visit(LingoExitRepeatStmtNode n) { }
        public void Visit(LingoNextRepeatStmtNode n) { }
        public void Visit(LingoVarNode n) { }
        public void Visit(LingoDatumNode n) { }
        public void Visit(LingoNextStmtNode n) { }
    }

    private class SendSpriteTypeResolver : ILingoAstVisitor
    {
        private readonly Dictionary<string, string> _methodMap;
        private readonly Dictionary<string, string> _generated;
        public SendSpriteTypeResolver(Dictionary<string, string> methodMap, Dictionary<string, string> generated)
        {
            _methodMap = methodMap;
            _generated = generated;
        }
        public void Visit(LingoHandlerNode n) => n.Block.Accept(this);
        public void Visit(LingoCommentNode n) { }
        public void Visit(LingoNewObjNode n) { n.ObjArgs.Accept(this); }
        public void Visit(LingoLiteralNode n) { }
        public void Visit(LingoCallNode n)
        {
            if (!string.IsNullOrEmpty(n.Name) && _methodMap.TryGetValue(n.Name, out var script))
                n.TargetType = script;
        }
        public void Visit(LingoObjCallNode n) { n.ArgList.Accept(this); }
        public void Visit(LingoBlockNode n) { foreach (var c in n.Children) c.Accept(this); }
        public void Visit(LingoIfStmtNode n) { n.Condition.Accept(this); n.ThenBlock.Accept(this); if (n.HasElse) n.ElseBlock!.Accept(this); }
        public void Visit(LingoIfElseStmtNode n) { n.Condition.Accept(this); n.ThenBlock.Accept(this); n.ElseBlock.Accept(this); }
        public void Visit(LingoPutStmtNode n) { n.Value.Accept(this); n.Target.Accept(this); }
        public void Visit(LingoBinaryOpNode n) { n.Left.Accept(this); n.Right.Accept(this); }
        public void Visit(LingoCaseStmtNode n) { n.Value.Accept(this); n.Otherwise?.Accept(this); }
        public void Visit(LingoTheExprNode n) { }
        public void Visit(LingoExitStmtNode n) { }
        public void Visit(LingoReturnStmtNode n) { n.Value?.Accept(this); }
        public void Visit(LingoTellStmtNode n) { n.Block.Accept(this); }
        public void Visit(LingoOtherwiseNode n) { n.Block.Accept(this); }
        public void Visit(LingoCaseLabelNode n) { n.Value.Accept(this); n.Block?.Accept(this); }
        public void Visit(LingoChunkExprNode n) { n.Expr.Accept(this); }
        public void Visit(LingoInverseOpNode n) { n.Expr.Accept(this); }
        public void Visit(LingoObjCallV4Node n) { n.Object.Accept(this); n.ArgList.Accept(this); }
        public void Visit(LingoMemberExprNode n) { n.Expr.Accept(this); }
        public void Visit(LingoObjPropExprNode n) { n.Object.Accept(this); n.Property.Accept(this); }
        public void Visit(LingoPlayCmdStmtNode n) { n.Command.Accept(this); }
        public void Visit(LingoThePropExprNode n) { n.Property.Accept(this); }
        public void Visit(LingoMenuPropExprNode n) { n.Menu.Accept(this); n.Property.Accept(this); }
        public void Visit(LingoSoundCmdStmtNode n) { n.Command.Accept(this); }
        public void Visit(LingoSoundPropExprNode n) { n.Sound.Accept(this); n.Property.Accept(this); }
        public void Visit(LingoCursorStmtNode n) { n.Value.Accept(this); }
        public void Visit(LingoGoToStmtNode n) { n.Target.Accept(this); }
        public void Visit(LingoAssignmentStmtNode n) { n.Target.Accept(this); n.Value.Accept(this); }
        public void Visit(LingoSendSpriteStmtNode n)
        {
            if (n.Message is LingoDatumNode dn && dn.Datum.Type == LingoDatum.DatumType.Symbol)
            {
                var name = dn.Datum.AsSymbol();
                if (_methodMap.TryGetValue(name, out var script))
                {
                    n.TargetType = script;
                }
                else
                {
                    if (!_generated.TryGetValue(name, out var className))
                    {
                        className = char.ToUpperInvariant(name[0]) + name[1..] + "Behavior";
                        _generated[name] = className;
                    }
                    _methodMap[name] = className;
                    n.TargetType = className;
                }
            }
            n.Sprite.Accept(this);
            n.Message.Accept(this);
            n.Arguments?.Accept(this);
        }
        public void Visit(LingoSendSpriteExprNode n)
        {
            if (n.Message is LingoDatumNode dn && dn.Datum.Type == LingoDatum.DatumType.Symbol)
            {
                var name = dn.Datum.AsSymbol();
                if (_methodMap.TryGetValue(name, out var script))
                {
                    n.TargetType = script;
                }
                else
                {
                    if (!_generated.TryGetValue(name, out var className))
                    {
                        className = char.ToUpperInvariant(name[0]) + name[1..] + "Behavior";
                        _generated[name] = className;
                    }
                    _methodMap[name] = className;
                    n.TargetType = className;
                }
            }
            n.Sprite.Accept(this);
            n.Message.Accept(this);
            n.Arguments?.Accept(this);
        }
        public void Visit(LingoObjBracketExprNode n) { n.Object.Accept(this); n.Index.Accept(this); }
        public void Visit(LingoSpritePropExprNode n) { n.Sprite.Accept(this); n.Property.Accept(this); }
        public void Visit(LingoChunkDeleteStmtNode n) { n.Chunk.Accept(this); }
        public void Visit(LingoChunkHiliteStmtNode n) { n.Chunk.Accept(this); }
        public void Visit(LingoRepeatWhileStmtNode n) { n.Condition.Accept(this); n.Body.Accept(this); }
        public void Visit(LingoMenuItemPropExprNode n) { n.MenuItem.Accept(this); n.Property.Accept(this); }
        public void Visit(LingoObjPropIndexExprNode n) { n.Object.Accept(this); n.PropertyIndex.Accept(this); }
        public void Visit(LingoRepeatWithInStmtNode n) { n.List.Accept(this); n.Body.Accept(this); }
        public void Visit(LingoRepeatWithToStmtNode n) { n.Start.Accept(this); n.End.Accept(this); n.Body.Accept(this); }
        public void Visit(LingoSpriteWithinExprNode n) { n.SpriteA.Accept(this); n.SpriteB.Accept(this); }
        public void Visit(LingoLastStringChunkExprNode n) { n.Source.Accept(this); }
        public void Visit(LingoSpriteIntersectsExprNode n) { n.SpriteA.Accept(this); n.SpriteB.Accept(this); }
        public void Visit(LingoStringChunkCountExprNode n) { n.Source.Accept(this); }
        public void Visit(LingoNotOpNode n) { n.Expr.Accept(this); }
        public void Visit(LingoVarNode n) { }
        public void Visit(LingoRepeatWithStmtNode n) { n.Start.Accept(this); n.End.Accept(this); n.Body.Accept(this); }
        public void Visit(LingoRepeatUntilStmtNode n) { n.Condition.Accept(this); n.Body.Accept(this); }
        public void Visit(LingoRepeatForeverStmtNode n) { n.Body.Accept(this); }
        public void Visit(LingoRepeatTimesStmtNode n) { n.Count.Accept(this); n.Body.Accept(this); }
        public void Visit(LingoExitRepeatIfStmtNode n) { n.Condition.Accept(this); }
        public void Visit(LingoNextRepeatIfStmtNode n) { n.Condition.Accept(this); }
        public void Visit(LingoErrorNode n) { }
        public void Visit(LingoEndCaseNode n) { }
        public void Visit(LingoWhenStmtNode n) { }
        public void Visit(LingoGlobalDeclStmtNode n) { }
        public void Visit(LingoPropertyDeclStmtNode n) { }
        public void Visit(LingoInstanceDeclStmtNode n) { }
        public void Visit(LingoExitRepeatStmtNode n) { }
        public void Visit(LingoNextRepeatStmtNode n) { }
        public void Visit(LingoDatumNode n) { }
        public void Visit(LingoNextStmtNode n) { }
    }
}
