using System.Diagnostics;
using System.Text;

namespace MalsMerger.Core.Helpers;

public enum LogLevel
{
    None, OK, Info, Warning, Error
}

public static class ConsoleHelper
{
    private const string Padding = "  ";
    private const string TriforceASCII = $"""
        {Padding}*       /\\
        {Padding}*      /  \\
        {Padding}*     /____\\
        {Padding}*    /\\   /\\
        {Padding}*   /  \\ /  \\
        {Padding}*  /____\/____\\
        """;

    public static bool Verbose { get; set; } = false;
    public static StreamWriter? LogFile { get; set; } = null;

    public static void PrintTriforce(string title, ConsoleColor color = ConsoleColor.Blue)
    {
        StringBuilder sb = new("\n");

        double width = 17;
        int len = title.Length + 8;

        if (len == width) {
            string line = Padding + new string('_', len);
            sb.AppendLine(TriforceASCII.Replace("*", string.Empty));
            sb.AppendLine(line);
            sb.Append($"\n{Padding}  - ");
            sb.Append(title);
            sb.Append(" -\n");
            sb.AppendLine(line);
        }
        else if (len > width) {
            int padding = (int)Math.Floor((len - width) / 2);
            string line = Padding + new string('_', len);
            sb.AppendLine(TriforceASCII.Replace("*", new string(' ', padding)));
            sb.AppendLine(line);
            sb.Append($"\n{Padding}  - ");
            sb.Append(title);
            sb.Append(" -\n");
            sb.AppendLine(line);
        }
        else {
            int padding = (int)Math.Ceiling(-(len - width) / 2);
            string line = Padding + new string('_', len + (padding * 2));
            sb.AppendLine(TriforceASCII.Replace("*", string.Empty));
            sb.AppendLine(line);
            sb.Append($"\n{Padding}  {new string(' ', padding)}- ");
            sb.Append(title);
            sb.Append(" -\n");
            sb.AppendLine(line);
        }

        Console.ForegroundColor = color;
        Console.WriteLine(sb);
        Console.ResetColor();
    }

    public static void CreateLogFile(string filename)
    {
        FileStream fs = File.Create(filename);
        LogFile = new(fs);
    }

    public static void CloseLogFile()
    {
        LogFile?.Flush();
        LogFile?.Dispose();
    }

    public static void Print(object obj, LogLevel level = LogLevel.None)
    {
        if (level == LogLevel.None) {
            Print(obj.ToString() ?? string.Empty);
        }
        else if (level == LogLevel.Info) {
            Console.ForegroundColor = ConsoleColor.Blue;
            Print($"[{level}] [{DateTime.Now:g}] -> {obj}");
        }
        else if (level == LogLevel.OK && Verbose) {
            Print($"[{level}] [{DateTime.Now:g}] -> {obj}");
        }
        else if (level == LogLevel.Warning && Verbose) {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Print($"[{level}] [{DateTime.Now:g}] -> {obj}");
        }
        else if (level == LogLevel.Error) {
            Console.ForegroundColor = ConsoleColor.Red;
            LogError(obj);
        }

        Console.ResetColor();
    }

    private static void Print(string msg)
    {
        LogFile?.WriteLine(msg);
        Console.WriteLine(msg);

#if DEBUG
        Debug.WriteLine(msg);
#endif
    }

    private static void LogError(object obj)
    {
        string header = $"[ERROR] [{DateTime.Now:g}] -> ";
        string padding = $"\n{Padding}";
        Print(header + obj.ToString()?.Replace("\n", padding));
    }
}