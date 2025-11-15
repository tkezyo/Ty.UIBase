using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Windows;
using Ty.ViewModels;

namespace Ty.Views
{
    /// <summary>
    /// PromptDialog.xaml 的交互逻辑
    /// </summary>
    public partial class PromptDialog : HandyControl.Controls.Window, IViewFor<PromptDialogViewModel> 
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
        //
        // 摘要:
        //     The view model dependency property.
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(PromptDialogViewModel), typeof(PromptDialog), new PropertyMetadata(null));

        //
        // 摘要:
        //     Gets the binding root view model.
        public PromptDialogViewModel? BindingRoot => ViewModel;

        public PromptDialogViewModel? ViewModel
        {
            get
            {
                return (PromptDialogViewModel)GetValue(ViewModelProperty);
            }
            set
            {
                SetValue(ViewModelProperty, value);
            }
        }

        object? IViewFor.ViewModel
        {
            get
            {
                return ViewModel;
            }
            set
            {
                ViewModel = (PromptDialogViewModel?)value;
            }
        }
    }
}
