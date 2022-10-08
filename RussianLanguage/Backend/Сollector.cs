using System.Text;
using RussianLanguage.FrontEnd;
using static RussianLanguage.Backend.CollectorConstants;

namespace RussianLanguage.Backend;

public static class Collector
{
    private static readonly StringBuilder _code = new();
    private static readonly List<Token> _mainLocalVariables = new();
    private static readonly List<Library> _libraries = new();

    public static string GetCode(List<Token> tokens)
    {
        AddLibraries();
        SetLocalVariables(tokens);

        return CreateCode(tokens);
    }

    private static void SetLocalVariables(IEnumerable<Token> tokens)
    {
        _mainLocalVariables.Clear();
        foreach (var t in tokens.Where(t => t.TokenKind == Kind.CreatedVariable))
            _mainLocalVariables.Add(t);
    }

    private static void AddLibraries()
    {
        _libraries.Clear();
        _libraries.Add(new Library("System.Runtime", "6:0:0:0"));
        _libraries.Add(new Library("System.Console", "6:0:0:0"));
    }

    private static string CreateCode(IReadOnlyList<Token> tokens)
    {
        _code.Clear();
        ConnectLibraries();

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

    private static void ConnectLibraries()
    {
        foreach (var library in _libraries)
            _code.Append($".assembly extern {library.FullName} {{ .ver {library.Version} }} \n");
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
        for (var i = 1; i < tokens.Count - 1; i++)
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
                    case Kind.EqualsSign:
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

            if (IsType(token)) _code.Append($"{GetPushCommand(token.Type)} {token.Text}\n");
            else if (IsMathSign(token)) _code.Append(GetMathCommand(tokens[i]) + "\n");
            else _code.Append($"ldloc.s {token.Text}\n");

            i++;
        }

        return i;
    }

    private static string GetMathCommand(Token token)
    {
        return token.TokenKind switch
        {
            Kind.Addition => "add",
            Kind.Subtraction => "sub",
            Kind.Multiplication => "mul",
            _ => "div"
        };
    }

    private static bool IsMathSign(Token token)
    {
        return token.TokenKind is Kind.Addition or Kind.Subtraction or Kind.Multiplication or Kind.Division;
    }

    private static int AppendMethodCall(IReadOnlyList<Token> tokens, int i, string variableName = "")
    {
        var args = new List<Token>(4);
        var method = new List<Token>(4);

        Token? fromVariable = null;
        var dataType = DataType.@null;

        if (i - 2 >= 0)
        {
            if (tokens[i - 1].TokenKind == Kind.EqualsSign)
                dataType = tokens[i - 2].Type;
            if (i - 4 >= 0 && tokens[i - 3].TokenKind == Kind.EqualsSign)
                dataType = tokens[i - 4].Type;

            if (tokens[i - 2].TokenKind == Kind.From)
                fromVariable = tokens[i - 1];
        }

        i = GetMethodTokens(tokens, i, method);

        _ = GetMethodArgs(tokens, i, args);

        if (fromVariable != null) _code.Append($"ldloc.s {fromVariable.Text}\n");

        PushArguments(args);

        var stringDataType = dataType == DataType.@null ? "void" : dataType.ToString();
        _code.Append(fromVariable == null
            ? $"call {stringDataType} [mscorlib]"
            : $"callvirt instance {stringDataType} [mscorlib]");

        foreach (var m in method) _code.Append(m.Text);

        AppendTypesInMethod("(", args, ")\n");

        if (variableName != "") _code.Append($"stloc.s {variableName}\n");

        return i;
    }

    private static void AppendTypesInMethod(string start, IReadOnlyList<Token> args, string end)
    {
        _code.Append(start);
        for (var index = 0; index < args.Count; index++)
        {
            _code.Append(args[index].Type);
            if (index + 1 < args.Count) _code.Append(", ");
        }

        _code.Append(end);
    }

    private static void PushArguments(List<Token> args)
    {
        foreach (var arg in args)
        {
            var variable = _mainLocalVariables.FirstOrDefault(x => x.Text == arg.Text);
            _code.Append(variable != null ? $"ldloc.s {variable.Text}\n" : $"{GetPushCommand(arg.Type)} {arg.Text}\n");
        }
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
        var varType = var.Type;

        var command = GetPushCommand(varType);

        if (value != null) _code.Append($"{command} {value.Text.Replace('\'', '"')}\n");

        _code.Append($"stloc.s {varName}\n");

        return i;
    }

    private static string GetPushCommand(DataType varType)
    {
        var command = varType switch
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