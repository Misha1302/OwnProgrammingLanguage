using Lexer.FrontEnd;

namespace Lexer.Lexer;

public struct Method
{
    public string MethodName;
    public DataType DataType;

    public Method(string methodName, DataType dataType)
    {
        MethodName = methodName;
        DataType = dataType;
    }
}