using System.Diagnostics;
using RussianLanguage.Backend;
using RussianLanguage.FrontEnd;
using RussianLanguage.FrontEnd.Lexer;

namespace RussianLanguage;

public static class Program
{
    private static readonly string _code;

    static Program()
    {
        _code = File.ReadAllText("code.sil");
    }

    private static void Main()
    {
        MainInternal();
    }

    private static void MainInternal()
    {
        var stopwatch = Stopwatch.StartNew();


        SetVariables(out var stringCharacter, out var language);

        var tokens = GetTokens(stringCharacter);
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

    private static List<Token> GetTokens(char stringCharacter)
    {
        var lexer = new Lexer(_code, stringCharacter);
        var tokens = lexer.GetTokens();
        return tokens;
    }
}