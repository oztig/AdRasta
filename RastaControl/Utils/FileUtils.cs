using System;
using System.IO;
using System.Threading.Tasks;

namespace RastaControl.Utils;

public class FileUtils
{
    public static async Task CopyMatchingFilesAsync(string sourceDir, string destinationDir, string searchPattern)
    {
        await Task.Run(() =>
        {
            var files = Directory.GetFiles(sourceDir, searchPattern, SearchOption.TopDirectoryOnly);

            foreach (var file in files)
            {
                var destPath = Path.Combine(destinationDir, Path.GetFileName(file));
                File.Copy(file, destPath, overwrite: true);
            }
        });
    }

    public static async Task RenameMatchingFilesAsync(string sourceDir, string baseFileName, string addSearchPattern,
        string replaceWithPattern)
    {
        var fullSearchPattern = baseFileName + addSearchPattern;
        await Task.Run(() =>
        {
            var files = Directory.GetFiles(sourceDir, baseFileName + addSearchPattern, SearchOption.TopDirectoryOnly);

            foreach (var originalFile in files)
            {
                if (Path.GetFileName(originalFile).Trim() != baseFileName)
                {
                    var newPath = originalFile.Replace(baseFileName, replaceWithPattern);
                    File.Move(originalFile, newPath, overwrite: true);
                    File.SetCreationTime(newPath,DateTime.Now);
                    File.SetLastWriteTime(newPath, DateTime.Now);
                    File.SetLastAccessTime(newPath, DateTime.Now);
                }
            }
        });
    }
}