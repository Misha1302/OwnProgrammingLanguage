using System.Runtime.CompilerServices;
using RussianLanguage.Xml;

namespace RussianLanguage.Exceptions;

public static class ExceptionThrower
{
    private const int MILLISECONDS_TIMEOUT = 10 * 1000;
    private static readonly Dictionary<string, string> _exceptions;

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    static ExceptionThrower()
    {
        _exceptions = XmlReaderDictionary.GetXmlElements(@"src\xml\exceptions.xml");
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public static void ThrowException(ExceptionType exceptionType, Language? language)
    {
        Exception ex;
        if (language != null)
        {
            ex = new Exception(_exceptions[$"{exceptionType}.{language}"]);
        }
        else
        {
            var languagesThroughOr = string.Join(" or ", Enum.GetValues(typeof(Language)).Cast<Language>());
            ex = new Exception($"Unknown language, use some of this languages: \n{languagesThroughOr}");
        }


        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(ex.Message);

        Thread.Sleep(MILLISECONDS_TIMEOUT);
        throw ex;
    }
}