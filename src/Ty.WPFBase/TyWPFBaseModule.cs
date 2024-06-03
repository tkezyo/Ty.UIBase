using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveUI;
using System.Drawing;
using System.Threading.Tasks;
using Ty.Services;
using Ty.ViewModels;
using Ty.ViewModels.CustomPages;
using Ty.Views;
using Ty.Views.CustomPages;

namespace Ty
{
    public class TyWPFBaseModule : ModuleBase
    {
        public override void DependsOn()
        {
            AddDepend<TyUIBaseModule>();
        }
        public override Task ConfigureServices(IHostApplicationBuilder builder)
        {
            RxApp.DefaultExceptionHandler = new MyCoolObservableExceptionHandler();
            builder.Services.AddSingleton<IMessageBoxManager, MessageBoxManager>();

            builder.Services.AddSingletonView<LayoutViewModel, LayoutView>();
            builder.Services.AddTransientView<ModalDialogViewModel, ModalDialog>();
            builder.Services.AddTransientView<PromptDialogViewModel, PromptDialog>();
            builder.Services.AddTransientView<CustomPageViewModel, CustomPageView>();
            builder.Services.AddTransient<MainWindow>();

        
            return Task.CompletedTask;
        }
    }
}
