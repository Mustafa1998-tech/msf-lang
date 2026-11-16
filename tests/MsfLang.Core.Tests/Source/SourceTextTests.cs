using MsfLang.Core.Source;
using Xunit;

namespace MsfLang.Core.Tests.Source;

public sealed class SourceTextTests
{
    [Fact]
    public void BomAtStart_IsAcceptedAndRemoved()
    {
        var source = new SourceText("test.msf", "\uFEFFprint(1);");

        Assert.True(source.HasBom);
        Assert.Equal("print(1);", source.Text);
        Assert.Equal(new SourcePosition(1, 1), source.GetPosition(0));
    }

    [Fact]
    public void LineBreaks_LfCrlfCr_CountAsSingleLines()
    {
        var lf = new SourceText("a.msf", "a\nb\nc");
        var crlf = new SourceText("b.msf", "a\r\nb\r\nc");
        var cr = new SourceText("c.msf", "a\rb\rc");

        Assert.Equal(3, lf.LineCount);
        Assert.Equal(3, crlf.LineCount);
        Assert.Equal(3, cr.LineCount);
        Assert.Equal(new SourcePosition(2, 1), lf.GetPosition(2));
        Assert.Equal(new SourcePosition(2, 1), crlf.GetPosition(3));
        Assert.Equal(new SourcePosition(2, 1), cr.GetPosition(2));
    }

    [Fact]
    public void Columns_CountUnicodeScalars_ArabicAndEmoji()
    {
        var arabic = new SourceText("ar.msf", "var س = 1;");
        // v,a,r,space,س,space,=,space,1,; => 'س' is column 5
        Assert.Equal(new SourcePosition(1, 5), arabic.GetPosition(4));

        var emoji = new SourceText("e.msf", "let x = \"😀\";");
        // l,e,t,space,x,space,=,space,",😀,",; => 😀 starts at column 10 (1-based scalar)
        var idx = emoji.Text.IndexOf("😀", StringComparison.Ordinal);
        Assert.Equal(new SourcePosition(1, 10), emoji.GetPosition(idx));
    }

    [Fact]
    public void Tab_CountsAsOneColumn()
    {
        var source = new SourceText("t.msf", "\tprint(1);");
        Assert.Equal(new SourcePosition(1, 2), source.GetPosition(1));
    }

    [Fact]
    public void EofPosition_IsAfterLastCharacter()
    {
        var source = new SourceText("eof.msf", "ab");
        Assert.Equal(new SourcePosition(1, 3), source.GetPosition(2));
    }

    [Fact]
    public void GetLineText_ReturnsLineWithoutBreak()
    {
        var source = new SourceText("lines.msf", "first\r\nsecond\nthird");
        Assert.Equal("first", source.GetLineText(1));
        Assert.Equal("second", source.GetLineText(2));
        Assert.Equal("third", source.GetLineText(3));
    }
}
