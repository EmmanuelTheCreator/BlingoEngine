using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using AbstUI.Components.Graphics;
using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.Texts;
using LingoEngine.Tools;
using FluentAssertions;
using Xunit;
namespace LingoEngine.Tests;
public class RtfToMarkdownTests
{
    [Fact]
    public void Convert_ReturnsStyledMarkdown_SingleLine()
    {
        var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;}{\\f0\\fs24\\cf1 plain}{\\b\\f0\\fs24\\cf1 bold}{\\i\\f0\\fs24\\cf1 italic}{\\ul\\f0\\fs24\\cf1 underline}}";

        var data = RtfToMarkdown.Convert(rtf);

        data.Markdown.Should().Be("{{PARA:0}}plain**bold***italic*__underline__");
        data.Segments.Select(s => (s.Bold, s.Italic, s.Underline)).Should().Equal(new[]
        {
            (false, false, false),
            (true, false, false),
            (false, true, false),
            (false, false, true)
        });
        data.Styles.Should().ContainKey("0");
        data.Styles["0"].Font.Should().Be("Arial");
        data.Styles["0"].FontSize.Should().Be(12);
        data.Styles["0"].Color.ToHex().Should().Be("#000000");
    }

    [Fact]
    public void Convert_ReturnsStyledMarkdown_MultiLine()
    {
        var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;\\red0\\green0\\blue255;}{\\f0\\fs24\\cf1 line1\\par}{\\i\\f0\\fs24\\cf2 line2}}";

        var data = RtfToMarkdown.Convert(rtf);

        var expected = "{{PARA:0}}line1\n{{PARA:1}}*line2*";
        data.Markdown.Should().Be(expected);
        data.Segments.Select(s => (s.Bold, s.Italic, s.Underline)).Should().Equal(new[]
        {
            (false, false, false),
            (false, true, false)
        });
        data.Segments[0].IsParagraph.Should().BeTrue();
        data.Segments[1].IsParagraph.Should().BeFalse();
        data.Segments[0].StyleId.Should().Be(0);
        data.Segments[1].StyleId.Should().Be(1);
        data.Styles["1"].Color.ToHex().Should().Be("#0000FF");
        data.PlainText.Should().Be("line1\nline2");
    }

    [Fact]
    public void Convert_ReusesStylesAcrossParagraphs()
    {
        var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;}" +
                  "{\\f0\\fs24\\cf1 one\\par}" +
                  "{\\f0\\fs28\\cf1 two\\par}" +
                  "{\\f0\\fs24\\cf1 one again}}";

        var data = RtfToMarkdown.Convert(rtf);

        var expected = "{{PARA:0}}one\n{{PARA:1}}two\n{{PARA:0}}one again";
        data.Markdown.Should().Be(expected);
        data.Segments.Select(s => s.StyleId).Should().Equal(new[] { 0, 1, 0 });
        data.Styles.Should().HaveCount(2);
        data.Styles["0"].FontSize.Should().Be(12);
        data.Styles["1"].FontSize.Should().Be(14);
    }

    [Fact]
    public void Convert_ParsesRightAlignment()
    {
        var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;}\\qr{\\f0\\fs24\\cf1 right}}";

        var data = RtfToMarkdown.Convert(rtf);

        data.Markdown.Should().Be("{{PARA:0}}right");
        data.Segments.Should().HaveCount(1);
        data.Styles["0"].Alignment.Should().Be(AbstTextAlignment.Right);
    }

    [Fact]
    public void Convert_ParsesCenterAlignment()
    {
        var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;}\\qc{\\f0\\fs24\\cf1 center}}";

        var data = RtfToMarkdown.Convert(rtf);

        data.Markdown.Should().Be("{{PARA:0}}center");
        data.Segments.Should().HaveCount(1);
        data.Styles["0"].Alignment.Should().Be(AbstTextAlignment.Center);
    }

    [Fact]
    public void Convert_ReturnsMargins()
    {
        var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;}\\li200\\ri400{\\f0\\fs24\\cf1 margin}}";

        var data = RtfToMarkdown.Convert(rtf);

        data.Segments.Should().HaveCount(1);
        data.Markdown.Should().Be("{{PARA:0}}margin");
        data.Styles["0"].MarginLeft.Should().Be(10);
        data.Styles["0"].MarginRight.Should().Be(20);
        data.Segments[0].Bold.Should().BeFalse();
        data.Segments[0].Italic.Should().BeFalse();
        data.Segments[0].Underline.Should().BeFalse();
    }

    [Fact]
    public void Convert_ParsesStylesheetAndUsesStyleIds()
    {
        var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;\\red255\\green0\\blue0;}{\\stylesheet{\\s1\\b\\cf1 style1;}{\\s2\\i\\cf2 style2;}}{\\s1\\f0\\fs24\\cf1 text1}{\\s2\\f0\\fs24\\cf2 text2}}";

        var data = RtfToMarkdown.Convert(rtf);

        data.Markdown.Should().Be("{{PARA:1}}{{FONT-FAMILY:Arial}}{{FONT-SIZE:12}}{{ALIGN:left}}text1{{STYLE:2}}text2{{/STYLE}}");
        data.Segments[0].StyleId.Should().Be(1);
        data.Segments[1].StyleId.Should().Be(2);
        data.Styles.Should().ContainKey("1");
        data.Styles["1"].Bold.Should().BeTrue();
        data.Styles.Should().ContainKey("2");
        data.Styles["2"].Italic.Should().BeTrue();
    }

    [Fact]
    public void Convert_DoesNotEmitFontTagsWhenStyleDefinesThem()
    {
        var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;\\red255\\green0\\blue0;}{\\stylesheet{\\s1\\f0\\fs24\\cf2 style1;}}{\\s1\\f0\\fs24\\cf2 text}}";

        var data = RtfToMarkdown.Convert(rtf);

        data.Markdown.Should().Be("{{PARA:1}}{{ALIGN:left}}text");
        data.Segments[0].StyleId.Should().Be(1);
        data.Styles.Should().ContainKey("1");
        data.Styles["1"].Font.Should().Be("Arial");
        data.Styles["1"].FontSize.Should().Be(12);
        data.Styles["1"].Color.ToHex().Should().Be("#FF0000");
    }

    [Fact]
    public void Convert_ParsesControlWords_ScoreText()
    {
        var rtf = "{\\rtf1\\ansi\\deff0 {\\fonttbl{\\f0\\fswiss Arial;}{\\f1\\fmodern Courier;}{\\f2\\fnil Earth *;}{\\f3\\fnil 8Pin Matrix *;}{\\f4\\fnil Anke Print;}" +
                  "{\\f5\\fnil Arcade *;}{\\f6\\fnil Arcade *;}}{\\colortbl\\red0\\green0\\blue0;\\red128\\green128\\blue128;\\red170\\green0\\blue0;\\red255\\green0\\blue0;}{\\stylesheet{\\s0\\fs24 Normal Text;}}" +
                  "\\pard \\f0\\fs24{\\pard \\b\\f6\\fs60\\cf3\\qr\\tx HI-SCORES}}";

        var data = RtfToMarkdown.Convert(rtf);

        var expected = "{{PARA:0}}**HI-SCORES**";
        data.Markdown.Should().Be(expected);
        data.PlainText.Should().Be("HI-SCORES");
        data.Styles["0"].Alignment.Should().Be(AbstTextAlignment.Right);
    }

    [Fact]
    public void Convert_ParsesControlWords_NumberText()
    {
        var rtf = "{\\rtf1\\ansi\\deff0 {\\fonttbl{\\f0\\fswiss Arial;}{\\f1\\fmodern Courier;}{\\f2\\fnil Earth *;}{\\f3\\fnil 8Pin Matrix *;}{\\f4\\fnil Anke Print;}" +
                  "{\\f5\\fnil Arcade *;}{\\f6\\fnil Arcade *;}}{\\colortbl\\red0\\green0\\blue0;\\red128\\green128\\blue128;\\red170\\green0\\blue0;\\red255\\green0\\blue0;}{\\stylesheet{\\s0\\fs24 Normal Text;}}" +
                  "\\pard \\f0\\fs24{\\pard \\b\\f6\\fs72\\cf3\\qr\\tx 1}}";

        var data = RtfToMarkdown.Convert(rtf);

        var expected = "{{PARA:0}}**1**";
        data.Markdown.Should().Be(expected);
        data.PlainText.Should().Be("1");
        data.Styles["0"].Alignment.Should().Be(AbstTextAlignment.Right);
    }

    [Fact]
    public void Convert_ParsesControlWords_MarginRight()
    {
        var rtf = "{\\rtf1\\ansi\\deff0 {\\fonttbl{\\f0\\fswiss Arial;}{\\f1\\fmodern Courier;}{\\f2\\fnil Earth *;}}{\\colortbl\\red0\\green0\\blue0;\\red128\\green128\\blue128;\\red0\\green0\\blue224;\\red224\\green0\\blue0;\\red224\\green0\\blue224;}{\\stylesheet{\\s0\\fs24 Normal Text;}}" +
                  "\\pard \\f0\\fs24{\\pard \\b\\f2\\fs48\\cf1\\ri560 GAME OVER}}";

        var data = RtfToMarkdown.Convert(rtf);

        var expected = "{{PARA:0}}**GAME OVER**";
        data.Markdown.Should().Be(expected);
        data.PlainText.Should().Be("GAME OVER");
        data.Styles["0"].MarginRight.Should().Be(28);
    }


    [Fact]
    public void Convert_HandlesActionKeyInstructions()
    {
        var rtf = @"{\rtf1\ansi\deff0 {\fonttbl{\f0\fswiss Arial;}{\f1\fnil Tahoma *;}}{\colortbl\red0\green0\blue0;\red102\green102\blue102;\red255\green0 \blue0;\red153\green153\blue153;\red187\green187\blue187;\red0\green0\blue224;\red224\green0\blue0;\red224\green0\blue224;}{\stylesheet
{\s0\fs24 Normal Text;}}\pard \f0\fs24{\pard \i\f1\cf3\expnd4\sl240 Actionkey}{\pard \b\i\f1\cf3\expnd4\sl240  = }{\pard \b\i\f1\cf4\expnd4\sl240 Space}{\pard \b\i\f1\cf3\expnd4\sl240\par
}{\pard \i\f1\cf3\expnd4\sl240 Press }{\pard \b\i\f1\cf4\expnd4\sl240 P}{\pard \b\i\f1\cf3\expnd4\sl240  }{\pard \i\f1\cf3\expnd4
\sl240 for Pause}{\pard \b\i\f1\cf3\expnd4\sl240\par
}{\pard \i\f1\cf3\expnd4\sl240 Press }{\pard \b\i\f1\cf4\expnd4\sl240 S }{\pard \i\f1\cf3\expnd4\sl240 to stop}{\pard \b\i\f1\cf3
\expnd4\sl240\par
}{\pard \i\f1\cf3\expnd4\sl240 Press }{\pard \b\i\f1\cf4\expnd4\sl240 ESC}{\pard \b\i\f1\cf3\expnd4\sl240  }{\pard \i\f1\cf3\expnd4
\sl240 to Quit}}";

        var data = RtfToMarkdown.Convert(rtf);

        data.PlainText.Should().Contain("Actionkey = Space");
        data.PlainText.Should().Contain("Press P");
        data.PlainText.Should().Contain("for Pause");
        data.PlainText.Should().Contain("Press S");
        data.PlainText.Should().Contain("to stop");
        data.PlainText.Should().Contain("Press ESC");
        data.PlainText.Should().Contain("to Quit");
    }

    [Fact]
    public void Convert_HandlesConstructionKitRtf()
    {
        var rtf = @"{\rtf1\ansi\deff0 {\fonttbl{\f0\fswiss Arial;}{\f1\fnil Tahoma *;}{\f2\fnil Bikly *;}{\f3\fnil Earth *;}{\f4\fnil Bikly;}{\f5\fnil Arcade *;}
}{\colortbl\red0\green0\blue0;\red255\green0\blue0;\red170\green170\blue170;\red255\green255\blue255;\red0\green0\blue224;\red224
\green0\blue0;\red224\green0\blue224;}{\stylesheet{\s0\fs24 Normal Text;}}\pard \f0\fs24{\pard \b\i\f5\fs36\cf3\sl380 Construction Kit}
}";

        var data = RtfToMarkdown.Convert(rtf);

        data.PlainText.Should().Be("Construction Kit");
    }

    [Fact]
    public void Convert_HandlesColonEscapes()
    {
        var rtf = @"{\rtf1\ansi\deff0 {\fonttbl{\f0\fswiss Arial;}{\f1\fmodern Courier;}}{\colortbl\red0\green0\blue0;\red128\green128\blue128;}{\stylesheet
{\s0\fs24 Normal Text;}}\pard \f0\fs24{\f1\cf1 ..................................................\:\:Multi BALLS\:\:....................................................
\par
..................................................\:\:Black Balls\:\:....................................................}{\pard 
\plain\f1\fs24\par
 }{\pard \f1\cf1 ....................................................\:\:Expand\:\:........................................................
\par
...................................................\:\:XTRA Balls\:\:.....................................................}{\pard 
\plain\f1\fs24\par
}{\pard \f1\cf1 ...................................................\:\:Slow Down\:\:.......................................................}
{\pard \plain\f1\fs24\par
}{\pard \f1\cf1 .....................................................\:\:Shoot\:\:...........................................................}
{\pard \plain\f1\fs24\par
}{\pard \f1\cf1 ....................................................\:\:Speed Up\:\:......................................................}
{\pard \plain\f1\fs24\par
}{\pard \f1\cf1 ......................................................\:\:Magnetic\:\:...................................................}
{\pard \plain\f1\fs24\par
}{\pard \f1\cf1 ......................................................\:\:Live\:\:..........................................................}
{\pard \plain\f1\fs24\par
}{\pard \f1\cf1 ......................................................\:\:Gates\:\:..........................................................}
}";

        var data = RtfToMarkdown.Convert(rtf);

        var lines = data.Markdown.Split('\n');
        lines.Should().HaveCount(10);
        lines[0].Should().MatchRegex(@"^\{\{PARA:0\}\}.*::Multi BALLS::.*$");
        lines[1].Should().MatchRegex(@"^\{\{PARA:0\}\}.*::Black Balls::.*$");
        lines[2].Should().MatchRegex(@"^\{\{PARA:0\}\}.*::Expand::.*$");
        lines[3].Should().MatchRegex(@"^\{\{PARA:0\}\}.*::XTRA Balls::.*$");
        lines[4].Should().MatchRegex(@"^\{\{PARA:0\}\}.*::Slow Down::.*$");
        lines[5].Should().MatchRegex(@"^\{\{PARA:0\}\}.*::Shoot::.*$");
        lines[6].Should().MatchRegex(@"^\{\{PARA:0\}\}.*::Speed Up::.*$");
        lines[7].Should().MatchRegex(@"^\{\{PARA:0\}\}.*::Magnetic::.*$");
        lines[8].Should().MatchRegex(@"^\{\{PARA:0\}\}.*::Live::.*$");
        lines[9].Should().MatchRegex(@"^\{\{PARA:0\}\}.*::Gates::.*$");

        data.Styles.Should().HaveCount(1);
        data.Segments.Should().AllSatisfy(s => s.StyleId.Should().Be(0));
        var style = data.Styles["0"];
        style.Font.Should().Be("Courier");
        style.FontSize.Should().Be(12);
        style.Color.ToHex().Should().Be("#808080");
        style.Alignment.Should().Be(AbstTextAlignment.Left);
        data.PlainText.Should().Contain("::Multi BALLS::");
        data.PlainText.Should().Contain("::Black Balls::");
        data.PlainText.Should().NotContain("\\");
    }

    [Fact]
    public void Convert_PreservesLeadingTab()
    {
        var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;}{\\f0\\fs24\\cf1\\tabAfter}}";

        var data = RtfToMarkdown.Convert(rtf);

        data.PlainText.Should().Be("\tAfter");
    }

    [Fact]
    public void Convert_ParsesLineHeight()
    {
        var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;}\\pard{\\f0\\fs24\\cf1\\sl360 line}}";

        var data = RtfToMarkdown.Convert(rtf);

        data.PlainText.Should().Be("line");
        data.Segments[0].LineHeight.Should().Be(18);
    }

    [Fact]
    public void Convert_HandlesColorIndexWithoutLeadingSemicolon()
    {
        const string rtf = "{\\rtf1\\ansi\\deff0 {\\fonttbl{\\f0\\fswiss Arial;}{\\f1\\fnil Arcade *;}{\\f2\\fnil Earth *;}}{\\colortbl\\red0\\green0\\blue0;\\red255\\green0\r\n\\blue0;}{\\stylesheet{\\s0\\fs24 Normal Text;}}\\pard \\f0\\fs24{\\pard \\f2\\fs36\\cf1\\qc New }{\\pard \\b\\f2\\fs36\\cf1\\qc Highscore!!!}{\\pard \r\n\\f2\\fs36\\cf1\\qc\\par\r\n}{\\pard \r\n\\f2\\fs28\\cf1\\qc Enter your }{\\pard \\f2\\fs36\\cf1\\qc Name}}";

        var data = RtfToMarkdown.Convert(rtf);

        var expected = "{{PARA:1}}New **Highscore!!!**\n{{PARA:2}}Enter your {{FONT-SIZE:18}}Name";
        data.Markdown.Should().Be(expected);

        data.Styles.Should().ContainKey("1");
        data.Styles.Should().ContainKey("2");
        var style1 = data.Styles["1"];
        style1.Font.Should().Be("Earth");
        style1.FontSize.Should().Be(18);
        style1.Color.ToHex().Should().Be("#FF0000");
        style1.Alignment.Should().Be(AbstTextAlignment.Center);
        var style2 = data.Styles["2"];
        style2.FontSize.Should().Be(14);
    }

    [Fact]
    public void Convert_IncludesStyleSheetTag_WhenRequested()
    {
        var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;}{\\f0\\fs24\\cf1 text}}";

        var data = RtfToMarkdown.Convert(rtf, includeStyleSheet: true);

        data.Markdown.Should().StartWith("{{STYLE-SHEET:");
        var prefix = "{{STYLE-SHEET:";
        var jsonEnd = data.Markdown.IndexOf("}}", prefix.Length, StringComparison.Ordinal);
        jsonEnd.Should().BeGreaterThan(0);
        var end = data.Markdown.IndexOf("}}", jsonEnd + 2, StringComparison.Ordinal);
        end.Should().BeGreaterThan(0);
        var json = data.Markdown.Substring(prefix.Length, jsonEnd - prefix.Length + 2);
        var sheet = JsonSerializer.Deserialize<Dictionary<string, MarkdownStyleSheetTTO>>(json);
        sheet.Should().NotBeNull();
        sheet!["0"].FontFamily.Should().Be("Arial");
        sheet["0"].FontSize.Should().Be(12);
        sheet["0"].Color.Should().BeNull();
    }

    [Fact]
    public void Convert_ConvertsUnicodeCharacters()
    {
        var rtf = "{\\rtf1\\ansi\\uc0\\deff0 {\\fonttbl{\\f0\\fswiss Arial;}{\\f1\\fnil Tahoma;}}{\\colortbl;\\red153\\green153\\blue153;}{\\stylesheet{\\s0\\fs24 Normal Text;}}\\pard \\f0\\fs24{\\pard \\f1\\fs24\\cf1 \\u169 2005 by Emmanuel The Creator\\par}}";

        var data = RtfToMarkdown.Convert(rtf);

        data.PlainText.Should().Be("© 2005 by Emmanuel The Creator\n");
    }

    [Fact]
    public void Convert_UsesSingleRightAlignedGrayTahomaStyle()
    {
        var rtf = "{\\rtf1\\ansi\\deff0 {\\fonttbl{\\f0\\fswiss Arial;}{\\f1\\fnil Tahoma;}}{\\colortbl\\red0\\green0\\blue0;\\red153\\green153\\blue153;}{\\stylesheet{\\s0\\fs24 Normal Text;}}\\pard\\qr \\f0\\fs24{\\f1\\fs24\\cf1 nele\\par\nnele\\par\nMichael\\par\nMichael\\par\nnele\\par\nnele\\par\nManu\\par\nMichael\\par\nMichael\\par\nLaura\\par}}";

        var data = RtfToMarkdown.Convert(rtf);

        var expected = "{{PARA:0}}nele\n{{PARA:0}}nele\n{{PARA:0}}Michael\n{{PARA:0}}Michael\n{{PARA:0}}nele\n{{PARA:0}}nele\n{{PARA:0}}Manu\n{{PARA:0}}Michael\n{{PARA:0}}Michael\n{{PARA:0}}Laura\n{{PARA:0}}";
        data.Markdown.Should().Be(expected);
        data.Styles.Should().HaveCount(1);
        data.Segments.Should().OnlyContain(s => s.StyleId == 0);
        data.Segments.Should().OnlyContain(s => s.IsParagraph);
        var style2 = data.Styles["0"];
        style2.Font.Should().Be("Tahoma");
        style2.FontSize.Should().Be(12);
        style2.Color.ToHex().Should().Be("#999999");
        style2.Alignment.Should().Be(AbstTextAlignment.Right);
    }
}
