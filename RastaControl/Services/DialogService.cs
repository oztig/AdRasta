using System.Threading.Tasks;
using Avalonia.Controls;
using RastaControl.Views;

namespace RastaControl.Services;

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
}