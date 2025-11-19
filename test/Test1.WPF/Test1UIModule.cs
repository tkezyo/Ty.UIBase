using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Drawing;
using Test1.WPF.Models;
using Test1.WPF.ViewModels;
using Test1.WPF.Views;
using Ty;
using Ty.Configs;
using Ty.ViewModels;
using Ty.Views;

namespace Test1.WPF
{
    public class Test1UIModule : ModuleBase
    {
        public override Task ConfigureServices(IHostApplicationBuilder builder)
        {
            builder.Services.AddSingleton<App>();
            //builder.Services.AddHostedService<WpfHostedService<App, MainWindow>>();
            builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

            builder.Services.AddTransientView<ConfigTestViewModel, ConfigTestView>();
            builder.Services.AddTransientCustomPageView<CustomPage1ViewModel, CustomPage1View>();

            builder.Services.AddConfigOptionProvider<StringProvider>();

            builder.Services.Configure<PageOptions>(options =>
            {
                options.FirstLoadPage = typeof(LayoutViewModel);
                options.Title = "配置编辑器";
            });
            builder.Services.Configure<MenuOptions>(options =>
            {
                options.Menus.Add(new MenuInfo { DisplayName = "ttt", GroupName = "ttt", Name = "Menu.345", Color = Color.Black, ViewModel = typeof(ConfigTestViewModel) });
                options.Menus.Add(new MenuInfo { DisplayName = "123", GroupName = "123", Name = "Menu.123", Color = Color.Black });
                options.Menus.Add(new MenuInfo { DisplayName = "345", GroupName = "345", Name = "Menu.123.345" });
                options.Menus.Add(new MenuInfo { DisplayName = "789", GroupName = "789", Name = "Menu.123.345.789", ViewModel = typeof(ConfigTestViewModel) });

            });
            builder.Services.Configure<CustomPageOption>(options =>
            {
                options.RootPath = "D:\\CustomPage";
                options.Name = "name";
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
