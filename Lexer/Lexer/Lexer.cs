using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Lexer.FrontEnd;
using static Lexer.Lexer.LexerVariables;

namespace Lexer.Lexer;

public class Lexer
{
    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static void Init(string code)
    {
        CodeLines = Preprocessor.Preprocess(code);
        StringCharacter = Preprocessor.STRING_CHARACTER_INTERNAL;
    }

    /// <summary>
    ///     Gets all tokens from code<br />
    ///     Eof - end of file
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public static List<Token> GetTokens(string code)
    {
        Init(code);

        var token = DefaultToken;
        var tokens = new List<Token>();

        while (token.TokenKind != Kind.Eof)
        {
            token = GetNextToken();
            tokens.Add(token);
        }

        tokens = Preprocessor.PreprocessTokens(tokens);

        ConnectUnknownTokens(tokens);

        LexerFixTokens.FixTokens(tokens);

        return tokens;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static void ConnectUnknownTokens(IList<Token> tokens)
    {
        for (var i = 0; i < tokens.Count; i++)
        {
            var startPosition = i;
            if (tokens[i].TokenKind != Kind.Unknown) continue;

            var startTokenText = new StringBuilder(tokens[startPosition].Text, 16);
            i++;
            while (tokens[i].TokenKind == Kind.Unknown)
            {
                startTokenText.Append(tokens[i].Text);
                tokens.RemoveAt(i);
            }

            tokens[startPosition].Text = startTokenText.ToString();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static Token GetNextToken()
    {
        while (true)
        {
            if (Code.Length <= Position + 1)
            {
                LinePosition++;
                Position = 0;
            }
            else
            {
                Position++;
            }

            if (Code[Position] == EOF_CHAR) return EofToken;


            var currentChar = Code[Position];

            if (currentChar == NEW_LINE_CHAR) return NewLineToken;
            if (currentChar == StringCharacter && Position > 0 && Code[Position - 1] != '\\') return GetString();
            if (char.IsWhiteSpace(currentChar)) return WhiteSpaceToken;


            if (currentChar == COMMENT_CHAR)
            {
                while (Code[Position] != NEW_LINE_CHAR)
                {
                    Position++;
                    if (Code[Position] == EOF_CHAR) return EofToken;
                }

                Position--;
                continue;
            }

            var isNegativeNumber = currentChar == '(' && Code[Position + 1] == '-'; // example: (-6)
            if (char.IsNumber(currentChar) || isNegativeNumber)
                return GetNextNumberToken(isNegativeNumber);

            foreach (var symbol in Other)
                if (Code[Position..].StartsWith(symbol.Key))
                {
                    Position += symbol.Key.Length - 1;
                    return new Token(symbol.Value, symbol.Key);
                }

            return GetCommand(out var token) ? token : new Token(Kind.Unknown, currentChar);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static Token GetString()
    {
        Position++;

        var pattern = $"(?<!(\\\\)){StringCharacter}";
        var endIndex = Regex.Match(Code[(Position + 1)..], pattern).Index + Position + 1;
        var value = Code[Position..endIndex];

        Position += value.Length;
        return new Token(Kind.String, $"{StringCharacter}{value}{StringCharacter}", value, DataType.@string);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static bool GetCommand(out Token token)
    {
        foreach (var commandPair in Words.Where(commandPair =>
                     Code[Position..].IndexOf(commandPair.Key, StringComparison.Ordinal) == 0))
        {
            if (Code.Length > Position + commandPair.Key.Length + 1)
            {
                if (Regex.IsMatch(Code[Position + commandPair.Key.Length].ToString(), "[a-zA-Z]")) continue;

                Position += commandPair.Key.Length - 1;

                token = new Token(commandPair.Value, commandPair.Key);
                return true;
            }

            Position += commandPair.Key.Length - 1;

            token = new Token(commandPair.Value, commandPair.Key);
            return true;
        }

        token = null!;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static Token GetNextNumberToken(bool isNegativeNumber)
    {
        Token value;

        if (isNegativeNumber) Position++;
        switch (Code[Position + 1])
        {
            case 'x':
                Position += 2;
                value = GetNextHexToken();
                if (isNegativeNumber) Position++;
                return value;
            case 'b':
                Position += 2;
                value = GetNextBinaryToken();
                if (isNegativeNumber) Position++;
                return value;
            default:
                value = GetNextDecimalToken();
                if (isNegativeNumber) Position++;
                return value;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static Token GetNextHexToken()
    {
        const int numberBase = 16;
        var validCharacters = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };
        return GetNextNumberToken(numberBase, validCharacters);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static Token GetNextBinaryToken()
    {
        const int numberBase = 2;
        var validCharacters = new[] { '0', '1' };
        return GetNextNumberToken(numberBase, validCharacters);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static Token GetNextNumberToken(int numberBase, char[] validCharacters)
    {
        var number = new StringBuilder();
        var ch = Code[Position];
        var isInt = true;

        do
        {
            number.Append(ch);
            if (ch == '.') isInt = false;
            Position++;
            ch = Code[Position];
        } while (Code.Length > Position && validCharacters.Contains(ch));

        Position--;

        var numberStr = number.ToString();
        return isInt
            ? new Token(Kind.Int, numberStr, Convert.ToInt32(numberStr, numberBase), DataType.int32)
            : new Token(Kind.Float, numberStr, Convert.ToSingle(numberStr), DataType.float32);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static Token GetNextDecimalToken()
    {
        const int numberBase = 10;
        var validCharacters = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.' };
        return GetNextNumberToken(numberBase, validCharacters);
    }
}