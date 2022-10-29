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
    internal static List<CodeLine> Preprocess(string code)
    {
        code = DeleteCommentsFromCode(code);

        code = AppendEndOfCode(code);

        var lines = GetCodeLines(code);

        return lines;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static string AppendEndOfCode(string code)
    {
        return code + '\0';
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static List<CodeLine> GetCodeLines(string code)
    {
        var split = code.Split('\n');
        var lines = new List<CodeLine>(split.Length);
        lines.AddRange(split.Select((t, index) => new CodeLine(t, index)));
        lines = ClearCodeLines(lines);
        return lines;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static string DeleteCommentsFromCode(string code)
    {
        var split = code.Split('\"');
        for (var i = 0; i < split.Length; i += 2)
        {
            split[i] = Regex.Replace(split[i], "//.*", string.Empty, RegexOptions.Compiled);
            split[i] = Regex.Replace(split[i], "/\\*(.|\n)*\\*/", string.Empty, RegexOptions.Compiled);
        }

        code = string.Join('"', split);
        return code;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static List<CodeLine> ClearCodeLines(IEnumerable<CodeLine> lines)
    {
        var l = lines.Select((x, i) => new CodeLine(x.Line, i));
        l = l.Where(x => !string.IsNullOrWhiteSpace(x.Line));
        return l.ToList();
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public static List<Token> PreprocessTokens(List<Token> tokens)
    {
        tokens.RemoveAll(token1 => token1.TokenKind == Kind.Whitespace);
        return tokens;
    }
}