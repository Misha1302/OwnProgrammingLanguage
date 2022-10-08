namespace RussianLanguage.FrontEnd;

public enum Kind
{
    Eof,
    Whitespace,
    Define,
    Unknown,
    NewLine,
    OpenParenthesis,
    CloseParenthesis,
    DefaultType,
    Dot,
    Comma,
    EqualsSign,
    Variable,
    CreatedVariable,
    Call,
    Void,
    From,
    OpenBracket,
    CloseBracket,
    Division,
    Multiplication,
    Subtraction,
    Addition,
    
    Int,
    Float,
    Bool,
    String,
    
    IntType,
    FloatType,
    VoidType,
    StringType,
    BoolType
}