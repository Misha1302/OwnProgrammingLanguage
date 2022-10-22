using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Lexer.FrontEnd;

public static class Preprocessor
{
    public const char STRING_CHARACTER_INTERNAL = '\"';

    /// <summary>
    ///     Does not significantly change the code and breaks it into lines.
    ///     Removes comments and blank lines
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public static List<CodeLine> Preprocess(string code)
    {
        code = code.Replace("\0", string.Empty) + '\0';

        var split = code.Split('\n');
        var lines = new List<CodeLine>(split.Length);
        lines.AddRange(split.Select((t, index) => new CodeLine(t, index)));
        lines = ClearCodeLines(lines);

        return lines;
    }

    private static List<CodeLine> ClearCodeLines(List<CodeLine> lines)
    {
        lines = lines.Select((x, i) => new CodeLine(Regex.Replace(x.Line, "(//.*)|(/\\*.*\\*/)", string.Empty), i)).ToList();
        lines = lines.Where(x => !string.IsNullOrWhiteSpace(x.Line)).ToList();
        return lines;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public static List<Token> PreprocessTokens(List<Token> tokens)
    {
        tokens.RemoveAll(token1 => token1.TokenKind == Kind.Whitespace);
        return tokens;
    }
}