﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MultiRPC.Extensions;

public static class FileExt
{
    public static string CheckFilename(string filename, string location, IEnumerable<string>? blacklistedFilenames = null)
    {
        //If the directory doesn't even exist then we defo can use the filename
        if (!Directory.Exists(location))
        {
            return filename;
        }
        
        var currentFiles = Directory.EnumerateFiles(location).Select(Path.GetFileNameWithoutExtension).Concat(blacklistedFilenames ?? ArraySegment<string?>.Empty).ToArray();
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