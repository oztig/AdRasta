using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdRasta.Utils;
using CliWrap;

namespace AdRasta.Models;

public class Mads
{
    public new List<string> MadsCommandLineArguments { get; set; } = new List<string>();
    private static string NoNameHeader { get; set; } = "no_name.h";
    private static string NoNameAsq { get; set; } = "no_name.asq";
    private string NoNameHeaderLocation { get; set; } = string.Empty;
    private string NoNameAsqLocation { get; set; } = string.Empty;
    private static string? DestinationDir { get; set; } = string.Empty;
    private static string? DestinationFileNoExtension { get; set; } = string.Empty;
    private static string NewHeaderName { get; set; } = string.Empty;
    private static string NewAsqName { get; set; } = string.Empty;
    private string NewHeaderLocation { get; set; } = string.Empty;
    private string NewAsqLocation { get; set; } = string.Empty;
    private string? OutputFileName { get; set; } = string.Empty;

    public async Task GenerateExecutableFile(string madsExecutable, string noNameFilesLocation,
        string outputFileLocation, string executableFileName)
    {
        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();
        NoNameHeaderLocation = Path.Combine(noNameFilesLocation, NoNameHeader);
        NoNameAsqLocation = Path.Combine(noNameFilesLocation, NoNameAsq);
        DestinationDir = Path.GetDirectoryName(outputFileLocation);
        DestinationFileNoExtension = Path.GetFileNameWithoutExtension(outputFileLocation)?.Trim();
        NewHeaderName = NoNameHeader.Replace("no_name", DestinationFileNoExtension);
        NewAsqName = NoNameAsq.Replace("no_name", DestinationFileNoExtension);
        NewHeaderLocation = Path.Combine(DestinationDir, NewHeaderName);
        NewAsqLocation = Path.Combine(DestinationDir, NewAsqName);
        OutputFileName = Path.GetFileName(outputFileLocation)?.Trim();

        MadsCommandLineArguments.Clear();
        MadsCommandLineArguments.Add(SafeCommand.QuoteIfNeeded(NewAsqLocation));
        MadsCommandLineArguments.Add(" -o:" +
                                     SafeCommand.QuoteIfNeeded(Path.Combine(DestinationDir,
                                        executableFileName)));

        try
        {
            // Copy no_name.asq and no_name.h to new name
            if (await CopyAndModifyBuildFile())
            {
                var safeCommandExe = madsExecutable;

                // Now Run Mads to Generate the output file
                await Cli.Wrap(safeCommandExe)
                    .WithArguments(MadsCommandLineArguments, false)
                    .WithValidation(CommandResultValidation.None)
                    .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                    .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                    .ExecuteAsync();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    /// <summary>
    /// Copy no_name.asq and no_name.h to new name, based on destination filename
    /// Modify content of no_name.asq to match source filenames
    /// </summary>
    /// <returns>If Successful</returns>
    private async Task<bool> CopyAndModifyBuildFile()
    {
        var ASQCopyFrom = NoNameAsqLocation;
        var HeaderCopyFrom = NoNameHeaderLocation;
        var ASQCopyTo = NewAsqLocation;
        var HeaderCopyTo = NewHeaderLocation;

        // Check source file exists
        if (File.Exists(ASQCopyFrom) && File.Exists(HeaderCopyFrom))
        {
            // Copy to new names
            try
            {
                File.Copy(ASQCopyFrom, ASQCopyTo, true);
                File.Copy(HeaderCopyFrom, HeaderCopyTo, true);

                // Read file content
                var content = await File.ReadAllTextAsync(ASQCopyFrom);

                // Replace string
                var modifiedContent = content.Replace("output.png", OutputFileName)
                    .Replace(NoNameHeader, NewHeaderName);

                // Write to file
                await File.WriteAllTextAsync(ASQCopyTo, modifiedContent);

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        else
            return false;
    }
}