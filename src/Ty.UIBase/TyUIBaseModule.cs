using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveUI;
using Ty.Services;
using Ty.Services.Configs;
using Ty.ViewModels.Configs;

namespace Ty;

public class TyUIBaseModule : ModuleBase
{
    public override Task ConfigureServices(IHostApplicationBuilder builder)
    {
        RxApp.DefaultExceptionHandler = new MyCoolObservableExceptionHandler();

        builder.Services.AddSingleton<PermissionService>();
        builder.Services.AddSingleton<MenuService>();
        builder.Services.AddSingleton<ConfigManager>();
        builder.Services.AddSingleton<ConfigEditViewModel>();

        return Task.CompletedTask;
    }
}
