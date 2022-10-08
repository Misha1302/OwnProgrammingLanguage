namespace RussianLanguage.FrontEnd;

public struct CodeLine
{
    public readonly string Line;
    public readonly int Position;

    public CodeLine(string line, int position)
    {
        Line = line;
        Position = position;
    }
}