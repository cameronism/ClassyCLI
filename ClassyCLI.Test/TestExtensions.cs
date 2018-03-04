using System;
using System.IO;

public static class TestExtensions
{
    [ThreadStatic]
    private static TextWriter _tw;
    public static void SetTextWriter(TextWriter tw)
    {
        _tw = tw;
    }

    public static string H1(this string heading)
    {
        _tw.WriteLine($"# {heading}");
        _tw.WriteLine();
        return heading;
    }

    public static string H2(this string heading)
    {
        _tw.WriteLine($"## {heading}");
        _tw.WriteLine();
        return heading;
    }

    public static string H3(this string heading)
    {
        _tw.WriteLine($"### {heading}");
        _tw.WriteLine();
        return heading;
    }

}