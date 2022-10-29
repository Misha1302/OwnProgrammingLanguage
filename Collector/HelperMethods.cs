using System.Runtime.CompilerServices;
using System.Text;
using Lexer.FrontEnd;

namespace Collector;

internal static class HelperMethods
{
    private static readonly List<Token> _mainLocalVariables = new();
    private static int _conditionNumber;

#pragma warning disable CS8509 // The 'switch' expression does not handle all possible inputs (it is not exhaustive). For example, the pattern 'Kind.Eof' is not covered.
    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    internal static List<Token> SetLocalVariables(IEnumerable<Token> tokens)
    {
        _mainLocalVariables.Clear();
        foreach (var t in tokens.Where(t => t.TokenKind == Kind.CreatedVariable))
            _mainLocalVariables.Add(t);

        return _mainLocalVariables;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    internal static void AddAssemblies(IEnumerable<Token> tokens, StringBuilder code)
    {
        foreach (var t in tokens)
            if (t.TokenKind == Kind.Extern)
                code.Append($"{t.Text}\n");
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    internal static void InitLocalVariables(StringBuilder code)
    {
        code.Append(CollectorVariables.StartOfCilCode);

        for (var i = 0; i < _mainLocalVariables.Count; i++)
        {
            var type = _mainLocalVariables[i].DataType switch
            {
                DataType.@string => CollectorVariables.STRING_TYPE,
                DataType.int32 => CollectorVariables.INT_TYPE,
                DataType.float32 => CollectorVariables.FLOAT_TYPE,
                DataType.@bool => CollectorVariables.BOOLEAN_TYPE
            };
            code.Append($"[{i}] {type} {_mainLocalVariables[i].Text}");
            if (i + 1 < _mainLocalVariables.Count) code.Append(", ");
        }

        code.Append(")\n");
    }

    internal static int AppendCondition(List<Token> tokens, int i, StringBuilder code)
    {
        _conditionNumber++;
        i = Collector.AddExpression(tokens, i) + 1;
        code.Append($"brfalse.s else{_conditionNumber}\n\n");

        i = AppendIfBlock(tokens, i);

        code.Append($"br.s out{_conditionNumber}\n\n");
        code.Append($"else{_conditionNumber}:\n\n");

        i = AppendElseBlock(tokens, i + 1);

        code.Append($"out{_conditionNumber}:\n\n");
        _conditionNumber--;
        return i;
    }

    private static int AppendIfBlock(List<Token> tokens, int i)
    {
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

        Collector.AppendCode(internalTokens);

        return i;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static int AppendElseBlock(List<Token> tokens, int i)
    {
        if (tokens[i].TokenKind != Kind.Else) return i;

        i++;
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
        Collector.AppendCode(internalTokens);

        return i;
    }


    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    internal static string GetPushCommand(Token varType)
    {
        var command = varType.DataType switch
        {
            DataType.int32 => CollectorVariables.PUSH_INT_CONSTANT,
            DataType.@bool => CollectorVariables.PUSH_INT_CONSTANT,
            DataType.float32 => CollectorVariables.PUSH_FLOAT_CONSTANT,
            DataType.@string => CollectorVariables.PUSH_STRING_CONSTANT
        };
        return command;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    internal static bool IsType(Token token)
    {
        return token.TokenKind is Kind.String or Kind.Float or Kind.Int or Kind.Void or Kind.Bool;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    internal static int GetMethodTokens(IList<Token> tokens, int i, ICollection<Token> method)
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
    internal static int GetMethodArgs(IList<Token> tokens, int i, ICollection<Token> args)
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
    internal static bool IsOperator(Token token)
    {
        return token.TokenKind is Kind.Addition or Kind.Subtraction or Kind.Multiplication or Kind.Division
            or Kind.EqualsBoolSign or Kind.NotEqualsBoolSign or Kind.GreatThanBoolSign or Kind.LessThanLessBoolSign
            or Kind.AndBoolSign or Kind.OrBoolSign;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    internal static bool IsBoolOperator(Token token)
    {
        return token.TokenKind is Kind.EqualsBoolSign or Kind.NotEqualsBoolSign or Kind.GreatThanBoolSign
            or Kind.LessThanLessBoolSign or Kind.AndBoolSign or Kind.OrBoolSign;
    }


    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    internal static string GetMathCommand(Token token, bool isStringExpression)
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
    private static string PushCommand(Token? variable, Token arg, bool isStringExpression)
    {
        if (variable != null) return $"ldloc.s {variable.Text}\n";

        return IsOperator(arg) ? $"{GetMathCommand(arg, isStringExpression)}\n" : $"{GetPushCommand(arg)} {arg.Text}\n";
    }


    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    internal static void PushArguments(IReadOnlyCollection<Token> args, StringBuilder code)
    {
        var isStringExpression = args.Select(x => x.DataType).Contains(DataType.@string);
        foreach (var pushString in from arg in args
                 let variable = _mainLocalVariables.FirstOrDefault(x => x.Text == arg.Text)
                 select PushCommand(variable, arg, isStringExpression))
            code.Append(pushString);
    }

#pragma warning restore CS8509
}