using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;

namespace RastaControl.Models;

public class RastaConverter
{
    public async Task ExecuteRastaConverterCommand(string commandLocation, IReadOnlyList< string> commandLineArguments)
    {
        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();

        // TODO - Launch, and listen for results coming back - this includes procecss ID, and ???
        try
        {
            await Cli.Wrap(commandLocation)
                .WithArguments(commandLineArguments,true)
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
}