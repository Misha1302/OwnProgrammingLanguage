using System.Runtime.CompilerServices;

namespace Lexer.FrontEnd;

public struct CodeLine
{
    public readonly string Line;
    public readonly int Position;

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public CodeLine(string line, int position)
    {
        Line = line;
        Position = position;
    }
}