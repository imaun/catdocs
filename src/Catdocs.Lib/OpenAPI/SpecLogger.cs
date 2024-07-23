using System.Text;

namespace Catdocs.Lib.OpenAPI;

public static class SpecLogger
{
    
    public static string Filename { get; private set; }
    
    public static bool WriteToConsole { get; private set; }
    
    public static bool WriteToFile { get; private set; }

    public static void EnableConsoleLogging() => WriteToConsole = true;

    public static void DisableConsoleLogging() => WriteToConsole = false;

    public static void EnableWriteToFile() => WriteToFile = true;
    
    public static void DisableWriteToFile() => WriteToFile = false;

    public static void SetLogFilename(string filePath)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var now = DateTime.Now;
        Filename = $"{fileName}-{now:yyyy-mm-dd HH-MM}.log";
    }

    public static void LogException(string name, Exception exception)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{name} Exception: {exception.GetType()}");
        sb.AppendLine($"    Message: {exception.GetBaseException().Message}");
        sb.AppendLine($"    StackTrace: {exception.GetBaseException().StackTrace}");
        
        Log(sb.ToString());
    }

    public static void LogError(string error)
    {
        Log($"ERROR: {error}");
    }

    public static void LogWarning(string warning)
    {
        Log($"WARNING: {warning}");
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

        if (WriteToFile)
        {
            WriteLogToFile(log_item);
        }
    }


    private static void WriteLogToFile(string log)
    {
        var fs = new FileStream(
            Filename, FileMode.Append, FileAccess.Write, FileShare.Read);
        using var stream_writer = new StreamWriter(fs);
        stream_writer.WriteLine(log);
        stream_writer.Flush();
        stream_writer.Close();
    }
}