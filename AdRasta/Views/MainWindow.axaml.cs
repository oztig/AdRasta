using AdRasta.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;

namespace AdRasta.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        Activated += async (_, _) =>
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                await viewModel.Initialize(this);
                await viewModel.RastaControlViewModel.CheckInitialSetup();
            }
        };
    }
}