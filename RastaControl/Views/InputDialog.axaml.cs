using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using RastaControl.ViewModels;

namespace RastaControl.Views;

public partial class InputDialog : Window
{
    private bool? _confirmed;
    private string _input = string.Empty;

    public InputDialog(string title, string defaultInput,string inputWatermark, Window owner)
    {
        InitializeComponent();
        this.Owner = owner;

        var vm = new InputDialogViewModel();
        vm.Title = title;
        vm.UserInput = defaultInput;
        vm.InputWatermark = inputWatermark;
        vm.CloseAction = confirmed =>
        {
            _confirmed = confirmed;
            _input = vm.UserInput;
            Close();
        };

        DataContext = vm;
    }

    public async Task<(bool? confirmed, string value)> GetUserInput(Window owner)
    {
        await ShowDialog(owner);
        return (_confirmed, _input);
    }
}
