using System.Diagnostics;
using System.Text;

namespace RussianLanguage.Backend;

public static class IlController
{
    private static readonly ProcessStartInfo _processStartInfo;
    private static readonly StringBuilder _stringBuilder = new(1024);

    static IlController()
    {
        _processStartInfo = new ProcessStartInfo
        {
            UseShellExecute = false,
            CreateNoWindow = false,
            RedirectStandardOutput = true,
            FileName = Directory.GetCurrentDirectory() + "\\src\\cmd\\compile.cmd"
        };
    }

    public static bool CompileCodeToIl(string code)
    {
        File.WriteAllText("cil\\Program.il", code);

        var proc = new Process();
        proc.StartInfo = _processStartInfo;
        proc.Start();

        _stringBuilder.Clear();
        while (!_stringBuilder.ToString().EndsWith("pause\n"))
            _stringBuilder.Append(proc.StandardOutput.ReadLine() + '\n');

        var str = _stringBuilder.ToString();
        proc.Kill();

        return !str.Contains("***** FAILURE *****");
    }

    public static void StartIlCode()
    {
        var processStartInfo = new ProcessStartInfo
        {
            UseShellExecute = false,
            FileName = "src\\cmd\\start.cmd"
        };

        var proc = new Process();
        proc.StartInfo = processStartInfo;
        proc.Start();
    }
}