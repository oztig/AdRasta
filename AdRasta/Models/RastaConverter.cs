using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using CliWrap;
using CliWrap.Buffered;
using MsBox.Avalonia;
using System.IO;
using System.Linq;
using AdRasta.Services;
using AdRasta.Utils;
using MsBox.Avalonia.Enums;

namespace AdRasta.Models;

public class RastaConverter
{
    public async Task ExecuteRastaConverterCommand(string commandLocation, IReadOnlyList<string> commandLineArguments,
        string workingDIR = "")
    {
        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();

        // TODO - Launch, and listen for results coming back - this includes procecss ID, and ???
        try
        {
            if (workingDIR != "")
            {
                await Cli.Wrap(commandLocation)
                    .WithWorkingDirectory(workingDIR)
                    .WithArguments(commandLineArguments, true)
                    .WithValidation(CommandResultValidation.None)
                    .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                    .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                    .ExecuteBufferedAsync();
            }
            else
            {
                await Cli.Wrap(commandLocation)
                    .WithArguments(commandLineArguments, true)
                    .WithValidation(CommandResultValidation.None)
                    .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                    .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                    .ExecuteBufferedAsync();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<string> GenerateFullCommandLineString(Settings _settings, IReadOnlyList<string> argsString)
    {
        var fullCommandLine = $"{_settings.RastaConverterCommand} {string.Join(" ", argsString)}";
        return fullCommandLine;
    }

    public async Task ContinueConversion(RastaConversion _conversion, Window _window)
    {
    }

    public async Task ContinueConversion(Settings _settings, IReadOnlyList<string> commandLineArguments,
        string sourceFile, string conversionFile,
        Window _window)
    {
        var baseDirectory = Path.GetDirectoryName(conversionFile);
        var continueDirectory = Path.Combine(baseDirectory, ".Continue");
        var baseCopyFileName = Path.GetFileName(conversionFile);
        var searchPattern = baseCopyFileName.Trim() + "*.*";
        var baseOriginalFileName = Path.GetFileName(sourceFile);

        try
        {
            // Create Dir, if not already present
            Directory.CreateDirectory(continueDirectory);

            // Copy the images to the .Continue Directory
            await FileUtils.CopyMatchingFilesAsync(baseDirectory, continueDirectory, searchPattern);
            await FileUtils.CopyMatchingFilesAsync(baseDirectory, continueDirectory, baseOriginalFileName);

            // Rename the files in the .Continue Directory
            await FileUtils.RenameMatchingFilesAsync(continueDirectory, baseCopyFileName, "*.*", "output.png");

            // Copy RastaConverter Command
            await FileUtils.CopyMatchingFilesAsync(_settings.BaseRastaCommandLocation, continueDirectory,
                _settings.BaseRastaCommand);

            // Font File
            await FileUtils.CopyMatchingFilesAsync(_settings.BaseRastaCommandLocation, continueDirectory,
                "clacon2.ttf");

            // Palette Dir
            await FileUtils.CopyDirectoryIncludingRoot(_settings.PaletteDirectory, continueDirectory);

            // Generate the RastaConverter Command
            // TODO Done in other class for now - should be part of this 

            // Adjust the .opt and .rp files to allow process to continue
            await Adjust_Opt_And_RpFiles(commandLineArguments);

            var continueArgs = new List<string>();
            continueArgs.Add("/continue");

            // Continue
            await ExecuteRastaConverterCommand(Path.Combine(continueDirectory, _settings.BaseRastaCommand),
                continueArgs, continueDirectory);

            var copyBack = _settings.CopyWithoutConfirm;

            // Confirm ?
            if (!copyBack)
            {
                var answer = await DialogService.ShowYesNo("Copy Back To Original?",
                    "This will copy back, and overwrite the original files, and remove any temporary files\n Are you sure?",
                    _window);
                if (answer.ToString().ToUpper() == "YES")
                    copyBack = true;
            }

            if (copyBack)
            {
                // Rename back to source file name
                await FileUtils.RenameMatchingFilesAsync(continueDirectory, "output.png", "*", baseCopyFileName);
                
                // Move back to original
                await FileUtils.MoveMatchingFilesAsync(continueDirectory, baseDirectory,
                    baseCopyFileName.Trim() + "*.*");

                // Tidy Up
                await FileUtils.ClearDirectoryAsync(continueDirectory);
            }
        }
        catch (Exception e)
        {
            MessageBoxManager.GetMessageBoxStandard("Unexpected Error", "ContinueConversion failed" + e.Message,
                ButtonEnum.Ok, Icon.Error);
        }
    }

    private async Task Adjust_Opt_And_RpFiles(IReadOnlyList<string> commandLineArguments)
    {
        // Need to know:
        //  - location of files to be adjusted
        var toBeAdjustedLocation =
            Path.Combine(Path.GetDirectoryName(commandLineArguments[commandLineArguments.Count - 3]), ".Continue")
                .Replace("/i=", "").Replace("\\i=", "");
        var optFileLocation = Path.Combine(toBeAdjustedLocation, "output.png.opt");
        var rpFileLocation = Path.Combine(toBeAdjustedLocation, "output.png.rp");

        if (File.Exists(optFileLocation) && File.Exists(rpFileLocation))
        {
            await AdjustFile(optFileLocation, commandLineArguments);
            await AdjustFile(rpFileLocation, commandLineArguments);
        }
    }

    private async Task AdjustFile(string filename, IReadOnlyList<string> commandLineArguments)
    {
        // - Basename of original Input source image (copied version)
        var orignalSourceImageBaseName = Path.GetFileName(commandLineArguments[commandLineArguments.Count - 3]);

        // Basename of 'Copied' Image
        var copiedImageBaseName = Path.GetFileName(commandLineArguments[commandLineArguments.Count - 2]);

        // Change ; Input to basename of copied original image
        var inputIdentifier = "; InputName: ";

        // Change ;CmdLine to /i=basename as above, /o=basename of copied image (the _c one)
        var cmdIdentifier = "; CmdLine: ";

        try
        {
            var optContent = await File.ReadAllLinesAsync(filename);
            var updatedoptLines = optContent.Select(line =>
            {
                if (line.StartsWith(inputIdentifier))
                {
                    return inputIdentifier + "./" + orignalSourceImageBaseName;
                }

                if (line.StartsWith(cmdIdentifier))
                {
                    var newLine = "";

                    // All params up to /i=
                    newLine = cmdIdentifier + stringParseUtils.ExtractBetweenCmdLineAndInput(line);

                    // source image
                    newLine += " /i=./" + orignalSourceImageBaseName;

                    // Palette
                    var strLengh = commandLineArguments[commandLineArguments.Count - 1].Length;
                    var lastSlashIndex = commandLineArguments[commandLineArguments.Count - 1].LastIndexOf('/');
                    if (lastSlashIndex != -1 && lastSlashIndex + 1 < strLengh)
                        newLine += " /pal=./Palettes/" + commandLineArguments[commandLineArguments.Count - 1]
                            .Substring(lastSlashIndex + 1);

                    return newLine;
                }

                return line;
            }).ToArray();

            await File.WriteAllLinesAsync(filename, updatedoptLines);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}