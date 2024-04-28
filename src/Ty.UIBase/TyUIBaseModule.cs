using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveUI;
using System.Threading.Tasks;
using Ty.Services;
using Ty.ViewModels;

namespace Ty
{
    public class TyUIBaseModule : ModuleBase
    {
        public override Task ConfigureServices(IHostApplicationBuilder builder)
        {
            RxApp.DefaultExceptionHandler = new MyCoolObservableExceptionHandler();
          

            return Task.CompletedTask;
        }
    }
}
