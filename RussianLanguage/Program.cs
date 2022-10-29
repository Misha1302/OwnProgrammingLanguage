using System.Diagnostics;
using System.Runtime.CompilerServices;
using RussianLanguage.Backend;
using RussianLanguage.Exceptions;
using RussianLanguage.Xml;

namespace RussianLanguage;

public static class Program
{
    private static string _code = string.Empty;

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static void Main(string[] args)
    {
        MainInternal(args);
    }


    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static void MainInternal(IEnumerable<string> strings)
    {
        _code = File.ReadAllText(strings.FirstOrDefault() ?? "code.sil");

        var stopwatch = Stopwatch.StartNew();

        var codeCompilationStatus = CompileCode(_code);

        stopwatch.Stop();
        Console.WriteLine($"Compilation time: {stopwatch.ElapsedMilliseconds} ms");


        if (!codeCompilationStatus)
            ExceptionThrower.ThrowException(ExceptionType.CodeWithErrors, GetLanguage());

        IlController.StartIlCode();
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static bool CompileCode(string code)
    {
        var tokens = CodeOptimizer.OptimizeTokens(Lexer.Lexer.Lexer.GetTokens(code));
        var ilCode = Collector.Collector.GetCode(tokens);
        var codeCompilationStatus = IlController.CompileCodeToIl(ilCode);
        return codeCompilationStatus;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static Language GetLanguage()
    {
        var parameters = XmlReaderDictionary.GetXmlElements("src/xml/settings.xml");
        var languageParameter = parameters["lng"];

        var language = !Enum.TryParse(languageParameter, true, out Language lng) ? (Language?)null : lng;
        if (language == null) ExceptionThrower.ThrowException(ExceptionType.UnknownLanguage, null);

        return (Language)language!;
    }
}