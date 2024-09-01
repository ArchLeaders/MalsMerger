using System.Diagnostics;
using System.Text;

namespace MalsMerger.Core.Helpers;

public enum LogLevel
{
    None, Ok, Info, Warning, Error
}

public static class ConsoleHelper
{
    private const string PADDING = "  ";
    private const string TRIFORCE_ASCII = $"""
        {PADDING}*       /\\
        {PADDING}*      /  \\
        {PADDING}*     /____\\
        {PADDING}*    /\\   /\\
        {PADDING}*   /  \\ /  \\
        {PADDING}*  /____\/____\\
        """;

    public static bool Verbose { get; set; } = false;
    public static StreamWriter? LogFile { get; set; }

    public static void PrintTriforce(string title, ConsoleColor color = ConsoleColor.Blue)
    {
        StringBuilder sb = new("\n");

        const double width = 17;
        int len = title.Length + 8;

        if (len == (int)width) {
            string line = PADDING + new string('_', len);
            sb.AppendLine(TRIFORCE_ASCII.Replace("*", string.Empty));
            sb.AppendLine(line);
            sb.Append($"\n{PADDING}  - ");
            sb.Append(title);
            sb.Append(" -\n");
            sb.AppendLine(line);
        }
        else if (len > width) {
            int padding = (int)Math.Floor((len - width) / 2);
            string line = PADDING + new string('_', len);
            sb.AppendLine(TRIFORCE_ASCII.Replace("*", new string(' ', padding)));
            sb.AppendLine(line);
            sb.Append($"\n{PADDING}  - ");
            sb.Append(title);
            sb.Append(" -\n");
            sb.AppendLine(line);
        }
        else {
            int padding = (int)Math.Ceiling(-(len - width) / 2);
            string line = PADDING + new string('_', len + (padding * 2));
            sb.AppendLine(TRIFORCE_ASCII.Replace("*", string.Empty));
            sb.AppendLine(line);
            sb.Append($"\n{PADDING}  {new string(' ', padding)}- ");
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
        switch (level) {
            case LogLevel.None:
                Print(obj.ToString() ?? string.Empty);
                break;
            case LogLevel.Info:
                Console.ForegroundColor = ConsoleColor.Blue;
                Print($"[{level}] [{DateTime.Now:g}] -> {obj}");
                break;
            case LogLevel.Ok when Verbose:
                Print($"[{level}] [{DateTime.Now:g}] -> {obj}");
                break;
            case LogLevel.Warning:
                Console.ForegroundColor = ConsoleColor.Yellow;
                Print($"[{level}] [{DateTime.Now:g}] -> {obj}");
                break;
            case LogLevel.Error:
                Console.ForegroundColor = ConsoleColor.Red;
                LogError(obj);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(level), level, null);
        }

        Console.ResetColor();
    }

    private static void Print(string msg)
    {
        LogFile?.WriteLine(msg);
        Console.WriteLine(msg);
        Trace.WriteLine(msg);
    }

    private static void LogError(object obj)
    {
        string header = $"[ERROR] [{DateTime.Now:g}] -> ";
        const string padding = $"\n{PADDING}";
        Print(header + obj.ToString()?.Replace("\n", padding));
    }
}