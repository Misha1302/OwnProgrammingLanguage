using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace RussianLanguage.Backend;

public static class IlController
{
    private static readonly ProcessStartInfo _processStartInfo;
    private static readonly StringBuilder _stringBuilder = new(1024);

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    static IlController()
    {
        _processStartInfo = new ProcessStartInfo
        {
            UseShellExecute = false,
            CreateNoWindow = false,
            RedirectStandardOutput = true,
            FileName = $"{Directory.GetCurrentDirectory()}\\src\\cmd\\compile.cmd"
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public static bool CompileCodeToIl(string code)
    {
        File.WriteAllText(@"cil\Program.il", code);

        var proc = new Process();
        proc.StartInfo = _processStartInfo;
        proc.Start();

        _stringBuilder.Clear();
        while (!StringBuilderEndsWith(_stringBuilder, ">pause\n"))
            _stringBuilder.Append(proc.StandardOutput.ReadLine() + '\n');

        var str = _stringBuilder.ToString();
        proc.Kill();

        return !str.Contains("***** FAILURE *****");
    }

    private static bool StringBuilderEndsWith(StringBuilder stringBuilder, string str)
    {
        if (str.Length > stringBuilder.Length) return false;
        for (var i = str.Length - 1; i > 0; i--)
        {
            var ch = str[^i];
            if (stringBuilder[^i] != ch) return false;
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public static void StartIlCode()
    {
        var processStartInfo = new ProcessStartInfo
        {
            UseShellExecute = false,
            FileName = @"src\cmd\start.cmd"
        };

        var proc = new Process();
        proc.StartInfo = processStartInfo;
        proc.Start();
    }
}