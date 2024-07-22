namespace Catdocs.Tests.Common;

public class TestDataProvider
{

    private const string _rootTestDataDir = "TestDataFiles";

    public static string GetRootTestDataDir()
    {
        return _rootTestDataDir;
    }

    public static string GetTestDataFilePath(string filePath)
    {
        var fileInfo = new FileInfo(Path.GetFullPath(Path.Combine(GetRootTestDataDir(), filePath)));
        return fileInfo.FullName;
    }

    public static string ReadFileAsString(string filePath)
    {
        return File.ReadAllText(GetTestDataFilePath(filePath));
    }
}