namespace Catdocs.Tests.Common;

public class TestDataProvider
{

    private const string _rootTestDataDir = "TestDataFiles";

    private static string GetRootTestDataDir()
    {
        return _rootTestDataDir;
    }

    private static string GetTestDataFilePath(string filePath)
    {
        return Path.Combine(GetRootTestDataDir(), filePath);
    }

    public static string ReadFileAsString(string filePath)
    {
        return File.ReadAllText(GetTestDataFilePath(filePath));
    }
}