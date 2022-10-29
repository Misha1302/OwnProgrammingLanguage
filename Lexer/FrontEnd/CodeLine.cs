using System.Runtime.CompilerServices;

namespace Lexer.FrontEnd;

public struct CodeLine
{
    public readonly string Line;
    // we will need this field soon
    // ReSharper disable once MemberCanBePrivate.Global
    public readonly int Position;

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public CodeLine(string line, int position)
    {
        Line = line;
        Position = position;
    }
}