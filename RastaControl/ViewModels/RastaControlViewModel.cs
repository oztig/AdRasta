using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using CliWrap;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia.Models;
using RastaControl.Models;
using Avalonia.Interactivity;
using DynamicData;
using DynamicData.Kernel;
using Microsoft.VisualBasic;
using RastaControl.Services;
using RastaControl.Utils;
using RastaControl.Views;

namespace RastaControl.ViewModels;

public class RastaControlViewModel : ViewModelBase
{
    /*private int _selectedConversion;

    public int SelectedConversion
    {
        get => _selectedConversion;
        set => this.RaiseAndSetIfChanged(ref _selectedConversion, value);
    }

    private ObservableCollection<RastaConversion> _rastaConversions = new ObservableCollection<RastaConversion>();

    public RastaConversion CurrentConversion
    {
        get => _rastaConversions[SelectedConversion];
        set
        {
            var rastaConversion = _rastaConversions[SelectedConversion];
            this.RaiseAndSetIfChanged(ref rastaConversion, value);
        }
    }*/

    private bool _resetDestinationPath = true;
    private WindowIcon? _icon;

    public WindowIcon? AppIcon
    {
        get => _icon;
        set => this.RaiseAndSetIfChanged(ref _icon, value);
    }

    private Settings _settings = new();
    public SourceData SourceData { get; } = new();
    public Mads Mads { get; } = new();

    public RastaConverter RastaConverter { get; } = new();
    public rc2mch rc2mch { get; } = new();

    private bool _isBusy;

    public bool IsBusy
    {
        get => _isBusy;
        set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }

    private string _rastConverterFullCommandLine = string.Empty;

    public string RastConverterFullCommandLine
    {
        get => _rastConverterFullCommandLine;
        set => this.RaiseAndSetIfChanged(ref _rastConverterFullCommandLine, value);
    }

    private string FullDestinationFileName => Path.Combine(DestinationFolderPath.Trim(),
        DestinationFileBaseName.Trim() + ".png");

    private bool _isExpanded;
    private double _previousHeight;

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (!_isExpanded && _window != null) // Store size before expanding
                _previousHeight = _window.Height;

            this.RaiseAndSetIfChanged(ref _isExpanded, value);
            AdjustWindowSize();
        }
    }

    private readonly string _previewButtonDefaultText = "Preview";
    private string _rastaCommandLineArguments = "";


    // File Selection
    public (FilePickerFileType, Action<string>) SourceFileTypeAction =>
        (FilePickerFileTypes.ImageAll, path => SourceFilePath = path);

    public (FilePickerFileType, Action<string>) MaskFileTypeAction =>
        (FilePickerFileTypes.ImageAll, path => MaskFilePath = path);

    public (FilePickerFileType, Action<string>) RegisterFileTypeAction =>
        (FilePickerFileTypes.TextPlain, path => RegisterOnOffFilePath = path);

    public ICommand PickFileCommand { get; }
    public ReactiveCommand<Unit, Unit> PickFolderCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowAboutCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowHelpCommand { get; }
    public ReactiveCommand<Unit, Unit> GenerateExecutableCommand { get; }

    private SizeToContent _windowSizing = SizeToContent.WidthAndHeight;

    public SizeToContent WindowSizing
    {
        get => _windowSizing;
        set => this.RaiseAndSetIfChanged(ref _windowSizing, value);
    }

    private Window? _window;

    // Input
    private bool _canContinue;

    public bool CanContinue
    {
        get => _canContinue;
        set => this.RaiseAndSetIfChanged(backingField: ref _canContinue, value);
    }

    private string _sourceFilePath = string.Empty;

    public string SourceFilePath
    {
        get => _sourceFilePath;
        set
        {
            this.RaiseAndSetIfChanged(ref _sourceFilePath, value);
            SourceFileBasename = value;
        }
    }

    private string _destinationFolder = string.Empty;

    public string DestinationFolderPath
    {
        get => _destinationFolder == String.Empty ? "Select Destination Folder" : _destinationFolder;
        set
        {
            this.RaiseAndSetIfChanged(ref _destinationFolder, value);
            CopySourceImageToDestination();
            SetButtons();
        }
    }

    private string _sourceFileBasename = string.Empty;

    public string SourceFileBasename
    {
        get => _sourceFileBasename == string.Empty ? "Select a file" : _sourceFileBasename;
        set
        {
            value = Path.GetFileName(value);
            this.RaiseAndSetIfChanged(ref _sourceFileBasename, value);
            SetDestinationPicker();
            SetButtons();
        }
    }

    public string DestinationFileBaseName
    {
        get
        {
            var destFile = Path.GetFileNameWithoutExtension(FileUtils.FileNameNoSpace(SourceFileBasename)) + "__c";
//            destFile = destFile + Path.GetExtension(SourceFileBasename);

            return destFile;
        }
    }


    private string _destinationFileExecutableName => Path.GetFileNameWithoutExtension(SourceFileBasename);

    private bool _autoHeight = true;

    public bool AutoHeight
    {
        get => _autoHeight;
        set
        {
            this.RaiseAndSetIfChanged(ref _autoHeight, value);
            HeightEnabled = !_autoHeight;
        }
    }

    private bool _heightEnabled;

    public bool HeightEnabled
    {
        get => _heightEnabled;
        set => this.RaiseAndSetIfChanged(ref _heightEnabled, value);
    }

    private decimal? _height = 240;

    public decimal? Height
    {
        get => _height;
        set
        {
            value ??= 0;
            this.RaiseAndSetIfChanged(ref _height, value);
        }
    }

    private string _selectedResizeFilter;

    public string SelectedResizeFilter
    {
        get => _selectedResizeFilter;
        set => this.RaiseAndSetIfChanged(ref _selectedResizeFilter, value);
    }

    private string _selectedPalette;

    public string SelectedPalette
    {
        get => _selectedPalette;
        set => this.RaiseAndSetIfChanged(ref _selectedPalette, value);
    }


    //Preprocess
    private string _selectedPreColourDistance;

    public string SelectedPreColourDistance
    {
        get => _selectedPreColourDistance;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedPreColourDistance, value);
            SetPreviewButtonWarning();
        }
    }

    private string _selectedDithering;

    public string SelectedDithering
    {
        get => _selectedDithering;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedDithering, value);
            SetPreviewButtonWarning();
        }
    }

    private decimal? _ditheringStrength = 1;

    public decimal? DitheringStrength
    {
        get => _ditheringStrength;
        set => this.RaiseAndSetIfChanged(ref _ditheringStrength, value);
    }

    private decimal? _ditheringRandomness = 0;

    public decimal? DitheringRandomness
    {
        get => _ditheringRandomness;
        set => this.RaiseAndSetIfChanged(ref _ditheringRandomness, value);
    }

    private decimal? _brightness = 0;

    public decimal? Brightness
    {
        get => _brightness;
        set
        {
            value ??= 0;
            this.RaiseAndSetIfChanged(ref _brightness, value);
        }
    }

    private decimal? _contrast = 0;

    public decimal? Contrast
    {
        get => _contrast;
        set
        {
            value ??= 0;
            this.RaiseAndSetIfChanged(ref _contrast, value);
        }
    }

    private decimal? _gamma = 1;

    public decimal? Gamma
    {
        get => _gamma;
        set
        {
            value ??= 0;
            this.RaiseAndSetIfChanged(ref _gamma, value);
        }
    }

    private bool _canPreview;

    public bool CanPreview
    {
        get => _canPreview;
        set => this.RaiseAndSetIfChanged(ref _canPreview, value);
    }

    private string _previewButtonText = "Preview";

    public string PreviewButtonText
    {
        get => _previewButtonText;
        set => this.RaiseAndSetIfChanged(ref _previewButtonText, value);
    }

    // Conversion
    private string _maskFilePath = string.Empty;

    public string MaskFilePath
    {
        get => _maskFilePath;
        set
        {
            this.RaiseAndSetIfChanged(ref _maskFilePath, value);
            MaskFileBasename = value;
        }
    }

    private string _maskFileBasename = string.Empty;

    public string MaskFileBasename
    {
        get => _maskFileBasename == string.Empty ? "Select a file" : _maskFileBasename;
        set
        {
            value = Path.GetFileName(value);
            this.RaiseAndSetIfChanged(ref _maskFileBasename, value);
        }
    }

    private decimal? _maskStrength = (decimal?)1.0;

    public decimal? MaskStrength
    {
        get => _maskStrength;
        set
        {
            value ??= 0;
            this.RaiseAndSetIfChanged(ref _maskStrength, value);
        }
    }

    private string _registerOnOffFilePath = string.Empty;

    public string RegisterOnOffFilePath
    {
        get => _registerOnOffFilePath;
        set
        {
            this.RaiseAndSetIfChanged(ref _registerOnOffFilePath, value);
            RegisterOnOffFileBasename = value;
        }
    }

    private string _registerOnOffFileBasename = string.Empty;

    public string RegisterOnOffFileBasename
    {
        get => _registerOnOffFileBasename == string.Empty ? "Select a file" : _registerOnOffFileBasename;
        set
        {
            SetCanEditRegisterFile(value);
            value = Path.GetFileName(value);
            this.RaiseAndSetIfChanged(ref _registerOnOffFileBasename, value);
        }
    }

    private bool _canEditRegisterFile;

    public bool CanEditRegisterFile
    {
        get => _canEditRegisterFile;
        set => this.RaiseAndSetIfChanged(ref _canEditRegisterFile, value);
    }

    private string _selectedThread;

    public string SelectedThread
    {
        get => _selectedThread;
        set => this.RaiseAndSetIfChanged(ref _selectedThread, value);
    }

    private string _selectedColourDistance;

    public string SelectedColourDistance
    {
        get => _selectedColourDistance;
        set => this.RaiseAndSetIfChanged(ref _selectedColourDistance, value);
    }

    private string _selectedInitialState;

    public string SelectedInitialState
    {
        get => _selectedInitialState;
        set => this.RaiseAndSetIfChanged(ref _selectedInitialState, value);
    }

    private string _selectedAutoSavePeriod;

    public string SelectedAutoSavePeriod
    {
        get => _selectedAutoSavePeriod;
        set => this.RaiseAndSetIfChanged(ref _selectedAutoSavePeriod, value);
    }


    // Generate
    private decimal? _numberOfSolutions = 1;

    public decimal? NumberOfSolutions
    {
        get => _numberOfSolutions;
        set
        {
            value ??= 0;
            this.RaiseAndSetIfChanged(ref _numberOfSolutions, value);
        }
    }

    private bool _canConvert;

    public bool CanConvert
    {
        get => _canConvert;
        set => this.RaiseAndSetIfChanged(ref _canConvert, value);
    }

    private bool _canSetDestination;

    public bool CanSetDestination
    {
        get => _canSetDestination;
        set => this.RaiseAndSetIfChanged(ref _canSetDestination, value);
    }

    private void CopySourceImageToDestination()
    {
        if (File.Exists(SourceFilePath) && Directory.Exists(DestinationFolderPath))
        {
            var sourceNoSpace = FileUtils.FileNameNoSpace(Path.GetFileName(SourceFilePath));
            var newFilePath = Path.Combine(DestinationFolderPath, sourceNoSpace);

            if (SourceFilePath != newFilePath)
            {
                File.Copy(SourceFilePath, newFilePath, true);
                _resetDestinationPath = false;
                SourceFilePath = newFilePath;
                _resetDestinationPath = true;
            }
        }
    }

    private void PopulateDefaultValues()
    {
        Brightness = 0;
        Contrast = 0;
        Gamma = (decimal)1.0;
        AutoHeight = true;
        CanPreview = false;
        CanConvert = false;
        Height = 240;
        NumberOfSolutions = 1;
        DitheringStrength = 1;
        DitheringRandomness = 0;
        MaskStrength = (decimal)1.0;
        SourceFilePath = string.Empty;
        DestinationFolderPath = string.Empty;
        MaskFilePath = string.Empty;
        RegisterOnOffFilePath = string.Empty;
        RastConverterFullCommandLine = string.Empty;

        SelectedPalette = "laoo";
        SelectedResizeFilter = "box";
        SelectedPreColourDistance = "ciede";
        SelectedDithering = "none";
        SelectedColourDistance = "yuv";
        SelectedInitialState = "random";
        SelectedThread = SourceData.TotalThreads[^1];
        SelectedAutoSavePeriod = "0";
    }

    /// <summary>
    /// Find 1st image in current dir, and set selected dir and file to that
    /// </summary>
    private async void SetDefaultSelectedFile()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var continueDir = Path.Combine(currentDir, ".Continue");
        var firstImage = await FileUtils.GetFirstImage(currentDir);
        var firstImage_extension = Path.GetExtension(firstImage);
        var convImage = Path.Combine(currentDir, Path.GetFileNameWithoutExtension(firstImage) + "__c" + ".png");

        // if (Directory.Exists(continueDir) && firstImage != null && File.Exists(firstImage))
        if (firstImage != null && File.Exists(firstImage) && File.Exists(convImage))
        {
            SourceFilePath = firstImage;
            DestinationFolderPath = currentDir;
            CanContinue = true;
        }
    }

    private void SetPreviewButtonWarning()
    {
        if (SelectedPreColourDistance == "ciede" && SelectedDithering == "knoll")
            SetPreviewButtonText(" (Slow with ciede + knoll)");
        else
            SetPreviewButtonText("");
    }

    private void SetPreviewButtonText(string toAdd)
    {
        PreviewButtonText = toAdd == string.Empty ? _previewButtonDefaultText : _previewButtonDefaultText + toAdd;
    }

    public async Task ResetAllValues()
    {
        var messageBox = MessageBoxManager.GetMessageBoxCustom(new MessageBoxCustomParams
        {
            ContentTitle = "Reset all settings?",
            ContentMessage = "This will set all settings back to their default values, are you sure?",
            ButtonDefinitions = new List<ButtonDefinition>
            {
                new ButtonDefinition { Name = "Okay" },
                new ButtonDefinition { Name = "Cancel" }
            },
            ShowInCenter = true, WindowStartupLocation = WindowStartupLocation.CenterOwner
        });

        var result = await messageBox.ShowWindowDialogAsync(_window);
        if (result.ToLower() == "okay")
            PopulateDefaultValues();
    }

    public void SetWindow(Window window)
    {
        _window = window;
    }

    public RastaControlViewModel(Window window)
    {
        _window = window;


        PopulateDefaultValues();
        SetIcon();

        PickFileCommand =
            ReactiveCommand.Create<(FilePickerFileType, Action<string>)>(async void (param) =>
                await OpenFilePicker(param.Item1, param.Item2)
            );
        PickFolderCommand = ReactiveCommand.CreateFromTask(PickFolderAsync);
        ShowHelpCommand = ReactiveCommand.CreateFromTask(async () => await ShowHelpMessage());
        ShowAboutCommand = ReactiveCommand.CreateFromTask(async () => await ShowAboutMessage());
        GenerateExecutableCommand = ReactiveCommand.CreateFromTask(async () =>
            await GenerateAndShowOutput());

        if (_settings.PopulateDefaultFile)
            SetDefaultSelectedFile();
    }


    private async Task OnLoaded(object? sender, RoutedEventArgs e)
    {
        await CheckInitialSetup();
    }

    public async Task CheckInitialSetup()
    {
        if (!_settings.CheckIniFileExists())
            await ShowMissingIniFile();
    }

    private void SetIcon()
    {
        Uri iconPath;

        try
        {
            if (Debugger.IsAttached)
                iconPath = new Uri($"avares://RastaControl/Assets/RastaContro-Debug.png");
            else
                iconPath = new Uri($"avares://RastaControl/Assets/copilot-polly.png");

            AppIcon = new WindowIcon(new Bitmap(AssetLoader.Open(iconPath)));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private async Task ShowHelpMessage()
    {
        try
        {
            var result = await Cli.Wrap(_settings.DefaultExecuteCommand)
                .WithArguments(SafeCommand.QuoteIfNeeded(_settings.HelpFileLocation))
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

    }

    private async Task GenerateAndShowOutput()
    {
        var safeCommand = _settings.MadsLocation;
        var safeParams = _settings.NoNameFilesLocation;
        var executableFilename = Path.GetFileNameWithoutExtension(_destinationFileExecutableName)?.Trim();
        var fullExePath = "";

        var userInput = await DialogService.ShowInputDialogAsync("Executable Name (no .xex extension required)",
            executableFilename, "destination name, no .xex required", _window);
        if (!(userInput.confirmed ?? false))
            return;

        fullExePath = Path.Combine(Path.GetDirectoryName(FullDestinationFileName), userInput.value.Trim() + ".xex");

        // Generate the .xex (sync, as need it to finish before we try and view the ouput
        await Mads.GenerateExecutableFile(safeCommand, safeParams, FullDestinationFileName, fullExePath
        ); //.GetAwaiter().GetResult();

        /*var toView = Path.Combine(Path.GetDirectoryName(FullDestinationFileName),
            Path.GetFileNameWithoutExtension(FullDestinationFileName)?.Trim() + ".xex");*/

        var toViewParams = new List<string>();
        toViewParams.Add(fullExePath);

        // View the output in Atari Emulator
        var result = await Cli.Wrap(_settings.DefaultExecuteCommand)
            .WithArguments(toViewParams, true)
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync();
    }

    private async Task ShowMissingIniFile()
    {
        var missingFileMessage = "Cannot find '" + _settings.IniFileLocation;

        var messageBox = MessageBoxManager.GetMessageBoxCustom(new MessageBoxCustomParams
        {
            ContentTitle = "Missing RastaControl.ini",
            Icon = Icon.Error,
            ContentMessage = missingFileMessage,
            ButtonDefinitions = new List<ButtonDefinition>
            {
                new ButtonDefinition { Name = "OK" },
            },
            ShowInCenter = true, WindowStartupLocation = WindowStartupLocation.CenterOwner
        });

        var result = await messageBox.ShowWindowDialogAsync(_window);

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp)
            desktopApp.Shutdown();
    }

    private async Task ShowAboutMessage()
    {
        var aboutMessage = "RastaConverter by Jakub Debski 2012-2025\n";
        aboutMessage += "RastaControl by oztig (Nick Pearson)\n";
        aboutMessage += "MADS and RC2MCH by Tomasz Biela\n\n";
        aboutMessage += "Special Thanks to:\n";
        aboutMessage += "Arkadiusz Lubaszka for the original RC GUI\n\n";
        aboutMessage += "Developed using JetBrains Rider and Avalonia UI \n";


        var messageBox = MessageBoxManager.GetMessageBoxCustom(new MessageBoxCustomParams
        {
            ContentTitle = "RastaControl (version 1.0 Beta)",
            Icon = Icon.Info,
            ContentMessage = aboutMessage,
            ButtonDefinitions = new List<ButtonDefinition>
            {
                new ButtonDefinition { Name = "OK" },
            },
            ShowInCenter = true, WindowStartupLocation = WindowStartupLocation.CenterOwner
        });

        var result = await messageBox.ShowWindowDialogAsync(_window);
    }

    private void SetDestinationPicker()
    {
        CanSetDestination = _sourceFilePath != String.Empty;
        if (_resetDestinationPath)
            DestinationFolderPath = string.Empty;
    }

    private void SetButtons()
    {
        // Can Preview rules - subject to change!
        CanPreview = _destinationFolder != string.Empty;
        CanConvert = CanPreview && _sourceFilePath != string.Empty;
        CanContinue = CanConvert;
        //  CanContinue = Directory.Exists(Path.Combine(_destinationFolder, ".Continue")) && CanConvert;
    }

    private void SetCanEditRegisterFile(string value)
    {
        CanEditRegisterFile = value != string.Empty;
    }

    private void AdjustWindowSize()
    {
        if (_window != null)
            // How do we auto size the expand? - Don't like hard coded values!
            _window.Height = IsExpanded ? _window.Height + 150 : _previousHeight;
    }

    private async Task OpenFilePicker(FilePickerFileType fileType, Action<string> setPath)
    {
        if (_window is null) return;

        var files = await _window.StorageProvider.OpenFilePickerAsync(new()
        {
            Title = "Select a file",
            AllowMultiple = false,

            FileTypeFilter = new[] { fileType },
        });

        setPath(files.Count > 0 ? Uri.UnescapeDataString(files[0].Path.AbsolutePath) : string.Empty);
    }

    private async Task PickFolderAsync()
    {
        var storageProvider = _window.StorageProvider;
        var folders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select a Folder",
            AllowMultiple = false
        });

        DestinationFolderPath = folders.Count > 0 ? Uri.UnescapeDataString(folders[0].Path.LocalPath) : string.Empty;
    }

    private async Task<bool> ViewImage(string fileName)
    {
        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();
        var ret = false;

        try
        {
            if (File.Exists(fileName))
            {
                var result = await Cli.Wrap(_settings.DefaultExecuteCommand)
                    .WithArguments(SafeCommand.QuoteIfNeeded(fileName))
                    .WithValidation(CommandResultValidation.None)
                    .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                    .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                    .ExecuteAsync();

                var stdOut = stdOutBuffer.ToString();
                var stdErr = stdErrBuffer.ToString();

                ret = true;
            }
            else
            {
                ret = false;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            ret = false;
        }

        return ret;
    }

    // TODO This should be in the RastaConverter object - and be passed a 'RastaConversion' object
    private async Task<IReadOnlyList<string>> GenerateRastaArguments(bool isPreview = false, bool isContinue = false)
    {
        var args = new List<string>();

        args.Add($"/i={SourceFilePath}");
        args.Add($"/o={FullDestinationFileName}");

        if (!AutoHeight)
            args.Add($"/h={Height}");

        args.Add($"/pal={Path.Combine(_settings.PaletteDirectory, SelectedPalette.Trim() + ".act")}");

        args.Add($"/filter={SelectedResizeFilter}");

        args.Add($"/predistance={SelectedPreColourDistance}");
        args.Add($"/dither={SelectedDithering}");

        if (SelectedDithering != "none")
        {
            args.Add($"/dither_val={DitheringStrength}");
            args.Add($"/dither_rand={DitheringRandomness}");
        }

        args.Add($"/brightness={Brightness}");
        args.Add($"/contrast={Contrast}");
        args.Add($"/gamma={Gamma}");

        if (!string.IsNullOrWhiteSpace(MaskFilePath))
        {
            args.Add($"/details={MaskFilePath}");
            args.Add($"/details_val={MaskStrength}");
        }

        if (!string.IsNullOrWhiteSpace(RegisterOnOffFilePath))
            args.Add($"/onoff={RegisterOnOffFilePath}");

        args.Add($"/distance={SelectedColourDistance}");
        args.Add($"/init={SelectedInitialState}");
        args.Add($"/s={NumberOfSolutions}");
        args.Add($"/save={SelectedAutoSavePeriod}");
        args.Add($"/threads={SelectedThread}");

        if (isPreview)
            args.Add("/preprocess");

        if (isContinue)
            args.Add("/continue");

        RastConverterFullCommandLine = await RastaConverter.GenerateFullCommandLineString(_settings, args);

        return args;
    }

    public async Task ContinueConvert()
    {
        var safeParams = await GenerateRastaArguments(false, true);

        await RastaConverter.ContinueConversion(_settings, safeParams, SourceFilePath,
            FullDestinationFileName, _window);

        await ViewImage(FullDestinationFileName.Trim());
    }

    public async Task ConvertImage()
    {
        var safeCommand = _settings.RastaConverterCommand;
        var safeParams = await GenerateRastaArguments(); // _rastaCommandLineArguments;
        SetButtons();
        await RastaConverter.ExecuteRastaConverterCommand(safeCommand, safeParams);
        await ViewImage(FullDestinationFileName.Trim());
    }

    public async Task GenerateMCH()
    {
        await rc2mch.GenerateMCH(_settings.RC2MCHCommand, FullDestinationFileName);
    }

    public async Task PreviewImage()
    {
        //  GenerateRastaCommand(true);

        var safeCommand = _settings.RastaConverterCommand;
        var safeParams = await GenerateRastaArguments(true); // _rastaCommandLineArguments;
        var viewFileName = FullDestinationFileName.Trim() + "-dst.png";

        await RastaConverter.ExecuteRastaConverterCommand(safeCommand, safeParams);
        await ViewImage(viewFileName);
    }

    public void ShowRastCommandLineText()
    {
        //  GenerateRastaCommand();
        GenerateRastaArguments();
    }
}