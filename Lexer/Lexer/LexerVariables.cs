using Lexer.FrontEnd;

namespace Lexer.Lexer;

internal static class LexerVariables
{
    #region Properties

    internal static string Code => LinePosition < CodeLines.Count ? CodeLines[LinePosition].Line : "\0";

    #endregion

    #region Constants

    internal const char COMMENT_CHAR = '#';
    internal const char EOF_CHAR = '\0';
    internal const char NEW_LINE_CHAR = '\n';

    #endregion

    #region Readonly fields

    internal static readonly IReadOnlyDictionary<string, Kind> Words = new Dictionary<string, Kind>
    {
        { "from", Kind.From },
        { "bool", Kind.BoolType },
        { "string", Kind.StringType },
        { "int", Kind.IntType },
        { "float", Kind.FloatType },
        { "void", Kind.VoidType },
        { "call", Kind.Call },
        { "using", Kind.Using }
        // { "if", Kind.If },
        // { "else", Kind.Else }
    };

    internal static readonly IReadOnlyDictionary<string, Kind> Other = new Dictionary<string, Kind>
    {
        { "(", Kind.OpenParenthesis },
        { ")", Kind.CloseParenthesis },

        { "[", Kind.OpenBracket },
        { "]", Kind.CloseBracket },

        { ",", Kind.Comma },
        { ".", Kind.Dot },
        { "::", Kind.MethodSeparator },

        { "==", Kind.EqualsBoolSign },
        { "!=", Kind.NotEqualsBoolSign },
        { ">", Kind.GreatThanBoolSign },
        { "<", Kind.LessThanLessBoolSign },
        { "and", Kind.AndBoolSign },
        { "or", Kind.OrBoolSign },

        { "=", Kind.AssignmentSign },

        { "+", Kind.Addition },
        { "-", Kind.Subtraction },
        { "*", Kind.Multiplication },
        { "/", Kind.Division }
    };

    internal static List<CodeLine> CodeLines = null!;

    internal static readonly Token DefaultToken = new(Kind.DefaultType, null!);
    internal static readonly Token EofToken = new(Kind.Eof, EOF_CHAR);
    internal static readonly Token NewLineToken = new(Kind.NewLine, NEW_LINE_CHAR);
    internal static readonly Token WhiteSpaceToken = new(Kind.Whitespace, ' ');

    internal static Preprocessor Preprocessor = null!;
    internal static char StringCharacter;

    #endregion

    #region Other fields

    internal static int LinePosition;
    internal static int _position = -1;

    internal static int Position
    {
        get => _position;
        set
        {
            if (Code.Length <= value)
            {
                _position = -1;
                LinePosition++;
            }
            else
            {
                _position = value;
            }
        }
    }

    #endregion
}