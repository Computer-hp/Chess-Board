public static class AssetsPathHelper
{
    private static readonly Lazy<string> lazyAssetsPath = new(GetProjectPath);
    public static string AssetsPath => lazyAssetsPath.Value;

    
    private static string GetProjectPath()
    {
        string appDirectory = Application.StartupPath;
        string imagesFolder = Path.GetFullPath(Path.Combine(appDirectory, "..\\..\\..\\Assets\\"));
        return imagesFolder;
    }
}
