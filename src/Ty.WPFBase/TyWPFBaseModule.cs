using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveUI;
using System.Threading.Tasks;
using Ty.Services;
using Ty.ViewModels;
using Ty.Views;

namespace Ty
{
    public class TyWPFBaseModule : ModuleBase
    {
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
}
