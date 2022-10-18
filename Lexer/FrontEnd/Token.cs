using System.Runtime.CompilerServices;

namespace Lexer.FrontEnd;

public class Token
{
    public string Text;
    public Kind TokenKind;
    public readonly object? Value;
    public DataType Type;
    public bool IsPartOfExpression;

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public Token(Kind tokenKind, string text, object? value = null, DataType type = DataType.@null, bool isPartOfExpression = false)
    {
        TokenKind = tokenKind;
        Text = text;
        Type = type;
        Value = value;
        IsPartOfExpression = isPartOfExpression;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public Token(Kind tokenKind, char text, object? value = null, DataType type = DataType.@null, bool isPartOfExpression = false)
    {
        TokenKind = tokenKind;
        Type = type;
        Text = text.ToString();
        Value = value;
        IsPartOfExpression = isPartOfExpression;
    }
}