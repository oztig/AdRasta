using System;
using System.IO;

namespace RastaControl.Models;

public class Settings
{
    public  string IniFileLocation = Path.Combine(Directory.GetCurrentDirectory().Trim(), "RastaControl.ini");
    public string RastaConverterCommand { get; set; } = string.Empty;

    public string BaseRastaCommandLocation => Path.GetDirectoryName(RastaConverterCommand); 
    public string BaseRastaCommand => Path.GetFileName(RastaConverterCommand);
    
    public string RC2MCHCommand { get; set; } = string.Empty;
    public string DefaultExecuteCommand { get; set; } = string.Empty;
    public string PaletteDirectory { get; set; } = string.Empty;
    public string MadsLocation { get; set; } = string.Empty;
    public string NoNameFilesLocation { get; set; } = string.Empty;

    public string HelpFileLocation { get; set; } = string.Empty;
    
    public bool CopyWithoutConfirm { get; set; }


    public Settings()
    {
        SetDefaults();
    }

    public bool CheckIniFileExists()
    {
        var currentDir = Directory.GetCurrentDirectory().Trim();
        Path.Combine(currentDir, "RastaControl.ini");
        return File.Exists(IniFileLocation);
    }

    private void SetDefaults()
    {
        if (OperatingSystem.IsWindows())
            DefaultExecuteCommand = "explorer";
        else if (OperatingSystem.IsLinux())
            DefaultExecuteCommand = "xdg-open";
        else
            DefaultExecuteCommand = "explorer";
        
        var ini = new Sini.IniFile(IniFileLocation);
        
        // Get from settings file, or ideally allow user to select
        RastaConverterCommand = ini.GetStr("Locations", "RastaConverter", string.Empty); // = "/home/nickp/Downloads/RastaConverter-master/src/rastaconv";
        RC2MCHCommand = ini.GetStr("Locations", "RC2MCH", string.Empty); // = "/home/nickp/Downloads/RastaConverter-master/src/rastaconv";
        HelpFileLocation = ini.GetStr("Locations", "RastaHelpFile", string.Empty); // "/home/nickp/Downloads/RastaConverter-master/help.txt";
        MadsLocation = ini.GetStr("Locations", "MADS", string.Empty);  // "/home/nickp/WUDSN/Tools/ASM/MADS/mads.linux-x86-64";
        PaletteDirectory = ini.GetStr("Locations", "PalettesDir", string.Empty); // "/home/nickp/Downloads/RastaConverter-master/src/Palettes";
        NoNameFilesLocation = ini.GetStr("Locations", "NoNameFilesDir", string.Empty); // "/home/nickp/Downloads/RastaConverter-master/Generator";
        CopyWithoutConfirm = ini.GetBool("Continue", "CopyWithoutConfirm", false);
    }
}