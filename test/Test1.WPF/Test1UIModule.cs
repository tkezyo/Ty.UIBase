using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Drawing;
using Ty;
using Ty.ViewModels;
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
                options.FirstLoadPage = typeof(LayoutViewModel);
                options.Title = "配置编辑器";
            });
            builder.Services.Configure<MenuOptions>(options =>
            {
                options.Menus.Add(new MenuInfo { DisplayName = "123", GroupName = "123", Name = "Menu.345",Color = Color.Black});
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
