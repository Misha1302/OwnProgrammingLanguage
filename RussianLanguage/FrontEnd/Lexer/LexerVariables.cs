namespace RussianLanguage.FrontEnd.Lexer;

public partial class Lexer
{
    #region Properties

    private string Code => _linePosition < _codeLines.Count ? _codeLines[_linePosition].Line : "\0";

    #endregion

    #region Constants

    private const char COMMENT_CHAR = '#';
    private const char EOF_CHAR = '\0';
    private const char NEW_LINE_CHAR = '\n';

    #endregion

    #region Readonly fields

    private static readonly IReadOnlyDictionary<string, Kind> _words = new Dictionary<string, Kind>
    {
        { "from", Kind.From },
        { "bool", Kind.BoolType },
        { "string", Kind.StringType },
        { "int", Kind.IntType },
        { "float", Kind.FloatType },
        { "void", Kind.VoidType },
        { "call", Kind.Call },
        { "using", Kind.Using },
        // { "if", Kind.If },
        // { "else", Kind.Else }
    };

    private static readonly IReadOnlyDictionary<string, Kind> _other = new Dictionary<string, Kind>
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

    private readonly List<CodeLine> _codeLines;

    private readonly Token _defaultToken = new(Kind.DefaultType, null!);
    private readonly Token _eofToken = new(Kind.Eof, EOF_CHAR);
    private readonly Token _newLineToken = new(Kind.NewLine, NEW_LINE_CHAR);
    private readonly Token _whiteSpaceToken = new(Kind.Whitespace, ' ');

    private readonly Preprocessor _preprocessor;
    private readonly char _stringCharacter;

    #endregion

    #region Other fields

    private int _linePosition;

    private int _position = -1;

    private int Position
    {
        get => _position;
        set
        {
            if (Code.Length <= value)
            {
                _position = -1;
                _linePosition++;
            }
            else
            {
                _position = value;
            }
        }
    }

    #endregion
}