using System.Runtime.CompilerServices;
using System.Text;
using Lexer.FrontEnd;
using Lexer.Lexer;
using static RussianLanguage.Backend.CollectorConstants;

namespace RussianLanguage.Backend;

public static class Collector
{
    private static int _conditionNumber;
    private static readonly StringBuilder _code = new();
    private static readonly List<Token> _mainLocalVariables = new();
    private static readonly List<Method> _methods;

    static Collector()
    {
        _methods = LexerFixTokens.Methods;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public static string GetCode(List<Token> tokens)
    {
        SetLocalVariables(tokens);

        return CreateCode(tokens);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static void SetLocalVariables(IEnumerable<Token> tokens)
    {
        _mainLocalVariables.Clear();
        foreach (var t in tokens.Where(t => t.TokenKind == Kind.CreatedVariable))
            _mainLocalVariables.Add(t);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static string CreateCode(List<Token> tokens)
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

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static void AddAssemblies(IEnumerable<Token> tokens)
    {
        foreach (var t in tokens)
            if (t.TokenKind == Kind.Extern)
                _code.Append($"{t.Text}\n");
    }

#pragma warning disable CS8509 // The 'switch' expression does not handle all possible inputs (it is not exhaustive). For example, the pattern 'Kind.Eof' is not covered.
    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static void InitLocalVariables()
    {
        for (var i = 0; i < _mainLocalVariables.Count; i++)
        {
            var type = _mainLocalVariables[i].DataType switch
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
    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static void AppendCodeFromMainMethod(List<Token> tokens)
    {
        var variableName = string.Empty;
        for (var i = 0; i < tokens.Count - 1; i++)
        {
            var token = tokens[i];
            if (token.IsPartOfExpression)
            {
                i = AddExpression(tokens, i);
                if (variableName != string.Empty)
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
                    case Kind.If:
                        i++;
                        i = AppendCondition(tokens, i);
                        break;
                }
            }
        }
    }

    private static int AppendCondition(List<Token> tokens, int i)
    {
        _conditionNumber++;
        i = AddExpression(tokens, i) + 1; // + 1 is OpenBracket
        _code.Append($"brfalse.s else{_conditionNumber}\n");

        var startI = i;
        var openBracketsCount = 1;

        while (openBracketsCount != 0)
            switch (tokens[++i].TokenKind)
            {
                case Kind.OpenBrace:
                    openBracketsCount++;
                    break;
                case Kind.CloseBrace:
                    openBracketsCount--;
                    break;
            }

        var count = i - startI;
        var internalTokens = tokens.GetRange(startI, count);

        AppendCodeFromMainMethod(internalTokens);

        _code.Append($"br.s out{_conditionNumber}\n");
        _code.Append($"else{_conditionNumber}:\n");
        i++;
        if (tokens[i].TokenKind == Kind.Else)
        {
            i++;
            startI = i;
            openBracketsCount = 1;

            while (openBracketsCount != 0)
                switch (tokens[++i].TokenKind)
                {
                    case Kind.OpenBrace:
                        openBracketsCount++;
                        break;
                    case Kind.CloseBrace:
                        openBracketsCount--;
                        break;
                }

            count = i - startI;
            internalTokens = tokens.GetRange(startI, count);
            AppendCodeFromMainMethod(internalTokens);
        }

        _code.Append($"out{_conditionNumber}:\n");
        return i;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static int AddExpression(IList<Token> tokens, int i)
    {
        var expressionType = GetTypeOfExpression(tokens, i);

        var isStringExpression = expressionType == ExpressionType.StringExpression;
        var isFloatExpression = expressionType == ExpressionType.FloatExpression;

        var stringAfterEachType = isFloatExpression ? "conv.r4\n" : string.Empty;

        while (tokens[i].IsPartOfExpression)
        {
            var token = tokens[i];
            if (IsType(token))
            {
                _code.Append(
                    $"{GetPushCommand(token)} {(token.TokenKind == Kind.String ? token.Text : token.Value)}\n");
                _code.Append(stringAfterEachType);
            }
            else if (IsOperator(token))
            {
                _code.Append($"{GetMathCommand(tokens[i], isStringExpression)}\n");
            }
            else
            {
                _code.Append($"ldloc.s {token.Text}\n");
            }


            i++;
        }

        return i;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static ExpressionType GetTypeOfExpression(IList<Token> tokens, int i)
    {
        var token = tokens[i];
        while (token.IsPartOfExpression)
        {
            if (token.TokenKind == Kind.Division || token.DataType == DataType.float32)
                return ExpressionType.FloatExpression;
            switch (token.DataType)
            {
                case DataType.@string:
                    return ExpressionType.StringExpression;
                case DataType.@bool:
                    return ExpressionType.BooleanExpression;
                default:
                    i++;
                    token = tokens[i];
                    break;
            }
        }

        return ExpressionType.IntExpression;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static string GetMathCommand(Token token, bool isStringExpression)
    {
        if (!isStringExpression)
            return token.TokenKind switch
            {
                Kind.Addition => "add.ovf",
                Kind.Subtraction => "sub.ovf",
                Kind.Multiplication => "mul.ovf",
                Kind.Division => "div",
                Kind.EqualsBoolSign => "ceq",
                Kind.NotEqualsBoolSign => $"ceq{Environment.NewLine}not",
                Kind.GreatThanBoolSign => "cgt",
                Kind.LessThanLessBoolSign => "clt",
                Kind.AndBoolSign => "and",
                Kind.OrBoolSign => "or"
            };

        return token.TokenKind switch
        {
            Kind.Addition => "call string [System.Runtime]System.String::Concat(string, string)",
            Kind.EqualsBoolSign => "call bool [System.Runtime]System.String::op_Equality(string, string)",
            Kind.NotEqualsBoolSign =>
                $"call bool [System.Runtime]System.String::op_Inequality(string, string){Environment.NewLine}"
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static bool IsOperator(Token token)
    {
        return token.TokenKind is Kind.Addition or Kind.Subtraction or Kind.Multiplication or Kind.Division
            or Kind.EqualsBoolSign or Kind.NotEqualsBoolSign or Kind.GreatThanBoolSign or Kind.LessThanLessBoolSign
            or Kind.AndBoolSign or Kind.OrBoolSign;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static bool IsBoolOperator(Token token)
    {
        return token.TokenKind is Kind.EqualsBoolSign or Kind.NotEqualsBoolSign or Kind.GreatThanBoolSign
            or Kind.LessThanLessBoolSign or Kind.AndBoolSign or Kind.OrBoolSign;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static int AppendMethodCall(IList<Token> tokens, int i, string variableName = "")
    {
        var args = new List<Token>(4);
        var method = new List<Token>(4);


        Token? fromVariable = null;
        if (i - 2 >= 0)
            if (tokens[i - 2].TokenKind == Kind.From)
                fromVariable = tokens[i - 1];


        i = GetMethodTokens(tokens, i, method);

        i = GetMethodArgs(tokens, i, args);


        if (fromVariable != null)
            _code.Append(fromVariable.DataType != DataType.@string
                ? $"ldloca.s {fromVariable.Text}\n"
                : $"ldloc.s {fromVariable.Text}\n");

        PushArguments(args);

        _code.Append("call ");

        var methodFullName = string.Join("", method.Select(x => x.Text));

        if (fromVariable != null) _code.Append("instance ");

        var methodName = methodFullName[(methodFullName.IndexOf(']') + 1)..];
        var dataType = _methods.First(x => x.MethodName == methodName).DataType;
        _code.Append($"{(dataType == DataType.@null ? "void" : dataType.ToString())} ");


        foreach (var m in method) _code.Append(m.Text);

        AppendTypesInMethod("(", args, ")\n");

        if (variableName != string.Empty)
            _code.Append($"stloc.s {variableName}\n");
        else if (dataType != DataType.@null) 
            _code.Append("pop\n");


        return i;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static void AppendTypesInMethod(string start, List<Token> args, string end)
    {
        foreach (var expressionPosition in LexerFixTokens.GetExpressionsPositions(args))
        {
            var type = DataType.@null;
            var tempDebug = args.GetRange(expressionPosition.startPosition, expressionPosition.count);
            foreach (var arg in tempDebug)
                if (IsBoolOperator(arg) || arg.DataType == DataType.@bool)
                {
                    type = DataType.@bool;
                    break;
                }
                else if (arg.DataType == DataType.@string)
                {
                    type = DataType.@string;
                }
                else
                {
                    type = DataType.int32;
                }

            args.RemoveRange(expressionPosition.startPosition, expressionPosition.count);
            args.Insert(expressionPosition.startPosition, new Token(Kind.Unknown, string.Empty, type: type));
        }

        _code.Append(start);
        for (var index = 0; index < args.Count; index++)
        {
            _code.Append(args[index].DataType);
            if (index + 1 < args.Count) _code.Append(", ");
        }

        _code.Append(end);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static void PushArguments(IReadOnlyCollection<Token> args)
    {
        var isStringExpression = args.Select(x => x.DataType).Contains(DataType.@string);
        foreach (var pushString in from arg in args
                 let variable = _mainLocalVariables.FirstOrDefault(x => x.Text == arg.Text)
                 select PushCommand(variable, arg, isStringExpression))
            _code.Append(pushString);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static string PushCommand(Token? variable, Token arg, bool isStringExpression)
    {
        if (variable != null) return $"ldloc.s {variable.Text}\n";

        return IsOperator(arg) ? $"{GetMathCommand(arg, isStringExpression)}\n" : $"{GetPushCommand(arg)} {arg.Text}\n";
    }

    private static int GetMethodArgs(IList<Token> tokens, int i, ICollection<Token> args)
    {
        i++;
        while (tokens[i].TokenKind != Kind.CloseParenthesis)
        {
            args.Add(tokens[i]);
            i++;
        }

        return i;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static int GetMethodTokens(IList<Token> tokens, int i, ICollection<Token> method)
    {
        i++;
        while (tokens[i].TokenKind != Kind.OpenParenthesis)
        {
            method.Add(tokens[i]);
            i++;
        }

        return i;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static bool IsType(Token token)
    {
        return token.TokenKind is Kind.String or Kind.Float or Kind.Int or Kind.Void or Kind.Bool;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static int AppendVariablesAssignment(IList<Token> tokens, int i)
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

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static string GetPushCommand(Token varType)
    {
        var command = varType.DataType switch
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