using ReactiveUI;
using System;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Windows.Input;
using Ty.ViewModels.CustomPages;

namespace Ty.Views.CustomPages
{
    /// <summary>
    /// CustomPageView.xaml 的交互逻辑
    /// </summary>
    public partial class CustomPageView : ReactiveUserControl<CustomPageViewModel>
    {
        public CustomPageView()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(c => KeyDown += c, c => KeyDown -= c).Subscribe(c =>
                {
                    if (ViewModel is null)
                    {
                        return;
                    }

                    if (c.EventArgs.KeyboardDevice.Modifiers == ModifierKeys.Control)
                    {
                        switch (c.EventArgs.Key)
                        {
                            case Key.W:
                                ViewModel.AddHeightCommand.Execute(false).Subscribe();
                                break;
                            case Key.S:
                                ViewModel.AddHeightCommand.Execute(true).Subscribe();
                                break;
                            case Key.A:
                                ViewModel.AddWidthCommand.Execute(false).Subscribe();
                                break;
                            case Key.D:
                                ViewModel.AddWidthCommand.Execute(true).Subscribe();
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        switch (c.EventArgs.Key)
                        {
                            case Key.W:
                                ViewModel.TopCommand.Execute(false).Subscribe();
                                break;
                            case Key.S:
                                ViewModel.TopCommand.Execute(true).Subscribe();
                                break;
                            case Key.A:
                                ViewModel.LeftCommand.Execute(false).Subscribe();
                                break;
                            case Key.D:
                                ViewModel.LeftCommand.Execute(true).Subscribe();
                                break;
                            default:
                                break;
                        }
                    }

                }).DisposeWith(d);
            });
        }
    }
}
