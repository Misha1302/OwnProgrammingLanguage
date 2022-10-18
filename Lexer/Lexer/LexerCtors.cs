using System.Runtime.CompilerServices;
using Lexer.FrontEnd;

namespace Lexer.Lexer;

public partial class Lexer
{
    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public Lexer(string code, char stringCharacter)
    {
        LexerVariables.Preprocessor = new Preprocessor(stringCharacter);
        LexerVariables.CodeLines = LexerVariables.Preprocessor.Preprocess(code);
        LexerVariables.StringCharacter = Preprocessor.STRING_CHARACTER_INTERNAL;
    }
}