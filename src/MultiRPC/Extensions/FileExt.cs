using System.IO;
using System.Linq;

namespace MultiRPC.Extensions;

public static class FileExt
{
    public static string CheckFilename(string filename, string location)
    {
        var currentFiles = Directory.EnumerateFiles(location).Select(Path.GetFileNameWithoutExtension).ToArray();
        if (currentFiles.All(x => x != filename))
        {
            return filename;
        }
        
        var count = 0;
        var name = filename;
        var spaceInd = name.LastIndexOf(' ');
        if (spaceInd != -1
            && int.TryParse(name[(spaceInd + 1)..], out count))
        {
            name = name[..spaceInd];
        }
            
        while (currentFiles.Any(x => name + $" {count}" == x))
        {
            count++;
        }
        return name + $" {count}";
    }
}