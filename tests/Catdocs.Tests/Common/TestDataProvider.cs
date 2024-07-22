namespace Catdocs.Tests.Common;

public class TestDataProvider
{

    private const string _rootTestDataDir = "TestDataFiles";

    protected virtual string GetRootTestDataDir()
    {
        return _rootTestDataDir;
    }

    protected string GetTestDataFilePath(string filePath)
    {
        return Path.Combine(GetRootTestDataDir(), filePath);
    }

    public string ReadFileAsString(string filePath)
    {
        return File.ReadAllText(GetTestDataFilePath(filePath));
    }
}