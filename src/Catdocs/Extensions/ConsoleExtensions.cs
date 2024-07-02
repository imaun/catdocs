namespace Catdocs;

public static class ConsoleExtensions
{

    public static void WriteErrorLine(string error)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(error);
        Console.ResetColor();
    }
}