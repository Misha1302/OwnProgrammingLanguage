using System.Diagnostics;
using Lexer.FrontEnd;
using RussianLanguage.Backend;

namespace RussianLanguage;

public static class Program
{
    private static string _code = string.Empty;

    private static void Main(string[] args)
    {
        MainInternal(args);
    }


    private static void MainInternal(IEnumerable<string> strings)
    {
        _code = File.ReadAllText(strings.FirstOrDefault() ?? "code.sil");

        var stopwatch = Stopwatch.StartNew();
        

        var tokens = GetTokens(_code);
        var ilCode = Collector.GetCode(tokens);
        var codeCompilationStatus = IlController.CompileCodeToIl(ilCode);


        stopwatch.Stop();
        Console.WriteLine($"compilation time: {stopwatch.ElapsedMilliseconds}ms");


        if (!codeCompilationStatus)
            ExceptionThrower.ThrowException(ExceptionType.CodeWithErrors, GetLanguage());

        IlController.StartIlCode();
    }

    private static bool CompileCode(string code)
    {
        var tokens = GetTokens(code);
        var ilCode = Collector.GetCode(tokens);
        var codeCompilationStatus = IlController.CompileCodeToIl(ilCode);
        return codeCompilationStatus;
    }

    private static Language GetLanguage()
    {
        var parameters = XmlReaderDictionary.GetXmlElements("src/xml/settings.xml");
        var languageParameter = parameters["lng"];

        var language = !Enum.TryParse(languageParameter, true, out Language lng) ? (Language?)null : lng;
        if (language == null) ExceptionThrower.ThrowException(ExceptionType.UnknownLanguage, null);

        return (Language)language!;
    }

    private static List<Token> GetTokens(in string code)
    {
        var lexer = new Lexer.Lexer.Lexer(code);
        var tokens = Lexer.Lexer.Lexer.GetTokens();
        return tokens;
    }
}