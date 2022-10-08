namespace RussianLanguage.FrontEnd.Lexer;

public static class LexerFixTokens
{
    public static void FixTokens(List<Token> tokens)
    {
        SetVariables(tokens);
        SetExpressions(tokens);
    }

    private static void SetExpressions(IList<Token> tokens)
    {
        for (var i = 0; i < tokens.Count; i++)
            if (tokens[i].TokenKind == Kind.OpenBracket)
            {
                tokens.RemoveAt(i);
                while (tokens[i].TokenKind != Kind.CloseBracket)
                {
                    tokens[i].IsPartOfExpression = true;
                    i++;
                }

                tokens.RemoveAt(i);
            }
    }

#pragma warning disable CS8509
    private static void SetVariables(IReadOnlyList<Token> tokens)
    {
        var variables = new Dictionary<string, DataType>();
        for (var i = 0; i < tokens.Count - 2; i++)
            if (IsType(tokens[i]) && tokens[i + 2].TokenKind == Kind.EqualsSign)
            {
                tokens[i + 1].TokenKind = Kind.CreatedVariable;
                tokens[i + 1].Type = tokens[i].TokenKind switch
                {
                    Kind.StringType => DataType.@string,
                    Kind.IntType => DataType.int32,
                    Kind.FloatType => DataType.float32,
                    Kind.BoolType => DataType.@bool
                };

                variables.Add(tokens[i + 1].Text, tokens[i + 1].Type);
                i++;
            }


        for (var index = 1; index < tokens.Count; index++)
            if (tokens[index].TokenKind != Kind.CreatedVariable && variables.ContainsKey(tokens[index].Text))
            {
                tokens[index].TokenKind = Kind.Variable;
                tokens[index].Type = variables[tokens[index].Text];
            }
    }
#pragma warning restore CS8509

    private static bool IsType(Token token)
    {
        return token.TokenKind is Kind.StringType or Kind.FloatType or Kind.IntType or Kind.BoolType;
    }
}