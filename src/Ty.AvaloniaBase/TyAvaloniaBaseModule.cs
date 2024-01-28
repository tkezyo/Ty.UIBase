﻿using Microsoft.Extensions.DependencyInjection;
using Ty.AvaloniaBase.Views;
using Ty.Services;
using Ty.ViewModels;

namespace Ty;

public class TyAvaloniaBaseModule : ModuleBase
{
    public override Task ConfigureServices(IServiceCollection serviceDescriptors)
    {
        serviceDescriptors.AddSingleton<IMessageBoxManager, MessageBoxManager>();

        serviceDescriptors.AddSingletonView<LayoutViewModel, LayoutView>();
        serviceDescriptors.AddTransientView<ModalDialogViewModel, ModalDialog>();
        serviceDescriptors.AddTransientView<PromptDialogViewModel, PromptDialog>();
        serviceDescriptors.AddTransient<MainWindow>();

        return Task.CompletedTask;
    }
}

