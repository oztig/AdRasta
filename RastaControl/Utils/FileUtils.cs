using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RastaControl.Utils;

public class FileUtils
{
    public static async Task ClearDirectoryAsync(string targetDirectory)
    {
        string[] files = Directory.GetFiles(targetDirectory, "*", SearchOption.AllDirectories);

        // Delete all files
        foreach (var file in files)
        {
            File.SetAttributes(file, FileAttributes.Normal); // Just in case it's read-only
            File.Delete(file);
        }

        // Delete all subdirectories
        foreach (var dir in Directory.GetDirectories(targetDirectory, "*", SearchOption.AllDirectories)
                     .OrderByDescending(d => d.Length)) // delete from deepest to shallowest
        {
            Directory.Delete(dir, true);
        }
    }

    public static async Task MoveMatchingFilesAsync(string sourceDir, string destinationDir, string searchPattern)
    {
        string[] files = Directory.GetFiles(sourceDir, searchPattern, SearchOption.AllDirectories);

        foreach (var moveFrom in files)
        {
            string destinationPath = Path.Combine(destinationDir, Path.GetFileName(moveFrom));

            // Move the file (overwrites if it exists—optional)
            try
            {
                File.Move(moveFrom, destinationPath, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }

    public static async Task CopyMatchingFilesAsync(string sourceDir, string destinationDir, string searchPattern)
    {
        /*await Task.Run(() =>
        {*/
        var files = Directory.GetFiles(sourceDir, searchPattern, SearchOption.TopDirectoryOnly);

        foreach (var file in files)
        {
            var destPath = Path.Combine(destinationDir, Path.GetFileName(file));
            File.Copy(file, destPath, overwrite: true);
        }
        /*});*/
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
                var newPath = originalFile.Replace(baseFileName, replaceWithPattern);
                File.Move(originalFile, newPath, overwrite: true);
                File.SetCreationTime(newPath, DateTime.Now);
                File.SetLastWriteTime(newPath, DateTime.Now);
                File.SetLastAccessTime(newPath, DateTime.Now);
            }
        });
    }

    public static async Task CopyDirectoryIncludingRoot(string sourceDir, string destinationRoot)
    {
        string dirName = Path.GetFileName(sourceDir.TrimEnd(Path.DirectorySeparatorChar));
        string destDir = Path.Combine(destinationRoot, dirName);
        await CopyDirectory(sourceDir, destDir);
    }

    public static async Task CopyDirectory(string sourceDir, string destDir, bool recursive = true)
    {
        var dir = new DirectoryInfo(sourceDir);
        if (!dir.Exists)
            return;

        DirectoryInfo[] dirs = dir.GetDirectories();
        Directory.CreateDirectory(destDir);

        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destDir, file.Name);
            file.CopyTo(targetFilePath, overwrite: true);
        }

        if (recursive)
        {
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, recursive);
            }
        }
    }

    public static async Task<string> GetFirstImage(string currentDir)
    {
        string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".webp" };
        
        var firstImage = Directory.EnumerateFiles(currentDir)
            .Where(file =>
            {
                string fileName = Path.GetFileName(file);
                string ext = Path.GetExtension(file);
                bool isImage = imageExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase);
                int firstDotIndex = fileName.IndexOf('.');
                string baseName = firstDotIndex >= 0 ? fileName.Substring(0, firstDotIndex) : fileName;
                string optFile = file.Trim() + ".opt";
                bool optExists = File.Exists(optFile);
                bool endsWithcc = baseName.EndsWith("__c", StringComparison.OrdinalIgnoreCase);
                return isImage
                       && !fileName.StartsWith("output", StringComparison.OrdinalIgnoreCase) && !endsWithcc;
            })
            .FirstOrDefault();


        return firstImage;
    }

    public static string FileNameNoSpace(string fileName)
    {
        return fileName.Replace(" ", "");
    }

    // Helper to extract from last "Palettes" onward
    public static string GetSuffixPath(string path, string anchorFolder)
    {
        var index = path.LastIndexOf(anchorFolder, StringComparison.OrdinalIgnoreCase);
        return (index >= 0) ? path.Substring(index).Replace('\\', '/') : Path.GetFileName(path);
    }
}