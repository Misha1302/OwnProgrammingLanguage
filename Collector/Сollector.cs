using System.Runtime.CompilerServices;
using System.Text;
using Lexer.FrontEnd;
using Lexer.Lexer;
using static Collector.CollectorVariables;
using static Collector.HelperMethods;

namespace Collector;

public static class Collector
{
    private static readonly StringBuilder _code;
    private static List<Token> _mainLocalVariables = new();
    private static readonly List<Method> _methods;

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    static Collector()
    {
        _methods = LexerFixTokens.Methods;
        _code = new StringBuilder(4096);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public static string GetCode(List<Token> tokens)
    {
        _mainLocalVariables = SetLocalVariables(tokens);

        return CreateCode(tokens);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static string CreateCode(List<Token> tokens)
    {
        _code.Clear();

        AddAssemblies(tokens, _code);

        InitLocalVariables(_code);

        AppendCode(tokens);

        AppendEndOfCode();

        return _code.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static void AppendEndOfCode()
    {
        _code.Append(EndOfCilMainMethod);
        _code.Append(CilMethodMainEnd);
        _code.Append("\n}\n");
    }
    
    
#pragma warning disable CS8509 // The 'switch' expression does not handle all possible inputs (it is not exhaustive). For example, the pattern 'Kind.Eof' is not covered.
    // ReSharper disable SwitchStatementMissingSomeEnumCasesNoDefault
    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    internal static void AppendCode(List<Token> tokens)
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
                        i = AppendCondition(tokens, i, _code);
                        break;
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    internal static int AddExpression(IList<Token> tokens, int i)
    {
        var expressionType = GetTypeOfExpression(tokens, i);

        var isStringExpression = expressionType == ExpressionType.StringExpression;
        var isFloatExpression = expressionType == ExpressionType.FloatExpression;

        var stringAfterEachType = isFloatExpression ? "conv.r4\n" : string.Empty;

        while (tokens[i].IsPartOfExpression)
        {
            var token = tokens[i];
            
            if (IsType(token))
                _code.Append($"{GetPushCommand(token)} {token.Text}\n{stringAfterEachType}");
            else if (IsOperator(token))
                _code.Append($"{GetMathCommand(tokens[i], isStringExpression)}\n");
            else
                _code.Append($"ldloc.s {token.Text}\n");
            
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

        PushArguments(args, _code);

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
}