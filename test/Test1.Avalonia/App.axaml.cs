using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Ty;
using Ty.AvaloniaBase.Views;

namespace Test1.Avalonia
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            // Create and assign the MainWindow after Avalonia platform is fully initialized
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                try
                {
                    var mainWindow = TyApp.ServiceProvider.GetRequiredService<MainWindow>();
                    desktop.MainWindow = mainWindow;

                    // Ensure the Avalonia app exits when host requests stop
                    var hostAppLifetime = TyApp.ServiceProvider.GetService<IHostApplicationLifetime>();
                    hostAppLifetime?.ApplicationStopping.Register(() => desktop.Shutdown());
                }
                catch (Exception ex)
                {
                    // Surface errors instead of swallowing silently
                    System.Diagnostics.Trace.TraceError($"Failed to create MainWindow: {ex}");
                    throw;
                }
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}