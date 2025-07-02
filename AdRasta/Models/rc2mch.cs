using System;
using System.Text;
using System.Threading.Tasks;
using AdRasta.Utils;
using CliWrap;

namespace AdRasta.Models;

public class rc2mch
{
    public async Task GenerateMCH(string rc2MCHExecutable, string sourceFile)
    {
        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();

        try
        {
            await Cli.Wrap(rc2MCHExecutable)
                .WithArguments(SafeCommand.QuoteIfNeeded(sourceFile))
                .WithValidation(CommandResultValidation.None)
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                .ExecuteAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }
}