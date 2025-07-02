using System.Collections.Generic;
using System.Threading.Tasks;
using AdRasta.Views;
using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Models;

namespace AdRasta.Services;

public static class DialogService
{
    public static async Task<(bool? confirmed, string value)> ShowInputDialogAsync(string title, string defaultInput,string inputWatermark, Window owner)
    {
        var dialog = new InputDialog(title, defaultInput,inputWatermark, owner)
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        return await dialog.GetUserInput(owner);
    }

    public static async Task<string> ShowYesNo(string title, string message, Window window)
    {
            var messageBox = MessageBoxManager.GetMessageBoxCustom(new MessageBoxCustomParams
            {
                ContentTitle = title,
                ContentMessage = message,
                ButtonDefinitions = new List<ButtonDefinition>
                {
                    new ButtonDefinition { Name = "Yes" },
                    new ButtonDefinition { Name = "No" }
                },
                ShowInCenter = true, WindowStartupLocation = WindowStartupLocation.CenterOwner
            });

            var result = await messageBox.ShowWindowDialogAsync(window);
            return result;
    }
}