using Avalonia;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ReactiveUI;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Ty.Services;
using Ty.ViewModels;

namespace Ty.AvaloniaBase.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow(IHostApplicationLifetime hostApplicationLifetime, IOptions<PageOptions> options, IMessageBoxManager messageBoxManager)
    {
        InitializeComponent();
        var vm = new MainWindowViewModel() { Title = options.Value.Title ?? "" };
        DataContext = vm;
        this.hostApplicationLifetime = hostApplicationLifetime;
        this._messageBoxManager = messageBoxManager;
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
    private readonly IMessageBoxManager _messageBoxManager;

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        if (_messageBoxManager is MessageBoxManager messageBoxManager)
        {
            messageBoxManager.SetNotifyManager(this);
        }
    }

    //protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    //{
    //    base.OnAttachedToVisualTree(e);
    //    if (_messageBoxManager is MessageBoxManager messageBoxManager)
    //    {
    //        messageBoxManager.SetNotifyManager(this);
    //    }
    //}

    protected override void OnClosed(EventArgs e)
    {
        hostApplicationLifetime.StopApplication();
    }

}
