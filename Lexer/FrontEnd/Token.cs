using System.Runtime.CompilerServices;

namespace Lexer.FrontEnd;

public class Token
{
    public readonly object? Value;
    public bool IsPartOfExpression;
    public string Text;
    public Kind TokenKind;
    public DataType DataType;

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public Token(Kind tokenKind, string text, object? value = null, DataType type = DataType.@null,
        bool isPartOfExpression = false)
    {
        TokenKind = tokenKind;
        Text = text;
        DataType = type;
        Value = value;
        IsPartOfExpression = isPartOfExpression;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public Token(Kind tokenKind, char text, object? value = null, DataType type = DataType.@null,
        bool isPartOfExpression = false)
    {
        TokenKind = tokenKind;
        DataType = type;
        Text = text.ToString();
        Value = value;
        IsPartOfExpression = isPartOfExpression;
    }
}