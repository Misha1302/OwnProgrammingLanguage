using System.Runtime.CompilerServices;

namespace RussianLanguage.FrontEnd.Lexer;

public partial class Lexer
{
    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public Lexer(string code, char stringCharacter)
    {
        _preprocessor = new Preprocessor(stringCharacter);
        _codeLines = _preprocessor.Preprocess(code);
        _stringCharacter = Preprocessor.STRING_CHARACTER_INTERNAL;
    }
}