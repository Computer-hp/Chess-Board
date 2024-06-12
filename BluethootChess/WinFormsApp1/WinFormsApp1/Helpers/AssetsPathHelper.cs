using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class AssetsPathHelper
{
    private static readonly Lazy<string> lazyAssetsPath = new Lazy<string>(GetProjectPath);
    public static string AssetsPath => lazyAssetsPath.Value;

    
    private static string GetProjectPath()
    {
        string appDirectory = Application.StartupPath;
        string imagesFolder = Path.GetFullPath(Path.Combine(appDirectory, "..\\..\\..\\Assets\\"));
        return imagesFolder;
    }
}
