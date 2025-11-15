using ReactiveUI;
using ReactiveUI.Avalonia;
using System.Reactive.Disposables.Fluent;
using Ty.ViewModels;

namespace Ty;

public partial class PromptDialog : ReactiveWindow<PromptDialogViewModel>
{
    public PromptDialog()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            ViewModel!.CloseCommand.Subscribe(c =>
            {
                Close();
            }).DisposeWith(d);
        });
    }
}