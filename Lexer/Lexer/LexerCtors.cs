using System.Runtime.CompilerServices;
using Lexer.FrontEnd;

namespace Lexer.Lexer;

public partial class Lexer
{
    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public Lexer(string code)
    {
        LexerVariables.CodeLines = Preprocessor.Preprocess(code);
        LexerVariables.StringCharacter = Preprocessor.STRING_CHARACTER_INTERNAL;
    }
}