using System;
using System.Collections.Generic;
using System.Text;
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
    private readonly LingoToCSharpConverterSettings _settings;

    public List<ErrorDto> Errors { get; } = new();

    public LingoToCSharpConverter() : this(new LingoToCSharpConverterSettings()) { }

    public LingoToCSharpConverter(LingoToCSharpConverterSettings settings)
    {
        _settings = settings;
    }

    private static string JoinContinuationLines(string source)
    {
        var lines = source.Split('\n');
        if (lines.Length == 0) return source;

        var sb = new StringBuilder();
        var previousContinues = false;

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var trimmedEnd = line.TrimEnd();

            if (previousContinues)
                sb.Append(trimmedEnd.TrimStart());
            else
            {
                if (i > 0)
                    sb.Append('\n');
                sb.Append(trimmedEnd);
            }

            if (trimmedEnd.EndsWith("\\"))
            {
                sb.Length--; // remove trailing backslash
                previousContinues = true;
            }
            else
            {
                previousContinues = false;
            }
        }

        return sb.ToString();
    }

    public string Convert(string lingoSource, ConversionOptions? options = null)
    {
        Errors.Clear();
        options ??= new ConversionOptions();
        lingoSource = lingoSource.Replace("\r", "\n");
        lingoSource = JoinContinuationLines(lingoSource);
        var trimmed = lingoSource.Trim();

        var match = System.Text.RegularExpressions.Regex.Match(
            trimmed,
            @"^member\s*\(\s*""(?<name>[^""]+)""\s*\)\.(?<prop>text|line|word|char)$",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (match.Success)
        {
            var prop = match.Groups["prop"].Value;
            var pascal = char.ToUpperInvariant(prop[0]) + prop[1..];
            return $"GetMember<ILingoMemberTextBase>(\"{match.Groups["name"].Value}\").{pascal}";
        }

        match = System.Text.RegularExpressions.Regex.Match(
            trimmed,
            @"^castLib\s*\(\s*(?<arg>[^\)]+)\s*\)\.save\s*\(\s*\)\s*$",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return $"CastLib({match.Groups["arg"].Value}).Save();";
        }

        if (TryConvertNewMember(trimmed, out var converted))
            return converted;

        var parser = new LingoAstParser();
        try
        {
            var ast = parser.Parse(lingoSource);
            return CSharpWriter.Write(ast, options.MethodAccessModifier, settings: _settings);
        }
        catch (Exception ex)
        {
            LogError(null, lingoSource, ex);
            throw;
        }
    }

    public string Convert(LingoScriptFile script, ConversionOptions? options = null)
    {
        Convert(new[] { script }, options);
        return script.CSharp;
    }

    public LingoBatchResult Convert(IEnumerable<LingoScriptFile> scripts, ConversionOptions? options = null)
    {
        Errors.Clear();
        options ??= new ConversionOptions();
        var result = new LingoBatchResult();
        var scriptList = scripts.ToList();
        var asts = new Dictionary<string, LingoNode>();
        var methodMap = new Dictionary<string, string>();
        var propInfo = new Dictionary<string, List<(string Name, string Type, string? Default)>>();
        var newHandlers = new Dictionary<string, LingoHandlerNode?>();
        var sourceMap = new Dictionary<string, string>();
        foreach (var s in scriptList)
        {
            var src = s.Source.Replace("\r", "\n");
            src = JoinContinuationLines(src);
            sourceMap[s.Name] = src;
            var st = s.Detection switch
            {
                ScriptDetectionType.Auto => DetectScriptType(src),
                ScriptDetectionType.Behavior => LingoScriptType.Behavior,
                ScriptDetectionType.Parent => LingoScriptType.Parent,
                ScriptDetectionType.Movie => LingoScriptType.Movie,
                _ => LingoScriptType.Behavior
            };
            s.Type = st;
        }
        var typeMap = new Dictionary<string, LingoScriptType>(StringComparer.OrdinalIgnoreCase);
        foreach (var s in scriptList)
        {
            typeMap[s.Name] = s.Type;
            typeMap[CSharpName.SanitizeIdentifier(s.Name)] = s.Type;
            typeMap[CSharpName.NormalizeScriptName(s.Name)] = s.Type;
        }

        // First pass: parse scripts, gather handler signatures and property names
        foreach (var file in scriptList)
        {
            var fileItem = file;

            try
            {
                var parser = new LingoAstParser();
                try
                {
                    var source = sourceMap[file.Name];
                    var ast = parser.Parse(source);
                    if (ast is LingoBlockNode block)
                    {
                        var nh = block.Children.OfType<LingoHandlerNode>()
                            .FirstOrDefault(h => h.Handler != null && h.Handler.Name.Equals("new", StringComparison.OrdinalIgnoreCase));
                        newHandlers[file.Name] = nh;
                        if (nh != null)
                            block.Children.Remove(nh);
                    }
                    asts[file.Name] = ast;
                }
                catch (Exception ex)
                {
                    LogError(file.Name, sourceMap[file.Name], ex);
                    throw;
                }

                var signatures = ExtractHandlerSignatures(sourceMap[file.Name]);
                signatures.Remove("new");
                var methodSigs = new List<MethodSignature>();
                foreach (var kv in signatures)
                {
                    var paramInfos = kv.Value.Select(p => new ParameterInfo(p, "object")).ToList();
                    methodSigs.Add(new MethodSignature(kv.Key, paramInfos));
                    if (!DefaultMethods.Contains(kv.Key) && !methodMap.ContainsKey(kv.Key))
                    {
                        var className = CSharpName.ComposeName(file.Name, file.Type, _settings);
                        methodMap[kv.Key] = className;
                        result.CustomMethods.Add(kv.Key);
                    }
                }
                result.Methods[file.Name] = methodSigs;

                var propDecls = ExtractPropertyDeclarations(sourceMap[file.Name]);
                var propDescs = ExtractPropertyDescriptions(sourceMap[file.Name]);
                var fieldMap = new Dictionary<string, (string Type, string? Default)>(StringComparer.OrdinalIgnoreCase);
                foreach (var n in propDecls)
                    fieldMap[n] = ("object", null);
                foreach (var d in propDescs)
                    fieldMap[d.Name] = (FormatToCSharpType(d.Format), d.Default);

                propInfo[file.Name] = fieldMap.Select(kv => (kv.Key, kv.Value.Type, kv.Value.Default)).ToList();
            }
            catch (Exception ex)
            {
                fileItem.Errors += Environment.NewLine + ex.Message;
            }
        }

        // Second pass: infer property and parameter types
        foreach (var file in asts.Keys.ToList())
        {
            var fileItem = scripts.First(x => x.Name == file);

            try
            {
                var fields = propInfo.TryGetValue(file, out var list) ? list.ToDictionary(x => x.Name, x => (x.Type, x.Default), StringComparer.OrdinalIgnoreCase) : new Dictionary<string, (string Type, string? Default)>(StringComparer.OrdinalIgnoreCase);
                var source = sourceMap[file];
                var inferredProps = InferPropertyTypes(source, fields.Keys);
                foreach (var kv in inferredProps)
                {
                    if (fields.TryGetValue(kv.Key, out var existing) && existing.Type == "object")
                        fields[kv.Key] = (kv.Value, existing.Default);
                }
                result.Properties[file] = fields.Select(kv => new PropertyInfo(kv.Key, kv.Value.Type)).ToList();

                var methods = result.Methods.TryGetValue(file, out var msList) ? msList : new List<MethodSignature>();
                var paramTypes = InferParameterTypes(source, methods, fields.ToDictionary(kv => kv.Key, kv => kv.Value.Type, StringComparer.OrdinalIgnoreCase));
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
            catch (Exception ex)
            {
                fileItem.Errors += Environment.NewLine + ex.Message;
            }
        }

        // Link SendSprite methods
        var generatedBehaviors = new Dictionary<string, string>();
        var annotator = new SendSpriteTypeResolver(methodMap, generatedBehaviors, _settings);
        foreach (var ast in asts)
        {
            var fileItem = scripts.First(x => x.Name == ast.Key);

            try
            {
                ast.Value.Accept(annotator);
            }
            catch (Exception ex)
            {
                fileItem.Errors += Environment.NewLine + ex.Message;
            }
        }


        // Generate final class code inserting methods
        foreach (var script in scriptList)
        {
            try
            {
                newHandlers.TryGetValue(script.Name, out var nh);
                var classCode = ConvertClass(script, nh, typeMap);
                var sigMap = result.Methods.TryGetValue(script.Name, out var msList)
                    ? msList.ToDictionary(m => m.Name, m => m, StringComparer.OrdinalIgnoreCase)
                    : new Dictionary<string, MethodSignature>(StringComparer.OrdinalIgnoreCase);
                var methods = CSharpWriter.Write(asts[script.Name], options.MethodAccessModifier, typeMap, sigMap, _settings);
                var insertIdx = classCode.LastIndexOf('}');
                if (insertIdx >= 0)
                    classCode = classCode[..insertIdx] + methods + classCode[insertIdx..];
                else
                    classCode += methods;

                var ns = BuildNamespace(script, options);
                var sb = new StringBuilder();
                sb.AppendLine("using System;");
                sb.AppendLine("using LingoEngine.Lingo.Core;");
                sb.AppendLine();
                if (!string.IsNullOrWhiteSpace(ns))
                {
                    sb.Append("namespace ").Append(ns).AppendLine(";");
                    sb.AppendLine();
                }
                sb.Append(classCode);
                var finalCode = sb.ToString();
                result.ConvertedScripts[script.Name] = finalCode;
                script.CSharp = finalCode;
            }
            catch (Exception ex)
            {
                script.Errors += Environment.NewLine + ex.Message;
            }
            script.Errors += GetCurrentErrorsAndFlush();
        }

        foreach (var kvp in generatedBehaviors)
        {
            var fileItem = scripts.FirstOrDefault(x => x.Name == kvp.Value);

            try
            {
                var safeName = CSharpName.SanitizeIdentifier(kvp.Value);
                var scriptBody = GenerateSendSpriteBehaviorClass(safeName, kvp.Key);
                var ns = BuildNamespace(fileItem ?? new LingoScriptFile(kvp.Value, string.Empty), options);
                var sb = new StringBuilder();
                sb.AppendLine("using System;");
                sb.AppendLine("using LingoEngine.Lingo.Core;");
                sb.AppendLine();
                if (!string.IsNullOrWhiteSpace(ns))
                {
                    sb.Append("namespace ").Append(ns).AppendLine(";");
                    sb.AppendLine();
                }
                sb.Append(scriptBody);
                var script = sb.ToString();
                result.ConvertedScripts[kvp.Value] = script;
                if (fileItem != null)
                    fileItem.CSharp = script;
            }
            catch (Exception ex)
            {
                if (fileItem != null)
                    fileItem.Errors += Environment.NewLine + ex.Message;
            }
            if (fileItem != null)
                fileItem.Errors += GetCurrentErrorsAndFlush();
        }

        return result;
    }
    public string GetCurrentErrorsAndFlush()
    {
        var errors = string.Join("\n", Errors.Select(e =>
           string.IsNullOrEmpty(e.File)
               ? $"Line {e.LineNumber}: {e.LineText} - {e.Error}"
               : $"{e.File}:{e.LineNumber}: {e.LineText} - {e.Error}"));
        Errors.Clear();
        return errors;
    }
    /// <summary>
    /// Generates a minimal C# class wrapper for a single Lingo script.
    /// Only the class declaration and constructor are emitted.
    /// The class name is derived from the file name plus the script type suffix.
    /// </summary>
    public string ConvertClass(LingoScriptFile script)
    {
        LingoHandlerNode? newHandler = null;
        try
        {
            var source = script.Source.Replace("\r", "\n");
            var parser = new LingoAstParser();
            var ast = parser.Parse(source);
            if (ast is LingoBlockNode block)
                newHandler = block.Children.OfType<LingoHandlerNode>()
                    .FirstOrDefault(h => h.Handler != null && h.Handler.Name.Equals("new", StringComparison.OrdinalIgnoreCase));
            var normalized = new LingoScriptFile(script.Name, source, script.Type);
            return ConvertClass(normalized, newHandler);
        }
        catch
        {
            // ignore parse errors here; they will surface later if needed
        }

        return ConvertClass(script, newHandler);
    }

    public string ConvertClass(LingoScriptFile script, LingoHandlerNode? newHandler, IReadOnlyDictionary<string, LingoScriptType>? scriptTypes = null)
    {
        var source = script.Source.Replace("\r", "\n");
        var handlers = ExtractHandlerNames(source);
        var scriptType = handlers.Contains("getPropertyDescriptionList") ? LingoScriptType.Behavior : script.Type;

        var baseType = scriptType switch
        {
            LingoScriptType.Movie => "LingoMovieScript",
            LingoScriptType.Parent => "LingoParentScript",
            LingoScriptType.Behavior => "LingoSpriteBehavior",
            _ => "LingoScriptBase"
        };

        var className = CSharpName.ComposeName(script.Name, scriptType, _settings);
        bool hasPropDescHandler = handlers.Contains("getPropertyDescriptionList");
        var propDescs = ExtractPropertyDescriptions(source);
        var propDecls = ExtractPropertyDeclarations(source);

        var sb = new System.Text.StringBuilder();
        var interfaces = new List<string>();
        if (hasPropDescHandler)
            interfaces.Add("ILingoPropertyDescriptionList");

        var eventInterfaces = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["blur"] = "IHasBlurEvent",
            ["focus"] = "IHasFocusEvent",
            ["keydown"] = "IHasKeyDownEvent",
            ["keyup"] = "IHasKeyUpEvent",
            ["mousewithin"] = "IHasMouseWithinEvent",
            ["mouseleave"] = "IHasMouseLeaveEvent",
            ["mousedown"] = "IHasMouseDownEvent",
            ["mouseup"] = "IHasMouseUpEvent",
            ["mousemove"] = "IHasMouseMoveEvent",
            ["mousewheel"] = "IHasMouseWheelEvent",
            ["mouseenter"] = "IHasMouseEnterEvent",
            ["mouseexit"] = "IHasMouseExitEvent",
            ["beginsprite"] = "IHasBeginSpriteEvent",
            ["endsprite"] = "IHasEndSpriteEvent",
            ["stepframe"] = "IHasStepFrameEvent",
            ["prepareframe"] = "IHasPrepareFrameEvent",
            ["enterframe"] = "IHasEnterFrameEvent",
            ["exitframe"] = "IHasExitFrameEvent"
        };

        foreach (var kv in eventInterfaces)
        {
            if (handlers.Contains(kv.Key))
                interfaces.Add(kv.Value);
        }

        var interfaceSuffix = interfaces.Count > 0 ? ", " + string.Join(", ", interfaces) : string.Empty;

        sb.AppendLine($"public class {className} : {baseType}{interfaceSuffix}");
        sb.AppendLine("{");

        var fieldMap = new Dictionary<string, (string Type, string? Default)>(StringComparer.OrdinalIgnoreCase);
        foreach (var n in propDecls)
            fieldMap[n] = ("object", null);
        foreach (var d in propDescs)
            fieldMap[d.Name] = (FormatToCSharpType(d.Format), d.Default);

        var inferredTypes = InferPropertyTypes(source, fieldMap.Keys);
        foreach (var kv in inferredTypes)
        {
            if (fieldMap.TryGetValue(kv.Key, out var existing) && existing.Type == "object")
                fieldMap[kv.Key] = (kv.Value, existing.Default);
        }
        var fieldTypes = fieldMap.ToDictionary(kv => kv.Key, kv => kv.Value.Type, StringComparer.OrdinalIgnoreCase);
        if (fieldMap.Count > 0)
        {
            foreach (var kv in fieldMap)
            {
                if (kv.Value.Default != null)
                    sb.AppendLine($"    public {kv.Value.Type} {kv.Key} = {kv.Value.Default};");
                else if (kv.Value.Type.StartsWith("LingoList<"))
                    sb.AppendLine($"    public {kv.Value.Type} {kv.Key} = new();");
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
        Dictionary<string, string> ctorParamTypes = new(StringComparer.OrdinalIgnoreCase);
        if (newHandler != null)
        {
            var argNames = newHandler.Handler.ArgumentNames.Where(a => !a.Equals("me", StringComparison.OrdinalIgnoreCase)).ToList();
            if (argNames.Count > 0)
            {
                var ms = new MethodSignature("new", argNames.Select(a => new ParameterInfo(a, "object")).ToList());
                var map = InferParameterTypes(source, new[] { ms }, fieldTypes);
                if (map.TryGetValue("new", out var m))
                    ctorParamTypes = m;
            }
            foreach (var arg in argNames)
            {
                var type = ctorParamTypes.TryGetValue(arg, out var t) ? t : "object";
                sb.Append($", {type} {arg}");
            }
        }
        sb.Append(") : base(env)");

        if (!needsGlobal && newHandler == null)
        {
            sb.AppendLine(" { }");
        }
        else
        {
            sb.AppendLine();
            sb.AppendLine("    {");
            if (needsGlobal)
                sb.AppendLine("        _global = global;");
            if (newHandler != null)
            {
                var methodCode = CSharpWriter.Write(newHandler, "public", scriptTypes, settings: _settings);
                var bodyStart = methodCode.IndexOf('{');
                var bodyEnd = methodCode.LastIndexOf('}');
                var body = bodyStart >= 0 && bodyEnd > bodyStart
                    ? methodCode.Substring(bodyStart + 1, bodyEnd - bodyStart - 1)
                    : string.Empty;
                foreach (var line in body.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                    sb.AppendLine("        " + line.Trim());
            }
            sb.AppendLine("    }");
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

    private static string BuildNamespace(LingoScriptFile script, ConversionOptions options)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(options.Namespace))
            parts.Add(options.Namespace);

        if (!string.IsNullOrWhiteSpace(script.RelativeDirectory))
        {
            var dirs = script.RelativeDirectory.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var d in dirs)
            {
                var seg = CSharpName.SanitizeIdentifier(d);
                if (seg.Length > 0)
                    parts.Add(char.ToUpperInvariant(seg[0]) + seg[1..]);
            }
        }

        return string.Join('.', parts);
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
            var defMatch = Regex.Match(body, @"#default:(?<val>[^#\]]+)");
            var fmtMatch = Regex.Match(body, @"#format:#(?<fmt>\w+)");
            var commentMatch = Regex.Match(body, @"#comment:""(.*?)""", RegexOptions.Singleline);
            var def = defMatch.Success ? defMatch.Groups["val"].Value.Trim().TrimEnd(',') : "0";
            var fmt = fmtMatch.Success ? fmtMatch.Groups["fmt"].Value.Trim().ToLowerInvariant() : string.Empty;
            if (Regex.IsMatch(def, @"(?i)rgb\s*\((.+)\)"))
                def = Regex.Replace(def, @"(?i)rgb\s*\((.+)\)", "AColor.FromCode($1)");
            if (fmt == "string" || fmt == "symbol")
                def = $"\"{Escape(def.Trim('"'))}\"";
            else if (fmt == "color" && Regex.IsMatch(def, @"^\d+$"))
                def = $"AColor.FromCode({def})";
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

        if (list.Count == 0)
        {
            var regex = new Regex(@"(?im)^\s*property\s+(.+)$");
            foreach (Match m in regex.Matches(source))
            {
                var names = m.Groups[1].Value;
                foreach (var n in names.Split(new[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
                    list.Add(n.Trim());
            }
        }
        return list;
    }

    private static Dictionary<string, string> InferPropertyTypes(string source, IEnumerable<string> propertyNames)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var targets = new HashSet<string>(propertyNames, StringComparer.OrdinalIgnoreCase);
        var lines = source.Split('\n');
        var locals = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            var anyAssign = Regex.Match(trimmed, @"(?i)^(\w+)\\s*=\\s*(.+)$");
            if (anyAssign.Success)
            {
                var lhs = anyAssign.Groups[1].Value;
                var rhsAll = anyAssign.Groups[2].Value.Trim();
                var t = InferTypeFromExpression(rhsAll);
                if (!targets.Contains(lhs))
                    locals[lhs] = t;
            }

            foreach (var name in targets)
            {
                var existingType = result.TryGetValue(name, out var ex) ? ex : null;
                if (existingType != null && existingType != "object") continue;
                var m = Regex.Match(trimmed, $"(?i)^{Regex.Escape(name)}\\s*=\\s*(.+)$");
                if (m.Success)
                {
                    var rhs = Regex.Replace(m.Groups[1].Value, @"--.*$", string.Empty).Trim();
                    var type = InferTypeFromExpression(rhs);
                    if (type == "object")
                    {
                        var idx = Regex.Match(rhs, @"^(\w+)\[");
                        if (idx.Success)
                        {
                            var srcName = idx.Groups[1].Value;
                            if (result.TryGetValue(srcName, out var srcType) && srcType.StartsWith("LingoList<"))
                            {
                                var elemType = Regex.Match(srcType, @"<(.*)>").Groups[1].Value;
                                type = elemType;
                            }
                            else if (locals.TryGetValue(srcName, out var localType) && localType.StartsWith("LingoList<"))
                            {
                                var elemType = Regex.Match(localType, @"<(.*)>").Groups[1].Value;
                                type = elemType;
                            }
                        }
                    }
                    result[name] = type;
                    continue;
                }

                var listOp = Regex.Match(trimmed,
                    $"(?i)^(append|add|addat|setat|setprop|setaprop|addprop)\\s+{Regex.Escape(name)}\\s*,\\s*(?:[^,]+,\\s*)?(.+)$");
                if (listOp.Success)
                {
                    var rhs = Regex.Replace(listOp.Groups[2].Value, @"--.*$", string.Empty).Trim();
                    var elemType = InferTypeFromExpression(rhs);
                    var opName = listOp.Groups[1].Value.ToLowerInvariant();
                    if (opName.Contains("prop"))
                        result[name] = $"LingoPropertyList<{elemType}>";
                    else
                        result[name] = $"LingoList<{elemType}>";
                    continue;
                }

                var propOps = $"(?i)^(deleteprop|getprop|getaprop|getpropat|findpos|findposnear|setaprop)\\s+{Regex.Escape(name)}\\b";
                var listOps = $"(?i)^(deleteat|deleteone|getat|getpos|count|max|min)\\s+{Regex.Escape(name)}\\b";
                if (!result.ContainsKey(name))
                {
                    if (Regex.IsMatch(trimmed, propOps))
                        result[name] = "LingoPropertyList<object>";
                    else if (Regex.IsMatch(trimmed, listOps))
                        result[name] = "LingoList<object>";
                }

                if (!result.ContainsKey(name))
                {
                    var listElem = Regex.Match(trimmed, $"(?i)^{Regex.Escape(name)}\\s*=\\s*(\\w+)\\[");
                    if (listElem.Success)
                    {
                        var src = listElem.Groups[1].Value;
                        if (result.TryGetValue(src, out var srcType) && srcType.StartsWith("LingoList<"))
                        {
                            var elemType = Regex.Match(srcType, @"<(.*)>").Groups[1].Value;
                            result[name] = elemType;
                            continue;
                        }
                    }
                    var direct = Regex.Match(trimmed, $"(?i)^{Regex.Escape(name)}\\s*=\\s*(\\w+)\\b");
                    if (direct.Success)
                    {
                        var src = direct.Groups[1].Value;
                        if (result.TryGetValue(src, out var srcType))
                        {
                            result[name] = srcType;
                            continue;
                        }
                    }
                }
            }
        }
        return result;
    }

    private static Dictionary<string, Dictionary<string, string>> InferParameterTypes(
        string source,
        IEnumerable<MethodSignature> methods,
        IDictionary<string, string> propertyTypes)
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
                var assign = Regex.Match(trimmed, @$"(?i)(?:^|then\s+|;\s*){Regex.Escape(param.Name)}\s*=\s*(.+)$");
                if (assign.Success)
                {
                    var rhsExpr = Regex.Replace(assign.Groups[1].Value, @"--.*$", string.Empty).Trim();
                    var rhsType = InferTypeFromExpression(rhsExpr);
                    if (rhsType == "object")
                    {
                        var rhsVar = Regex.Match(rhsExpr, @"^(\w+)$");
                        if (rhsVar.Success && propertyTypes.TryGetValue(rhsVar.Groups[1].Value, out var propType))
                            rhsType = propType;
                    }
                    result[current][param.Name] = rhsType;
                    continue;
                }

                var reverse = Regex.Match(trimmed, @$"(?i)(?:^|then\s+|;\s*)(\w+)\s*=\s*{Regex.Escape(param.Name)}\b");
                if (reverse.Success)
                {
                    var lhs = reverse.Groups[1].Value;
                    if (propertyTypes.TryGetValue(lhs, out var propType))
                    {
                        result[current][param.Name] = propType;
                        continue;
                    }
                }

                if (Regex.IsMatch(trimmed, $"{Regex.Escape(param.Name)}\\.text", RegexOptions.IgnoreCase))
                {
                    result[current][param.Name] = "string";
                    continue;
                }

                var concatPattern1 = "\"[^\"]*\"\\s*(?:&&|&)\\s*" + Regex.Escape(param.Name) + "\\b";
                var concatPattern2 = Regex.Escape(param.Name) + "\\s*(?:&&|&)\\s*\"";
                if (Regex.IsMatch(trimmed, concatPattern1, RegexOptions.IgnoreCase) ||
                    Regex.IsMatch(trimmed, concatPattern2, RegexOptions.IgnoreCase))
                {
                    result[current][param.Name] = "string";
                }
            }
        }
        return result;
    }

    private static string InferTypeFromExpression(string rhs)
    {
        if (Regex.IsMatch(rhs, "^\".*\"$")) return "string";
        if (rhs.Contains("member(", StringComparison.OrdinalIgnoreCase) && rhs.Contains(").text", StringComparison.OrdinalIgnoreCase)) return "string";
        if (Regex.IsMatch(rhs, @"(?i)^rgb\s*\(")) return "AColor";
        if (rhs.StartsWith("[") && rhs.EndsWith("]"))
        {
            var inner = rhs.Substring(1, rhs.Length - 2);
            var elems = inner.Split(',').Select(e => e.Trim()).Where(e => e.Length > 0).ToList();
            if (elems.Count == 0) return "LingoList<object>";
            var elemType = InferTypeFromExpression(elems[0]);
            foreach (var e in elems.Skip(1))
            {
                var t = InferTypeFromExpression(e);
                if (t != elemType)
                {
                    elemType = "object";
                    break;
                }
            }
            return $"LingoList<{elemType}>";
        }
        if (Regex.IsMatch(rhs, @"^(true|false)$", RegexOptions.IgnoreCase)) return "bool";
        if (Regex.IsMatch(rhs, @"^[-+]?[0-9]+$")) return "int";
        if (Regex.IsMatch(rhs, @"^[-+]?[0-9]*\\.[0-9]+$")) return "float";
        if (Regex.IsMatch(rhs, @"[-+*/]") && Regex.IsMatch(rhs, @"[0-9]"))
        {
            if (rhs.Contains('.')) return "float";
            return "int";
        }
        return "object";
    }

    private static string FormatToCSharpType(string fmt) => fmt switch
    {
        "integer" => "int",
        "float" => "float",
        "boolean" => "bool",
        "string" or "symbol" => "string",
        "color" => "AColor",
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

    private static readonly HashSet<string> DefaultMethods = new(new[]
        {
            "StepFrame","PrepareFrame","EnterFrame","ExitFrame","BeginSprite",
            "EndSprite","MouseDown","MouseUp","MouseMove","MouseEnter",
            "MouseLeave","MouseWithin","MouseExit","PrepareMovie","StartMovie",
            "StopMovie","KeyDown","KeyUp","Focus","Blur"
        }, StringComparer.OrdinalIgnoreCase);

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
        public void Visit(LingoPutStmtNode n) { n.Value.Accept(this); n.Target?.Accept(this); }
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
        public void Visit(LingoMemberExprNode n) { n.Expr.Accept(this); n.CastLib?.Accept(this); }
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
        public void Visit(LingoRangeExprNode n) { n.Start.Accept(this); n.End.Accept(this); }
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
        private readonly LingoToCSharpConverterSettings _settings;
        public SendSpriteTypeResolver(Dictionary<string, string> methodMap, Dictionary<string, string> generated, LingoToCSharpConverterSettings settings)
        {
            _methodMap = methodMap;
            _generated = generated;
            _settings = settings;
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
        public void Visit(LingoPutStmtNode n) { n.Value.Accept(this); n.Target?.Accept(this); }
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
        public void Visit(LingoMemberExprNode n) { n.Expr.Accept(this); n.CastLib?.Accept(this); }
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
                        var pascal = char.ToUpperInvariant(name[0]) + name[1..];
                        className = CSharpName.ComposeName(pascal, LingoScriptType.Behavior, _settings);
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
                        var pascal = char.ToUpperInvariant(name[0]) + name[1..];
                        className = CSharpName.ComposeName(pascal, LingoScriptType.Behavior, _settings);
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
        public void Visit(LingoRangeExprNode n) { n.Start.Accept(this); n.End.Accept(this); }
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
