using System;

namespace AdRasta.Utils;

public class stringParseUtils
{
    
    public static string ExtractBetweenCmdLineAndInput(string input)
    {
        const string cmdLineMarker = "; CmdLine:";
        const string inputMarker = " /i=";

        int startIndex = input.IndexOf(cmdLineMarker);
        if (startIndex == -1) return string.Empty;
        startIndex += cmdLineMarker.Length;

        int endIndex = input.IndexOf(inputMarker, startIndex);
        if (endIndex == -1) return string.Empty;

        return input.Substring(startIndex, endIndex - startIndex).Trim();
    }
}