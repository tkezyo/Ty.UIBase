using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Ty;

namespace Test1.WPF
{
    class Program
    {
        public static async Task Main(string[] args)
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

            var host = await ApplicationHostBuilder.CreateApplicationHost<Test1UIModule>(args, dependsOnFolder: "dlls", skipVerification: true) ?? throw new Exception();

            Thread thread = new(async () =>
            {
                await host.RunAsync();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

        }
    }
}
