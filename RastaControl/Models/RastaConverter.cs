using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using CliWrap;
using CliWrap.Buffered;
using MsBox.Avalonia;

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

    public async Task ContinueConversion(string commandLocation, string SourceFile, Window _window)
    {
        // Get all files in the 'source' file location (actually the destination file)
       var messageBox =   MessageBoxManager.GetMessageBoxStandard("Continue Conversion",
            "Continuing Conversion of :" + SourceFile + "\n\nUsing: " + commandLocation);
         
         var result = await messageBox.ShowWindowDialogAsync(_window);
    }
}