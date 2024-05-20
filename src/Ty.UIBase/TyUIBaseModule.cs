using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveUI;
using Ty.Services;

namespace Ty;

public class TyUIBaseModule : ModuleBase
{
    public override Task ConfigureServices(IHostApplicationBuilder builder)
    {
        RxApp.DefaultExceptionHandler = new MyCoolObservableExceptionHandler();

        builder.Services.AddSingleton<PermissionService>();
        builder.Services.AddSingleton<MenuService>();

        return Task.CompletedTask;
    }
}
