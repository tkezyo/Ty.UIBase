﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ReactiveUI;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using Ty.ViewModels;

namespace Ty.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : HandyControl.Controls.Window, IViewFor<MainWindowViewModel>
    {
        public MainWindow(IHostApplicationLifetime hostApplicationLifetime, IOptions<PageOptions> options)
        {
            InitializeComponent();
            var vm = new MainWindowViewModel() { Title = options.Value.Title ?? "" };
            DataContext = vm;

            if (options.Value.Loading is not null)
            {
                host.DefaultContent = options.Value.Loading;
            }
            else
            {
                TextBlock textBlock = new TextBlock();
                textBlock.Text = "Loading...";
                textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                textBlock.VerticalAlignment = VerticalAlignment.Center;
                host.DefaultContent = textBlock;
            }

            this.hostApplicationLifetime = hostApplicationLifetime;
            if (options.Value.Hight.HasValue)
            {
                Height = options.Value.Hight.Value;
            }
            if (options.Value.Width.HasValue)
            {
                Width = options.Value.Width.Value;
            }

            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                if (options.Value.FirstLoadPage is null)
                {
                    return;
                }
                var login = TyApp.ServiceProvider.GetRequiredService(options.Value.FirstLoadPage);
                if (login is ITyRoutableViewModel routableViewModel)
                {
                    routableViewModel.SetScreen(vm);
                    await vm.Router.Navigate.Execute(routableViewModel);
                }

            });
        }

        private readonly IHostApplicationLifetime hostApplicationLifetime;


        protected override void OnClosed(EventArgs e)
        {
            hostApplicationLifetime.StopApplication();
        }

        //
        // 摘要:
        //     The view model dependency property.
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(MainWindowViewModel), typeof(MainWindow), new PropertyMetadata(null));

        //
        // 摘要:
        //     Gets the binding root view model.
        public MainWindowViewModel? BindingRoot => ViewModel;

        public MainWindowViewModel? ViewModel
        {
            get
            {
                return (MainWindowViewModel)GetValue(ViewModelProperty);
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
                ViewModel = (MainWindowViewModel?)value;
            }
        }

    }
}
