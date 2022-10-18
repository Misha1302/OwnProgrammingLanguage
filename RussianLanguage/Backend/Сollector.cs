using System.Text;
using Lexer.FrontEnd;
using Lexer.Lexer;
using static RussianLanguage.Backend.CollectorConstants;

namespace RussianLanguage.Backend;

public static class Collector
{
    private static readonly StringBuilder _code = new();
    private static readonly List<Token> _mainLocalVariables = new();

    public static string GetCode(List<Token> tokens)
    {
        SetLocalVariables(tokens);

        return CreateCode(tokens);
    }

    private static void SetLocalVariables(IEnumerable<Token> tokens)
    {
        _mainLocalVariables.Clear();
        foreach (var t in tokens.Where(t => t.TokenKind == Kind.CreatedVariable))
            _mainLocalVariables.Add(t);
    }

    private static string CreateCode(IReadOnlyList<Token> tokens)
    {
        _code.Clear();

        AddAssemblies(tokens);

        _code.Append(StartOfCilCode);

        InitLocalVariables();

        _code.Append(")\n");

        AppendCodeFromMainMethod(tokens);

        _code.Append(EndOfCilMainMethod);
        _code.Append(CilMethodMainEnd);
        _code.Append("\n}");

        var q = _code.ToString();
        return q;
    }

    private static void AddAssemblies(IEnumerable<Token> tokens)
    {
        foreach (var t in tokens)
            if (t.TokenKind == Kind.Extern)
                _code.Append($"{t.Text}\n");
    }

#pragma warning disable CS8509 // The 'switch' expression does not handle all possible inputs (it is not exhaustive). For example, the pattern 'Kind.Eof' is not covered.
    private static void InitLocalVariables()
    {
        for (var i = 0; i < _mainLocalVariables.Count; i++)
        {
            var type = _mainLocalVariables[i].Type switch
            {
                DataType.@string => STRING_TYPE,
                DataType.int32 => INT_TYPE,
                DataType.float32 => FLOAT_TYPE,
                DataType.@bool => BOOLEAN_TYPE
            };
            _code.Append($"[{i}] {type} {_mainLocalVariables[i].Text}");
            if (i + 1 < _mainLocalVariables.Count) _code.Append(", ");
        }
    }

    // ReSharper disable SwitchStatementMissingSomeEnumCasesNoDefault
    private static void AppendCodeFromMainMethod(IReadOnlyList<Token> tokens)
    {
        var variableName = string.Empty;
        for (var i = 0; i < tokens.Count - 1; i++)
        {
            var token = tokens[i];
            if (token.IsPartOfExpression)
            {
                i = AddExpression(tokens, i);
                if (variableName != "")
                    _code.Append($"stloc.s {variableName}\n");
            }
            else
            {
                var nextToken = tokens[i + 1];
                switch (token.TokenKind)
                {
                    case Kind.AssignmentSign:
                        if (IsType(nextToken) || nextToken.IsPartOfExpression) i = AppendVariablesAssignment(tokens, i);
                        else variableName = tokens[i - 1].Text;
                        break;
                    case Kind.Call:
                        i = AppendMethodCall(tokens, i, variableName);
                        variableName = string.Empty;
                        break;
                }
            }
        }
    }

    private static int AddExpression(IReadOnlyList<Token> tokens, int i)
    {
        while (tokens[i].IsPartOfExpression)
        {
            var token = tokens[i];

            if (IsType(token)) _code.Append($"{GetPushCommand(token)} {token.Text}\n");
            else if (IsOperator(token)) _code.Append(GetMathCommand(tokens[i]) + "\n");
            else _code.Append($"ldloc.s {token.Text}\n");

            i++;
        }

        return i;
    }

    private static string GetMathCommand(Token token)
    {
        return token.TokenKind switch
        {
            Kind.Addition => "add.ovf",
            Kind.Subtraction => "sub.ovf",
            Kind.Multiplication => "mul.ovf",
            Kind.Division => "div.ovf",
            Kind.EqualsBoolSign => "ceq",
            Kind.NotEqualsBoolSign => "ceq\r\nnot",
            Kind.GreatThanBoolSign => "cgt",
            Kind.LessThanLessBoolSign => "clt",
            Kind.AndBoolSign => "and",
            Kind.OrBoolSign => "or"
        };
    }

    private static bool IsOperator(Token token)
    {
        return token.TokenKind is Kind.Addition or Kind.Subtraction or Kind.Multiplication or Kind.Division
            or Kind.EqualsBoolSign or Kind.NotEqualsBoolSign or Kind.GreatThanBoolSign or Kind.LessThanLessBoolSign
            or Kind.AndBoolSign or Kind.OrBoolSign;
    }

    private static bool IsBoolOperator(Token token)
    {
        return token.TokenKind is Kind.EqualsBoolSign or Kind.NotEqualsBoolSign or Kind.GreatThanBoolSign
            or Kind.LessThanLessBoolSign or Kind.AndBoolSign or Kind.OrBoolSign;
    }

    private static int AppendMethodCall(IReadOnlyList<Token> tokens, int i, string variableName = "")
    {
        var args = new List<Token>(4);
        var method = new List<Token>(4);

        Token? fromVariable = null;
        var dataType = DataType.@null;

        if (i - 2 >= 0)
        {
            if (tokens[i - 1].TokenKind == Kind.AssignmentSign)
                dataType = tokens[i - 2].Type;

            if (i - 4 >= 0 && tokens[i - 3].TokenKind == Kind.AssignmentSign)
                dataType = tokens[i - 4].Type;
            if (tokens[i - 2].TokenKind == Kind.From)
                fromVariable = tokens[i - 1];
        }

        i = GetMethodTokens(tokens, i, method);

        i = GetMethodArgs(tokens, i, args);

        if (fromVariable != null) _code.Append($"ldloca.s {fromVariable.Text}\n");

        PushArguments(args);

        var stringDataType = dataType == DataType.@null ? "void" : dataType.ToString();
        _code.Append(fromVariable == null
            ? $"call {stringDataType}"
            : $"call instance {stringDataType}");

        foreach (var m in method) _code.Append(m.Text);

        AppendTypesInMethod("(", args, ")\n");

        if (variableName != "") _code.Append($"stloc.s {variableName}\n");

        return i;
    }

    private static void AppendTypesInMethod(string start, List<Token> args, string end)
    {
        foreach (var expressionPosition in LexerFixTokens.GetExpressionsPositions(args))
        {
            var type = DataType.@null;
            var tempDebug = args.GetRange(expressionPosition.startPosition, expressionPosition.count);
            foreach (var arg in tempDebug)
                if (IsBoolOperator(arg) || arg.Type == DataType.@bool)
                {
                    type = DataType.@bool;
                    break;
                }
                else if (arg.Type == DataType.@string)
                {
                    type = DataType.@string;
                }
                else
                {
                    type = DataType.int32;
                }

            args.RemoveRange(expressionPosition.startPosition, expressionPosition.count);
            args.Insert(expressionPosition.startPosition, new Token(Kind.Unknown, "", type: type));
        }

        _code.Append(start);
        for (var index = 0; index < args.Count; index++)
        {
            _code.Append(args[index].Type);
            if (index + 1 < args.Count) _code.Append(", ");
        }

        _code.Append(end);
    }

    private static void PushArguments(IEnumerable<Token> args)
    {
        foreach (var pushString in from arg in args
                 let variable = _mainLocalVariables.FirstOrDefault(x => x.Text == arg.Text)
                 select PushCommand(variable, arg))
            _code.Append(pushString);
    }

    private static string PushCommand(Token? variable, Token arg)
    {
        if (variable != null) return $"ldloc.s {variable.Text}\n";

        return IsOperator(arg) ? $"{GetMathCommand(arg)}\n" : $"{GetPushCommand(arg)} {arg.Text}\n";
    }

    private static int GetMethodArgs(IReadOnlyList<Token> tokens, int i, ICollection<Token> args)
    {
        i++;
        while (tokens[i].TokenKind != Kind.CloseParenthesis)
        {
            args.Add(tokens[i]);
            i++;
        }

        return i;
    }

    private static int GetMethodTokens(IReadOnlyList<Token> tokens, int i, ICollection<Token> method)
    {
        i++;
        while (tokens[i].TokenKind != Kind.OpenParenthesis)
        {
            method.Add(tokens[i]);
            i++;
        }

        return i;
    }

    private static bool IsType(Token token)
    {
        return token.TokenKind is Kind.String or Kind.Float or Kind.Int or Kind.Void or Kind.Bool;
    }

    private static int AppendVariablesAssignment(IReadOnlyList<Token> tokens, int i)
    {
        var value = tokens[i + 1];
        var varName = tokens[i - 1].Text;

        if (value.IsPartOfExpression)
        {
            i = AddExpression(tokens, i + 1) - 1;
            value = null;
        }


        var var = _mainLocalVariables.First(x => x.Text == varName);

        var command = GetPushCommand(var);

        if (value != null) _code.Append($"{command} {value.Text.Replace('\'', '"')}\n");

        _code.Append($"stloc.s {varName}\n");

        return i;
    }

    private static string GetPushCommand(Token varType)
    {
        var command = varType.Type switch
        {
            DataType.int32 => PUSH_INT_CONSTANT,
            DataType.@bool => PUSH_INT_CONSTANT,
            DataType.float32 => PUSH_FLOAT_CONSTANT,
            DataType.@string => PUSH_STRING_CONSTANT
        };
        return command;
    }

#pragma warning restore CS8509
}