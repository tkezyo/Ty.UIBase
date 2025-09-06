using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Ty
{
    public class AvaloniaHostedService<TApplication, TMainWindow> : IHostedService
         where TApplication : Application, new()
         where TMainWindow : Window
    {
        public AvaloniaHostedService(IServiceProvider serviceProvider, IHostApplicationLifetime hostApplicationLifetime)
        {
            TyApp.ServiceProvider = serviceProvider;

         
            this._serviceProvider = serviceProvider;
            this._hostApplicationLifetime = hostApplicationLifetime;
        }

        private readonly IServiceProvider _serviceProvider;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var builder = BuildAvaloniaApp();
            _ = builder.StartWithClassicDesktopLifetime([],c=>
            {
                c.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                c.Exit += (s, e) =>
                {
                    _hostApplicationLifetime.StopApplication();
                };
                _hostApplicationLifetime.ApplicationStopping.Register(() =>
                {
                    c.Shutdown();
                });
            }
            );
            //_classicDesktopStyleApplicationLifetime.MainWindow = _serviceProvider.GetRequiredService<TMainWindow>();

            return Task.CompletedTask;
        }
        public static AppBuilder BuildAvaloniaApp()
          => AppBuilder.Configure<TApplication>()
              .UsePlatformDetect()
              .WithInterFont()
              .LogToTrace()
              .UseReactiveUI();
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
