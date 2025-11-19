using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveUI.Avalonia;
using Serilog;
using Serilog.Events;
using System;
using System.Threading.Tasks;
using Ty;
using Ty.AvaloniaBase.Views;

namespace Test1.Avalonia
{
    internal sealed class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static async Task Main(string[] args)
        {
            var configuration = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Debug()
#else
            .MinimumLevel.Information()
#endif
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext();

            Log.Logger = configuration.CreateLogger();

            // Build and start host via common bootstrap
            var host = await UiBootstrap.BuildAndStartHost<Test1AvaloniaModule>(args, dependsOnFolder: "dlls", skipVerification: true);

            var builder = AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace()
                .UseReactiveUI();

            // Start Avalonia desktop lifetime without constructing windows here.
            builder.StartWithClassicDesktopLifetime(args);

            // Stop the host once the Avalonia application exits.
            await host.StopAsync();
        }
    }
}
