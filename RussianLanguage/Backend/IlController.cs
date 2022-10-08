using System.Diagnostics;
using System.Text;

namespace RussianLanguage.Backend;

public static class IlController
{
    public static bool CompileCodeToIl(string code)
    {
        File.WriteAllText("cil\\Program.il", code);

        var processStartInfo = new ProcessStartInfo
        {
            UseShellExecute = false,
            CreateNoWindow = false,
            RedirectStandardOutput = true,
            FileName = Directory.GetCurrentDirectory() + "\\src\\cmd\\compile.cmd"
        };
        var proc = new Process();
        proc.StartInfo = processStartInfo;
        proc.Start();

        Thread.Sleep(100);

        var stringBuilder = new StringBuilder(512);
        while (!stringBuilder.ToString().Contains("pause"))
            stringBuilder.Append(proc.StandardOutput.ReadLine() + '\n');

        var str = stringBuilder.ToString();
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