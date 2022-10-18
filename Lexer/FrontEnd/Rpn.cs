namespace Lexer.FrontEnd;

public static class Rpn
{
    public static IEnumerable<Token> GetReversePolishNotation(List<Token> tokens)
    {
        var operationsStack = new Stack<Token>();
        var result = new List<Token>();

        foreach (var token in tokens)
        {
            if (IsNumber(token))
            {
                result.Add(token);
                continue;
            }

            if (IsOperator(token))
            {
                Token lastOperation;
                if (operationsStack.Count != 0)
                {
                    lastOperation = operationsStack.Peek();
                }
                else
                {
                    operationsStack.Push(token);
                    continue;
                }

                if (GetPriority(lastOperation) > GetPriority(token))
                {
                    operationsStack.Push(token);
                    continue;
                }

                result.Add(operationsStack.Pop());
                operationsStack.Push(token);
                continue;
            }

            switch (token.TokenKind)
            {
                case Kind.OpenParenthesis:
                    operationsStack.Push(token);
                    continue;
                case Kind.CloseParenthesis:
                    while (operationsStack.Peek().TokenKind != Kind.OpenParenthesis)
                        result.Add(operationsStack.Pop());
                    operationsStack.Pop();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(token.Text);
            }
        }

        while (operationsStack.Count != 0) result.Add(operationsStack.Pop());

        return result;
    }

    private static bool IsOperator(Token token)
    {
        return token.TokenKind is Kind.Addition or Kind.Subtraction or Kind.Multiplication or Kind.Division
            or Kind.EqualsBoolSign or Kind.NotEqualsBoolSign or Kind.GreatThanBoolSign or Kind.LessThanLessBoolSign
            or Kind.AndBoolSign or Kind.OrBoolSign;
    }

    private static bool IsNumber(Token token)
    {
        return token.Type is DataType.int32 or DataType.float32 or DataType.@bool or DataType.@string;
    }

    private static int GetPriority(Token token)
    {
        return token.TokenKind switch
        {
            Kind.OpenParenthesis => int.MaxValue,
            Kind.CloseParenthesis => int.MaxValue,


            Kind.Float => 2,
            Kind.Int => 2,


            Kind.Addition => 1,
            Kind.Subtraction => 1,

            Kind.EqualsBoolSign => 1,
            Kind.NotEqualsBoolSign => 1,
            Kind.LessThanLessBoolSign => 1,
            Kind.GreatThanBoolSign => 1,
            Kind.OrBoolSign => 1,


            Kind.Multiplication => 0,
            Kind.Division => 0,

            Kind.AndBoolSign => 1,

            _ => int.MinValue
        };
    }
}