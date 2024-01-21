using Avalonia.ReactiveUI;
using ReactiveUI;
using Ty.ViewModels;

namespace Ty.AvaloniaBase.Views;

public partial class ModalDialog : ReactiveWindow<ModalDialogViewModel>
{
    public ModalDialog()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            ViewModel!.Navigate();
            ViewModel!.ModalViewModel!.CloseCommand.Subscribe(c =>
            {
                Close();
            });
        });
    }


}