namespace SpecEditor.Lib.OpenAPI;

public static class SpecLogger
{
    
    public static string Filename { get; private set; }

    public static void SetLogFilename(string filePath)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var now = DateTime.Now;
        Filename = $"{fileName}-{now.Date.ToShortDateString()}-{now.ToShortTimeString()}.log";
    }

    public static void Log(string log)
    {
        if (string.IsNullOrWhiteSpace(log))
        {
            return;
        }

        var log_item = $"{DateTime.Now.ToLongDateString()}: {log}";
        var fs = new FileStream(
            Filename, FileMode.Append, FileAccess.Write, FileShare.Read);
        using var stream_writer = new StreamWriter(fs);
        stream_writer.WriteLine(log_item);
        stream_writer.Flush();
        stream_writer.Close();
    }
}