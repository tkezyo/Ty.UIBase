using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Windows;
using Ty.ViewModels;

namespace Ty.Views
{
    /// <summary>
    /// ModalDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ModalDialog : HandyControl.Controls.Window, IViewFor<ModalDialogViewModel>
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
                }).DisposeWith(d);
            });
        }

        //
        // 摘要:
        //     The view model dependency property.
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(ModalDialogViewModel), typeof(ModalDialog), new PropertyMetadata(null));

        //
        // 摘要:
        //     Gets the binding root view model.
        public ModalDialogViewModel? BindingRoot => ViewModel;

        public ModalDialogViewModel? ViewModel
        {
            get
            {
                return (ModalDialogViewModel)GetValue(ViewModelProperty);
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
                ViewModel = (ModalDialogViewModel?)value;
            }
        }
    }
}
