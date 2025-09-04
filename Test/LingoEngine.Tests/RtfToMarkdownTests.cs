using System.Linq;
using AbstUI.Texts;
using LingoEngine.Tools;
using Xunit;
namespace LingoEngine.Tests;
public class RtfToMarkdownTests
{
    [Fact]
    public void Convert_ReturnsStyledMarkdown_SingleLine()
    {
        var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;}{\\f0\\fs24\\cf1 plain}{\\b\\f0\\fs24\\cf1 bold}{\\i\\f0\\fs24\\cf1 italic}{\\ul\\f0\\fs24\\cf1 underline}}";

        var data = RtfToMarkdown.Convert(rtf);

        Assert.Equal("{{STYLE:0}}plain**bold***italic*__underline__{{/STYLE}}", data.Markdown);
        Assert.Equal(new[]
        {
            (false, false, false),
            (true, false, false),
            (false, true, false),
            (false, false, true)
        }, data.Segments.Select(s => (s.Bold, s.Italic, s.Underline)));
        Assert.True(data.Styles.ContainsKey("0"));
        Assert.Equal("Arial", data.Styles["0"].Font);
        Assert.Equal(12, data.Styles["0"].FontSize);
        Assert.Equal("#000000", data.Styles["0"].Color.ToHex());
    }

    [Fact]
    public void Convert_ReturnsStyledMarkdown_MultiLine()
    {
        var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;\\red0\\green0\\blue255;}{\\f0\\fs24\\cf1 line1\\par}{\\i\\f0\\fs24\\cf2 line2}}";

        var data = RtfToMarkdown.Convert(rtf);

        var expected = "{{FONT-FAMILY:Arial}}{{FONT-SIZE:12}}{{COLOR:#000000}}{{ALIGN:left}}line1\n{{COLOR:#0000FF}}*line2*";
        Assert.Equal(expected, data.Markdown);
        Assert.Equal(new[]
        {
            (false, false, false),
            (false, true, false)
        }, data.Segments.Select(s => (s.Bold, s.Italic, s.Underline)));
        Assert.True(data.Segments[0].IsParagraph);
        Assert.False(data.Segments[1].IsParagraph);
        Assert.Equal("line1\nline2", data.PlainText);
    }

    [Fact]
    public void Convert_ParsesRightAlignment()
    {
        var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;}\\qr{\\f0\\fs24\\cf1 right}}";

        var data = RtfToMarkdown.Convert(rtf);

        Assert.Equal("{{STYLE:0}}right{{/STYLE}}", data.Markdown);
        Assert.Single(data.Segments);
        Assert.Equal(AbstTextAlignment.Right, data.Styles["0"].Alignment);
    }

    [Fact]
    public void Convert_ParsesCenterAlignment()
    {
        var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;}\\qc{\\f0\\fs24\\cf1 center}}";

        var data = RtfToMarkdown.Convert(rtf);

        Assert.Equal("{{STYLE:0}}center{{/STYLE}}", data.Markdown);
        Assert.Single(data.Segments);
        Assert.Equal(AbstTextAlignment.Center, data.Styles["0"].Alignment);
    }

    [Fact]
    public void Convert_ReturnsMargins()
    {
        var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;}\\li200\\ri400{\\f0\\fs24\\cf1 margin}}";

        var data = RtfToMarkdown.Convert(rtf);

        Assert.Single(data.Segments);
        Assert.Equal("{{STYLE:0}}margin{{/STYLE}}", data.Markdown);
        Assert.Equal(10, data.Styles["0"].MarginLeft);
        Assert.Equal(20, data.Styles["0"].MarginRight);
        Assert.False(data.Segments[0].Bold);
        Assert.False(data.Segments[0].Italic);
        Assert.False(data.Segments[0].Underline);
    }

    [Fact]
    public void Convert_ParsesStylesheetAndUsesStyleIds()
    {
        var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;\\red255\\green0\\blue0;}{\\stylesheet{\\s1\\b\\cf1 style1;}{\\s2\\i\\cf2 style2;}}{\\s1\\f0\\fs24\\cf1 text1}{\\s2\\f0\\fs24\\cf2 text2}}";

        var data = RtfToMarkdown.Convert(rtf);

        Assert.Equal("{{STYLE:1}}{{FONT-FAMILY:Arial}}{{FONT-SIZE:12}}{{ALIGN:left}}text1{{/STYLE}}{{STYLE:2}}text2{{/STYLE}}", data.Markdown);
        Assert.Equal(1, data.Segments[0].StyleId);
        Assert.Equal(2, data.Segments[1].StyleId);
        Assert.True(data.Styles.ContainsKey("1"));
        Assert.True(data.Styles["1"].Bold);
        Assert.True(data.Styles.ContainsKey("2"));
        Assert.True(data.Styles["2"].Italic);
    }

    [Fact]
    public void Convert_DoesNotEmitFontTagsWhenStyleDefinesThem()
    {
        var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;\\red255\\green0\\blue0;}{\\stylesheet{\\s1\\f0\\fs24\\cf2 style1;}}{\\s1\\f0\\fs24\\cf2 text}}";

        var data = RtfToMarkdown.Convert(rtf);

        Assert.Equal("{{STYLE:1}}{{ALIGN:left}}text{{/STYLE}}", data.Markdown);
        Assert.Equal(1, data.Segments[0].StyleId);
        Assert.True(data.Styles.ContainsKey("1"));
        Assert.Equal("Arial", data.Styles["1"].Font);
        Assert.Equal(12, data.Styles["1"].FontSize);
        Assert.Equal("#FF0000", data.Styles["1"].Color.ToHex());
    }

    [Fact]
    public void Convert_ParsesControlWords_ScoreText()
    {
        var rtf = "{\\rtf1\\ansi\\deff0 {\\fonttbl{\\f0\\fswiss Arial;}{\\f1\\fmodern Courier;}{\\f2\\fnil Earth *;}{\\f3\\fnil 8Pin Matrix *;}{\\f4\\fnil Anke Print;}" +
                  "{\\f5\\fnil Arcade *;}{\\f6\\fnil Arcade *;}}{\\colortbl\\red0\\green0\\blue0;\\red128\\green128\\blue128;\\red170\\green0\\blue0;\\red255\\green0\\blue0;}{\\stylesheet{\\s0\\fs24 Normal Text;}}" +
                  "\\pard \\f0\\fs24{\\pard \\b\\f6\\fs60\\cf3\\qr\\tx HI-SCORES}}";

        var data = RtfToMarkdown.Convert(rtf);

        var expected = "{{STYLE:0}}**HI-SCORES**{{/STYLE}}";
        Assert.Equal(expected, data.Markdown);
        Assert.Equal("HI-SCORES", data.PlainText);
        Assert.Equal(AbstTextAlignment.Right, data.Styles["0"].Alignment);
    }

    [Fact]
    public void Convert_ParsesControlWords_NumberText()
    {
        var rtf = "{\\rtf1\\ansi\\deff0 {\\fonttbl{\\f0\\fswiss Arial;}{\\f1\\fmodern Courier;}{\\f2\\fnil Earth *;}{\\f3\\fnil 8Pin Matrix *;}{\\f4\\fnil Anke Print;}" +
                  "{\\f5\\fnil Arcade *;}{\\f6\\fnil Arcade *;}}{\\colortbl\\red0\\green0\\blue0;\\red128\\green128\\blue128;\\red170\\green0\\blue0;\\red255\\green0\\blue0;}{\\stylesheet{\\s0\\fs24 Normal Text;}}" +
                  "\\pard \\f0\\fs24{\\pard \\b\\f6\\fs72\\cf3\\qr\\tx 1}}";

        var data = RtfToMarkdown.Convert(rtf);

        var expected = "{{STYLE:0}}**1**{{/STYLE}}";
        Assert.Equal(expected, data.Markdown);
        Assert.Equal("1", data.PlainText);
        Assert.Equal(AbstTextAlignment.Right, data.Styles["0"].Alignment);
    }

    [Fact]
    public void Convert_ParsesControlWords_MarginRight()
    {
        var rtf = "{\\rtf1\\ansi\\deff0 {\\fonttbl{\\f0\\fswiss Arial;}{\\f1\\fmodern Courier;}{\\f2\\fnil Earth *;}}{\\colortbl\\red0\\green0\\blue0;\\red128\\green128\\blue128;\\red0\\green0\\blue224;\\red224\\green0\\blue0;\\red224\\green0\\blue224;}{\\stylesheet{\\s0\\fs24 Normal Text;}}" +
                  "\\pard \\f0\\fs24{\\pard \\b\\f2\\fs48\\cf1\\ri560 GAME OVER}}";

        var data = RtfToMarkdown.Convert(rtf);

        var expected = "{{STYLE:0}}**GAME OVER**{{/STYLE}}";
        Assert.Equal(expected, data.Markdown);
        Assert.Equal("GAME OVER", data.PlainText);
        Assert.Equal(28, data.Styles["0"].MarginRight);
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

        Assert.Contains("Actionkey = Space", data.PlainText);
        Assert.Contains("Press P", data.PlainText);
        Assert.Contains("for Pause", data.PlainText);
        Assert.Contains("Press S", data.PlainText);
        Assert.Contains("to stop", data.PlainText);
        Assert.Contains("Press ESC", data.PlainText);
        Assert.Contains("to Quit", data.PlainText);
    }

    [Fact]
    public void Convert_HandlesConstructionKitRtf()
    {
        var rtf = @"{\rtf1\ansi\deff0 {\fonttbl{\f0\fswiss Arial;}{\f1\fnil Tahoma *;}{\f2\fnil Bikly *;}{\f3\fnil Earth *;}{\f4\fnil Bikly;}{\f5\fnil Arcade *;}
}{\colortbl\red0\green0\blue0;\red255\green0\blue0;\red170\green170\blue170;\red255\green255\blue255;\red0\green0\blue224;\red224
\green0\blue0;\red224\green0\blue224;}{\stylesheet{\s0\fs24 Normal Text;}}\pard \f0\fs24{\pard \b\i\f5\fs36\cf3\sl380 Construction Kit}
}";

        var data = RtfToMarkdown.Convert(rtf);

        Assert.Equal("Construction Kit", data.PlainText);
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

        Assert.Contains("::Multi BALLS::", data.PlainText);
        Assert.Contains("::Black Balls::", data.PlainText);
        Assert.DoesNotContain("\\", data.PlainText);
    }

    [Fact]
    public void Convert_PreservesLeadingTab()
    {
        var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;}{\\f0\\fs24\\cf1\\tabAfter}}";

        var data = RtfToMarkdown.Convert(rtf);

        Assert.Equal("\tAfter", data.PlainText);
    }

    [Fact]
    public void Convert_ParsesLineHeight()
    {
        var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0\\green0\\blue0;}\\pard{\\f0\\fs24\\cf1\\sl360 line}}";

        var data = RtfToMarkdown.Convert(rtf);

        Assert.Equal("line", data.PlainText);
        Assert.Equal(18, data.Segments[0].LineHeight);
    }

}
