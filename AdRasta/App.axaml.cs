using AdRasta.ViewModels;
using AdRasta.Views;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace AdRasta;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var mainWindow = new MainWindow();
        mainWindow.DataContext = new MainWindowViewModel(mainWindow); // Pass window directly
        mainWindow.Show();

        base.OnFrameworkInitializationCompleted();
    }
}