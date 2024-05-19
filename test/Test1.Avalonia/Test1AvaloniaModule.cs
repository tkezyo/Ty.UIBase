using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Threading.Tasks;
using Test1.Avalonia.ViewModels;
using Ty;
using Ty.AvaloniaBase.Views;

namespace Test1.Avalonia
{
    public class Test1AvaloniaModule : ModuleBase
    {
        public override void DependsOn()
        {
            AddDepend<Ty.TyAvaloniaBaseModule>();
        }
        public override Task ConfigureServices(IHostApplicationBuilder hostApplicationBuilder)
        {
            hostApplicationBuilder.Services.AddSingleton<App>();
            hostApplicationBuilder.Services.AddTransient<MainWindow>();
            hostApplicationBuilder.Services.AddHostedService<AvaloniaHostedService<App, MainWindow>>();
            hostApplicationBuilder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

            //serviceDescriptors.AddTransientCustomPageView<ChatViewModel, ChatView>();
            //serviceDescriptors.AddCustomLogView<CustomLogViewModel, CustomLogView>();
            hostApplicationBuilder.Services.AddSingletonView<TestViewModel, TestView>();


            hostApplicationBuilder.Services.Configure<PageOptions>(options =>
            {
                options.FirstLoadPage = typeof(TestViewModel);
                options.Title = "测试";
            });


            return Task.CompletedTask;
        }
    }
}
