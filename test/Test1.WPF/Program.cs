using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Ty;
using Ty.Views;

namespace Test1.WPF
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var configuration = new LoggerConfiguration()
#if DEBUG
           .MinimumLevel.Debug()
#else
            .MinimumLevel.Information()
#endif
           .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
           //输出到文件
           .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
           .Enrich.FromLogContext();

            Log.Logger = configuration.CreateLogger();

            // Build and start host via common bootstrap (blockingly wait on the task so we stay on the STA thread)
            var host = UiBootstrap.BuildAndStartHost<Test1UIModule>(args, dependsOnFolder: "dlls", skipVerification: true).GetAwaiter().GetResult();

            try
            {
                // Resolve and run the WPF application on the STA main thread; App will create and show MainWindow in OnStartup
                var app = host.Services.GetRequiredService<App>();
                app.Run();
            }
            finally
            {
                // Ensure host is stopped and disposed after UI exits
                host.StopAsync().GetAwaiter().GetResult();
                (host as IDisposable)?.Dispose();
            }
        }
    }
}
