using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Assets.Scripts.Extensions
{
    public static class ExtendedDirectory
    {
        public static IEnumerable<FileInfo> GetFilesInfo(string path,
            string searchPattern, SearchOption searchOption)
        {
            var directoryInfo = new DirectoryInfo(path);
            var files = directoryInfo.GetFiles(searchPattern);
            var results = new List<FileInfo>(files);
            var shouldContinue = searchOption == SearchOption.AllDirectories;
            var directories = directoryInfo.GetDirectories().ToList();

            while (shouldContinue)
            {
                var tempDirectories = new List<DirectoryInfo>();
                foreach (var folder in directories)
                {
                    results.AddRange(folder.GetFiles());
                    tempDirectories.AddRange(folder.GetDirectories());
                }
                directories = tempDirectories;
                shouldContinue = directories.Count > 0;
            }

            return results;
        }
    }
}