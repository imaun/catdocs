using System.Text;

namespace Catdocs.Lib.OpenAPI;

public static class SpecLogger
{
    
    public static string Filename { get; private set; }
    
    public static bool WriteToConsole { get; private set; }

    public static void EnableConsoleLogging() => WriteToConsole = true;

    public static void DisableConsoleLogging() => WriteToConsole = false;

    public static void SetLogFilename(string filePath)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var now = DateTime.Now;
        Filename = $"{fileName}-{now.Date.ToShortDateString()}-{now.ToShortTimeString()}.log";
    }

    public static void LogException(string name, Exception exception)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{name} Exception: {exception.GetType()}");
        sb.AppendLine($"    Message: {exception.GetBaseException().Message}");
        sb.AppendLine($"    StackTrace: {exception.GetBaseException().StackTrace}");
        
        Log(sb.ToString());
    }

    public static void Log(string log)
    {
        if (string.IsNullOrWhiteSpace(log))
        {
            return;
        }

        var log_item = $"{DateTime.Now.ToLongDateString()}: {log}";
        
        if (WriteToConsole)
        {
            Console.WriteLine(log_item);
        }
        
        var fs = new FileStream(
            Filename, FileMode.Append, FileAccess.Write, FileShare.Read);
        using var stream_writer = new StreamWriter(fs);
        stream_writer.WriteLine(log_item);
        stream_writer.Flush();
        stream_writer.Close();
    }
}