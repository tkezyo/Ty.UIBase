using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System.Threading.Tasks;
using Ty.Services;
using Ty.ViewModels;
using Ty.Views;

namespace Ty
{
    public class TyWPFBaseModule : ModuleBase
    {
        public override Task ConfigureServices(IServiceCollection serviceDescriptors)
        {
            RxApp.DefaultExceptionHandler = new MyCoolObservableExceptionHandler();
            serviceDescriptors.AddSingleton<IMessageBoxManager, MessageBoxManager>();

            serviceDescriptors.AddSingletonView<LayoutViewModel, LayoutView>();
            serviceDescriptors.AddTransientView<ModalDialogViewModel, ModalDialog>();
            serviceDescriptors.AddTransientView<PromptDialogViewModel, PromptDialog>();
            serviceDescriptors.AddTransient<MainWindow>();

            return Task.CompletedTask;
        }
    }
}
