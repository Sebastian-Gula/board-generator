using System.Collections.Generic;
using System.IO;
using System.Linq;

public class DictionaryInfoHelper
{
    public IList<string> GetFolderNames(string path)
    {
        var dir = new DirectoryInfo(path);
        var info = dir.GetDirectories();

        return info.Select(i => i.Name).ToList();
    }

    public IList<string> GetFilesNames(string path)
    {
        var dir = new DirectoryInfo(path);
        var info = dir.GetFiles();

        return info.Select(i => i.Name).ToList();
    }
}
