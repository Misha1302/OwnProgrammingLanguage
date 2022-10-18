using System.Diagnostics;
using Lexer.FrontEnd;
using RussianLanguage.Backend;

namespace RussianLanguage;

public static class Program
{
    private static string _code = null!;

    private static void Main(string[] args)
    {
        MainInternal(args);
    }


    private static void MainInternal(IReadOnlyList<string> strings)
    {
        _code = File.ReadAllText(strings.Count > 0 ? strings[0] : "code.sil");

        var stopwatch = Stopwatch.StartNew();


        SetVariables(out var stringCharacter, out var language);

        var tokens = GetTokens(stringCharacter, _code);
        var ilCode = Collector.GetCode(tokens);
        var codeCompilationStatus = IlController.CompileCodeToIl(ilCode);


        stopwatch.Stop();
        Console.WriteLine(stopwatch.ElapsedMilliseconds);


        if (!codeCompilationStatus)
            ExceptionThrower.ThrowException(ExceptionType.CodeWithErrors, language);

        IlController.StartIlCode();
    }

    private static char GetStringCharacter(string stringCharacterParameter, Language? language)
    {
        if (language == null) ExceptionThrower.ThrowException(ExceptionType.UnknownLanguage, null);
        if (stringCharacterParameter.Length > 1)
            ExceptionThrower.ThrowException(ExceptionType.StringCharacterCannotBeString, language);

        return stringCharacterParameter[0];
    }

    private static void SetVariables(out char stringCharacter, out Language? language)
    {
        var parameters = XmlReaderDictionary.GetXmlElements("src/xml/settings.xml");
        var stringCharacterParameter = parameters["stringCharacter"];
        var languageParameter = parameters["lng"];

        language = !Enum.TryParse(languageParameter, true, out Language lng) ? null : lng;
        stringCharacter = GetStringCharacter(stringCharacterParameter, language);
    }

    private static List<Token> GetTokens(char stringCharacter, in string code)
    {
        var lexer = new Lexer.Lexer.Lexer(code, stringCharacter);
        var tokens = lexer.GetTokens();
        return tokens;
    }
}