using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using CliWrap;
using CliWrap.Buffered;
using MsBox.Avalonia;
using System.IO;
using RastaControl.Utils;

namespace RastaControl.Models;

public class RastaConverter
{
    public async Task ExecuteRastaConverterCommand(string commandLocation, IReadOnlyList<string> commandLineArguments)
    {
        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();

        // TODO - Launch, and listen for results coming back - this includes procecss ID, and ???
        try
        {
            await Cli.Wrap(commandLocation)
                .WithArguments(commandLineArguments, true)
                .WithValidation(CommandResultValidation.None)
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                .ExecuteBufferedAsync();
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

    public async Task ContinueConversion(Settings _settings, string sourceFile, string conversionFile,
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

            // Adjust the .opt and .rp files to allow process to continue
            await Adjust_Opt_And_RpFiles();

            // Copy back and clear up?
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private async Task Adjust_Opt_And_RpFiles()
    {
        // Need to know:
        //  - location of files
        // - Basename of original Input source image (copied version)
        // Need all RastaArguments (as array / collection / whatever)
        // Change ; Input to baename of copied original image
        // Change ;CmdLine to /i=basename as above, /o=basename of copied image (the _c one)
        // Change Palettes to relative Path, so just 'Palettes/name_of_palette_selected'
        // Rest of the command line to stay the same
        
        
    }
}