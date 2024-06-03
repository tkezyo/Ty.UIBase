using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Ty.ViewModels;
using Ty.ViewModels.CustomPages;

namespace Ty;

public static class TyViewModelExtension
{
    public static void AddSingletonView<TViewModel, TView>(this IServiceCollection services)
        where TView : class, IViewFor<TViewModel>
        where TViewModel : class, ITyRoutableViewModel
    {
        services.AddSingleton<TViewModel>();
        services.AddKeyedTransient<IViewFor, TView>(typeof(TViewModel).FullName);
    }
    public static void AddTransientView<TViewModel, TView>(this IServiceCollection services)
        where TView : class, IViewFor<TViewModel>
        where TViewModel : class, ITyRoutableViewModel
    {
        services.AddTransient<TViewModel>();
        services.AddKeyedTransient<IViewFor, TView>(typeof(TViewModel).FullName);
    }

    public static void AddSingletonCustomPageView<TViewModel, TView>(this IServiceCollection serviceDescriptors)
       where TView : class, IViewFor<TViewModel>
       where TViewModel : class, ICustomPageViewModel
    {
        serviceDescriptors.AddKeyedSingleton<ICustomPageInjectViewModel, TViewModel>(TViewModel.Category + ":" + TViewModel.Name);
        serviceDescriptors.AddKeyedTransient<IViewFor, TView>(typeof(TViewModel).FullName);
        serviceDescriptors.Configure<CustomPageOption>(c =>
        {
            var group = c.GetOrAddGroup(TViewModel.Category);

            group.Add(TViewModel.GetDefinition());
        });
    }
    public static void AddTransientCustomPageView<TViewModel, TView>(this IServiceCollection serviceDescriptors)
        where TView : class, IViewFor<TViewModel>
        where TViewModel : class, ICustomPageViewModel
    {
        serviceDescriptors.AddKeyedTransient<ICustomPageInjectViewModel, TViewModel>(TViewModel.Category + ":" + TViewModel.Name);
        serviceDescriptors.AddKeyedTransient<IViewFor, TView>(typeof(TViewModel).FullName);
        serviceDescriptors.AddTransientView<TViewModel, TView>();

        serviceDescriptors.Configure<CustomPageOption>(c =>
        {
            var group = c.GetOrAddGroup(TViewModel.Category);

            group.Add(TViewModel.GetDefinition());
        });
    }
}
