using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Threading.Tasks;
using Test1.Avalonia.ViewModels;
using Ty;
using Ty.AvaloniaBase.Views;
using Ty.ViewModels;

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
            hostApplicationBuilder.Services.AddSingletonView<LayoutViewModel, LayoutView>();


            hostApplicationBuilder.Services.Configure<PageOptions>(options =>
            {
                options.FirstLoadPage = typeof(LayoutViewModel);
                options.Title = "测试";

            });

            hostApplicationBuilder.Services.Configure<MenuOptions>(options =>
            {
                options.Menus.Add(new MenuInfo
                {
                    DisplayName = "测试",
                    GroupName = "测试",
                    Name = "测试",
                    Icon = "Icon",
                    ViewModel = typeof(TestViewModel)
                });

            });




            return Task.CompletedTask;
        }
    }
}
