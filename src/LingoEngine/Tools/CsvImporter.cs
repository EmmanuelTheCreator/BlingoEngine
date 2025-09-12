using AbstUI.Primitives;
using LingoEngine.Casts;
using LingoEngine.Members;
using LingoEngine.Texts;
using System;
using System.IO;
using System.Text;
using AbstUI.Resources;
using AbstUI.Texts;
using System.Threading.Tasks;

namespace LingoEngine.Tools
{
    public class CsvImporter
    {
        private readonly IAbstResourceManager _resourceManager;

        public CsvImporter(IAbstResourceManager resourceManager)
        {
            _resourceManager = resourceManager;
        }

        public record CsvRow
        {
            public int Number { get; set; }
            public LingoMemberType Type { get; set; }
            public string Name { get; set; }
            public APoint RegPoint { get; set; }
            public string FileName { get; set; }

            public CsvRow(int number, LingoMemberType type, string name, APoint registration, string fileName)
            {
                Number = number;
                Type = type;
                Name = name;
                this.RegPoint = registration;
                FileName = fileName;
            }
        }

        /// <summary>
        /// Format: comma split
        ///     Number,Type,Name,Registration Point,Filename
        ///     1,bitmap,BallB,"(5, 5)",
        /// </summary>
        public void ImportInCastFromCsvFile(ILingoCast cast, string filePath, bool skipFirstLine = true, Action<string>? logWarningMethod = null) =>
            ImportInCastFromCsvFileAsync(cast, filePath, skipFirstLine, logWarningMethod).GetAwaiter().GetResult();

        public async Task ImportInCastFromCsvFileAsync(ILingoCast cast, string filePath, bool skipFirstLine = true, Action<string>? logWarningMethod = null)
        {
            var rootFolder = Path.GetDirectoryName(GetRelativePath(Environment.CurrentDirectory, filePath)) ?? "";
            var csv = await ImportCsvCastFileAsync(filePath, skipFirstLine);
            foreach (var row in csv)
            {
                var fn = row.FileName;
                if (string.IsNullOrWhiteSpace(row.FileName))
                {
                    var ext = ".png";
                    switch (row.Type)
                    {
                        case LingoMemberType.Text:
                            ext = ".txt";
                            break;
                        case LingoMemberType.Field:
                            ext = ".txt";
                            break;
                        case LingoMemberType.Sound:
                            ext = ".wav";
                            break;
                        case LingoMemberType.Script:
                            ext = ".cs";
                            break;
                    }
                    var name = row.Name.Length < 1 ? "" : "_" + row.Name;
                    fn = row.Number + name + ext;
                }
                var fileName = Path.Combine(rootFolder, fn);

                var newMember = cast.Add(row.Type, row.Number, row.Name, fileName, row.RegPoint);
                if (newMember is ILingoMemberTextBase textMember)
                {
                    var mdFile = Path.ChangeExtension(fileName, ".md");
                    var rtfFile = Path.ChangeExtension(fileName, ".rtf");
                    if (await _resourceManager.FileExistsAsync(mdFile))
                    {
                        var mdContent = await _resourceManager.ReadTextFileAsync(mdFile) ?? string.Empty;
                        var markDownData = AbstMarkdownReader.Read(mdContent);
#if DEBUG
                        if (mdFile.Contains("39_xtraNames"))
                        {
                        }
#endif
                        textMember.SetTextMD(markDownData);
#if DEBUG
                        if (mdFile.Contains("39_xtraNames"))
                        {
                            textMember.Preload();
                        }
#endif

                    }
                    else if (await _resourceManager.FileExistsAsync(rtfFile))
                    {
                        var rtfContent = await _resourceManager.ReadTextFileAsync(rtfFile) ?? string.Empty;
                        var md = RtfToMarkdown.Convert(rtfContent, true);
                        textMember.SetTextMD(md);
#if DEBUG
                        File.WriteAllText(mdFile, md.Markdown);
#endif
                    }
                    else
                    {
                        var file = await _resourceManager.ReadTextFileAsync(textMember.FileName) ?? string.Empty;
                        if (file == null)
                        {
                            logWarningMethod?.Invoke("File not found for Text :" + textMember.FileName);
                            continue;
                        }
                        textMember.Text = file;
                    }
                }
            }
        }
        public async Task<IReadOnlyCollection<CsvRow>> ImportCsvCastFileAsync(string filePath, bool skipFirstLine = true)
        {
            var returnData = new List<CsvRow>();
            var csv = await ImportAsync(filePath, skipFirstLine);
            foreach (var row in csv)
            {
                var number = Convert.ToInt32(row[0]);
                var type = row[1];
                var name = row[2];
                var registration = row[3].TrimStart('(').TrimEnd(')').Split(',').Select(int.Parse).ToArray();
                var fileName = row[4];
                var type1 = LingoMemberType.Unknown;
                Enum.TryParse(type, true, out type1);
                returnData.Add(new CsvRow(number, type1, name, (registration[0], registration[1]), fileName));
            }
            return returnData;
        }

        public IReadOnlyCollection<CsvRow> ImportCsvCastFile(string filePath, bool skipFirstLine = true) =>
            ImportCsvCastFileAsync(filePath, skipFirstLine).GetAwaiter().GetResult();

        public async Task<List<string[]>> ImportAsync(string filePath, bool skipFirstLine = true)
        {
            var rows = new List<string[]>();
            var content = await _resourceManager.ReadTextFileAsync(filePath);
            if (content == null)
                return rows;

            using var reader = new StringReader(content);
            string? line;
            var hasSkipped = false;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                if (skipFirstLine && !hasSkipped)
                {
                    hasSkipped = true;
                    continue;
                }

                rows.Add(ParseCsvLine(line));
            }

            return rows;
        }

        public List<string[]> Import(string filePath, bool skipFirstLine = true) =>
            ImportAsync(filePath, skipFirstLine).GetAwaiter().GetResult();

        private string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            var sb = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            sb.Append('"');
                            i++; // Skip next quote
                        }
                        else
                            inQuotes = false;
                    }
                    else
                        sb.Append(c);
                }
                else
                {
                    if (c == ',')
                    {
                        result.Add(sb.ToString());
                        sb.Clear();
                    }
                    else if (c == '"')
                        inQuotes = true;
                    else
                        sb.Append(c);
                }
            }

            result.Add(sb.ToString()); // Add last field
            return result.ToArray();
        }

#if NET48
        private static string GetRelativePath(string basePath, string filePath)
        {
            var baseUri = new Uri(AppendDirectorySeparatorChar(basePath));
            var fileUri = new Uri(filePath);
            var relative = baseUri.MakeRelativeUri(fileUri).ToString().Replace('/', Path.DirectorySeparatorChar);
            return Uri.UnescapeDataString(relative);
        }

        private static string AppendDirectorySeparatorChar(string path)
            => path.EndsWith(Path.DirectorySeparatorChar.ToString()) ? path : path + Path.DirectorySeparatorChar;
#else
        private static string GetRelativePath(string basePath, string filePath) => Path.GetRelativePath(basePath, filePath);
#endif
    }

}
