using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Ty;
using Ty.Views;

namespace Test1.WPF
{
    public class Test1UIModule : ModuleBase
    {
        public override Task ConfigureServices(IHostApplicationBuilder builder)
        {
            builder.Services.AddSingleton<App>();
            builder.Services.AddHostedService<WpfHostedService<App, MainWindow>>();
            builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

            builder.Services.Configure<PageOptions>(options =>
            {
                options.Title = "配置编辑器";
            });

            return Task.CompletedTask;
        }

        public override void DependsOn()
        {
            AddDepend<TyWPFBaseModule>();
            AddDepend<TyUIBaseModule>();
            AddDepend<TyWPFBaseModule>();
        }
    }
}
