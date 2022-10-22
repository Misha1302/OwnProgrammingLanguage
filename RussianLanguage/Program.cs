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


        SetVariables(out var language);

        var tokens = GetTokens(_code);
        var ilCode = Collector.GetCode(tokens);
        var codeCompilationStatus = IlController.CompileCodeToIl(ilCode);


        stopwatch.Stop();
        Console.WriteLine($"compilation time: {stopwatch.ElapsedMilliseconds}ms");


        if (!codeCompilationStatus)
            ExceptionThrower.ThrowException(ExceptionType.CodeWithErrors, language);

        IlController.StartIlCode();
    }

    private static void SetVariables(out Language? language)
    {
        var parameters = XmlReaderDictionary.GetXmlElements("src/xml/settings.xml");
        var languageParameter = parameters["lng"];

        language = !Enum.TryParse(languageParameter, true, out Language lng) ? null : lng;
        if (language == null) ExceptionThrower.ThrowException(ExceptionType.UnknownLanguage, null);
    }

    private static List<Token> GetTokens(in string code)
    {
        var lexer = new Lexer.Lexer.Lexer(code);
        var tokens = Lexer.Lexer.Lexer.GetTokens();
        return tokens;
    }
}