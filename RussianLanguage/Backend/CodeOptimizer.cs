using System.Globalization;
using System.Runtime.CompilerServices;
using Lexer.FrontEnd;
using Lexer.Lexer;

namespace RussianLanguage.Backend;

public static class CodeOptimizer
{
    // ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault
    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public static List<Token> OptimizeTokens(List<Token> tokens)
    {
        foreach (var expressionsPosition in LexerFixTokens.GetExpressionsPositions(tokens))
        {
            var expression = expressionsPosition;

            for (var i = expression.startPosition; i < expression.count + expression.startPosition; i++)
                checked
                {
                    var number0 = tokens[i];
                    var number1 = tokens[i + 1];
                    var sign = tokens[i + 2];

                    if (!Rpn.IsNumber(number0) || !Rpn.IsNumber(number1)) continue;

                    var outputKind = Kind.Unknown;
                    var outputDataType = DataType.@null;

                    outputKind = SetKindAndDataType(number0, number1, ref outputKind, ref outputDataType);

                    if (outputKind == Kind.Unknown) continue;
                    if (GetValue(sign, number0, number1, ref outputDataType, out var value)) continue;

                    tokens.RemoveRange(i, 3);

                    var text = value.ToString(CultureInfo.InvariantCulture).Replace(',', '.');
                    var token = new Token(outputKind, text, value, outputDataType, true);
                    tokens.Insert(i, token);
                    expression.count -= 2;
                }
        }

        return tokens;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static bool GetValue(Token sign, Token number0, Token number1, ref DataType outputDataType, out float value)
    {
        var isFloatExpression = outputDataType == DataType.float32;
        switch (sign.TokenKind)
        {
            case Kind.Addition:
                value = isFloatExpression
                    ? Convert.ToSingle(number0.Value) + Convert.ToSingle(number1.Value)
                    : Convert.ToInt32(number0.Value) + Convert.ToInt32(number1.Value);
                break;
            case Kind.Subtraction:
                value = isFloatExpression
                    ? Convert.ToSingle(number0.Value) - Convert.ToSingle(number1.Value)
                    : Convert.ToInt32(number0.Value) - Convert.ToInt32(number1.Value);
                break;
            case Kind.Multiplication:
                value = isFloatExpression
                    ? Convert.ToSingle(number0.Value) * Convert.ToSingle(number1.Value)
                    : Convert.ToInt32(number0.Value) * Convert.ToInt32(number1.Value);
                break;
            case Kind.Division:
                value = Convert.ToSingle(number0.Value) / Convert.ToSingle(number1.Value);
                outputDataType = DataType.float32;
                break;
            default:
                value = float.Epsilon;
                return true;
        }

        return false;
    }

    private static Kind SetKindAndDataType(Token number0, Token number1, ref Kind outputKind,
        ref DataType outputDataType)
    {
        switch (number0.TokenKind)
        {
            case Kind.Int when number1.TokenKind == Kind.Int:
            {
                outputKind = Kind.Int;
                outputDataType = DataType.int32;
                break;
            }
            case Kind.Float:
            {
                if (number1.TokenKind is Kind.Float or Kind.Int)
                {
                    outputKind = Kind.Float;
                    outputDataType = DataType.float32;
                }

                break;
            }
            default:
            {
                if (number1.TokenKind == Kind.Float && number0.TokenKind is Kind.Float or Kind.Int)
                {
                    outputKind = Kind.Float;
                    outputDataType = DataType.float32;
                }

                break;
            }
        }

        return outputKind;
    }
}