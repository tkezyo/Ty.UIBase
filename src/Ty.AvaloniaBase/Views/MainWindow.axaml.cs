using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ReactiveUI;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Ty.ViewModels;

namespace Ty.AvaloniaBase.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow(IHostApplicationLifetime hostApplicationLifetime, IOptions<PageOptions> options)
    {
        InitializeComponent();
        var vm = new MainWindowViewModel() { Title = options.Value.Title ?? "" };
        DataContext = vm;
        this.hostApplicationLifetime = hostApplicationLifetime;

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

}
