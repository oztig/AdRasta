using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdRasta.Models;
using AdRasta.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Models;

namespace AdRasta.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        this.Opened += (_, _) =>
        {
            Dispatcher.UIThread.Post(async () =>
            {
                // This runs after the window is painted
                _ = await CheckIniFileExists();
            }, DispatcherPriority.Background);
        };
    }

    private async Task<bool> CheckIniFileExists()
    {
        if (!Settings.CheckIniFileExists())
        {
            // Show a warning and exit
            var messageBox = MessageBoxManager.GetMessageBoxCustom(new MessageBoxCustomParams
            {
                ContentTitle = "Cannot find AdRasta.ini file",
                ContentMessage = "Unable to find :" + Settings.IniFileLocation,
                ButtonDefinitions = new List<ButtonDefinition>
                {
                    new ButtonDefinition { Name = "Okay" },
                },
                ShowInCenter = true, WindowStartupLocation = WindowStartupLocation.CenterOwner
            });

            var result = await messageBox.ShowWindowDialogAsync(this);
            Environment.Exit(-1);
            return false;
        }

        return true;
    }
}