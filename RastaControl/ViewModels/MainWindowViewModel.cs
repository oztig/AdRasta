using System.Threading.Tasks;
using Avalonia.Controls;

namespace RastaControl.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public RastaControlViewModel RastaControlViewModel { get; }

    public MainWindowViewModel(Window window)
    {
        RastaControlViewModel = new RastaControlViewModel(window);
    }

    public async Task Initialize(Window window)
    {
        RastaControlViewModel.SetWindow(window);
    }
}