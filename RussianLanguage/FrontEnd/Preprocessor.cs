using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace RussianLanguage.FrontEnd;

public class Preprocessor
{
    private const int LINE_INDEX_OFFSET = 1;
    private const string WHITE_SPACES_WITHOUT_NEW_LINE = "[ \t\v\f\r]";
    public const char STRING_CHARACTER_INTERNAL = '\"';

    private readonly char _stringCharacter;

    public Preprocessor(char stringCharacter)
    {
        _stringCharacter = stringCharacter;
    }

    /// <summary>
    ///     Prepares the code for splitting into tokens. <br />
    ///     Doesn't remove single line comments!
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public List<CodeLine> Preprocess(string code)
    {
        code = PreprocessCodeInternal(code);

        var split = code.Split('\n');
        var lines = new List<CodeLine>(split.Length);
        lines.AddRange(split.Select((t, index) => new CodeLine(t, index + LINE_INDEX_OFFSET)));
        lines = ClearCodeLines(lines);

        return lines;
    }

    private static List<CodeLine> ClearCodeLines(List<CodeLine> lines)
    {
        lines = lines.Select((x, i) => new CodeLine(Regex.Replace(x.Line, "(//.*)|(/\\*.*\\*/)", ""), i)).ToList();
        lines = lines.Where(x => !string.IsNullOrWhiteSpace(x.Line)).ToList();
        return lines;
    }

    private string PreprocessCodeInternal(string code)
    {
        var unixTimeMilliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();

        code = code.Replace($"\\{_stringCharacter}", unixTimeMilliseconds)
            .Replace(_stringCharacter, STRING_CHARACTER_INTERNAL);

        var split = code.Split(STRING_CHARACTER_INTERNAL);

        for (var i = 0; i < split.Length; i += 2)
        {
            split[i] = Regex.Replace(split[i], $"{WHITE_SPACES_WITHOUT_NEW_LINE}+", " ");
            split[i] = Regex.Replace(split[i], $"(?<=(-)){WHITE_SPACES_WITHOUT_NEW_LINE}+(?=[0-9])", "");
            split[i] = Regex.Replace(split[i], $"\\s+(?=([\\-\\+\\/\\*]))", "");
            split[i] = Regex.Replace(split[i], $"(?<=([\\-\\+\\/\\*]))\\s+", "");
            // split[i] = Regex.Replace(split[i], $"(?<!([\\-\\+\\/\\*]))-(?=(\\s*\\d))", "+-");
        }

        var result = new StringBuilder(string.Join(STRING_CHARACTER_INTERNAL, split));

        result = result.Replace("\0", "");
        result.Append('\0').Append('\0').Append('\0').Append('\0');

        var returnValue = result.ToString();
        returnValue = returnValue.Replace(unixTimeMilliseconds, $"\\{_stringCharacter}");

        return returnValue;
    }

    /// <summary>
    ///     executes preprocessor directives
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public static List<Token> PreprocessTokens(IEnumerable<Token> tokens, bool removeWhitespaces = true)
    {
        var tokensInternal = new List<Token>(tokens);

        if (removeWhitespaces)
            tokensInternal.RemoveAll(token1 => token1.TokenKind == Kind.Whitespace);

        return tokensInternal;
    }
}