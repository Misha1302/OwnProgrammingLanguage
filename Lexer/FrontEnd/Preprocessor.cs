using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Lexer.FrontEnd;

public static class Preprocessor
{
    public const char STRING_CHARACTER_INTERNAL = '\"';

    /// <summary>
    ///     Adds EOF character and removes all comments
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public static List<CodeLine> Preprocess(string code)
    {
        code = Regex.Replace(code, "(//.*)|(/\\*(.|\n)*\\*/)", string.Empty, RegexOptions.Compiled) + '\0';

        var split = code.Split('\n');
        var lines = new List<CodeLine>(split.Length);
        lines.AddRange(split.Select((t, index) => new CodeLine(t, index)));
        lines = ClearCodeLines(lines);

        return lines;
    }

    private static List<CodeLine> ClearCodeLines(List<CodeLine> lines)
    {
        lines = lines.Select((x, i) => new CodeLine(x.Line, i)).ToList();
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