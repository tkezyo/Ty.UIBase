using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveUI;

namespace Ty
{
    public class AvaloniaHostedService<TApplication, TMainWindow> : IHostedService
         where TApplication : Application, new()
         where TMainWindow : Window
    {
        public AvaloniaHostedService(IServiceProvider serviceProvider, IHostApplicationLifetime hostApplicationLifetime)
        {
            TyApp.ServiceProvider = serviceProvider;

            _classicDesktopStyleApplicationLifetime = new ClassicDesktopStyleApplicationLifetime
            {
                ShutdownMode = ShutdownMode.OnExplicitShutdown
            };
            _classicDesktopStyleApplicationLifetime.Exit += (s, e) =>
            {
                hostApplicationLifetime.StopApplication();
            };
            hostApplicationLifetime.ApplicationStopping.Register(() =>
            {
                _classicDesktopStyleApplicationLifetime.Shutdown();
            });
            this._serviceProvider = serviceProvider;
        }

        private readonly IServiceProvider _serviceProvider;
        private ClassicDesktopStyleApplicationLifetime _classicDesktopStyleApplicationLifetime;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var builder = BuildAvaloniaApp();
            _ = builder.SetupWithLifetime(_classicDesktopStyleApplicationLifetime);
            _classicDesktopStyleApplicationLifetime.MainWindow = _serviceProvider.GetRequiredService<TMainWindow>();
            _classicDesktopStyleApplicationLifetime.Start([]);

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
