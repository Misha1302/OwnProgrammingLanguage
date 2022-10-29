namespace Collector;

public static class CollectorVariables
{
    public const string PUSH_INT_CONSTANT = "ldc.i4";
    public const string PUSH_FLOAT_CONSTANT = "ldc.r4";
    public const string PUSH_STRING_CONSTANT = "ldstr";
    
    public const string STRING_TYPE = "string";
    public const string INT_TYPE = "int32";
    public const string FLOAT_TYPE = "float32";
    public const string BOOLEAN_TYPE = "bool";

    public static string StartOfCilCode => File.ReadAllText(@"src\ilCode\0.il");
    public static string EndOfCilMainMethod => File.ReadAllText(@"src\ilCode\1.il");
    public static string CilMethodMainEnd => File.ReadAllText(@"src\ilCode\2.il");
}