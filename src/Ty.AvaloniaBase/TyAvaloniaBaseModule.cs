using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveUI;
using Ty.AvaloniaBase.Views;
using Ty.Services;
using Ty.ViewModels;

namespace Ty;

public class TyAvaloniaBaseModule : ModuleBase
{
    public override void DependsOn()
    {
        AddDepend<Ty.TyUIBaseModule>();
    }
    public override Task ConfigureServices(IHostApplicationBuilder builder)
    {
        RxApp.DefaultExceptionHandler = new MyCoolObservableExceptionHandler();
        builder.Services.AddSingleton<IMessageBoxManager, MessageBoxManager>();

        builder.Services.AddSingletonView<LayoutViewModel, LayoutView>();
        builder.Services.AddTransientView<ModalDialogViewModel, ModalDialog>();
        builder.Services.AddTransientView<PromptDialogViewModel, PromptDialog>();
        builder.Services.AddTransient<MainWindow>();

        return Task.CompletedTask;
    }
}

