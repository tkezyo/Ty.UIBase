using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveUI;
using System.Threading.Tasks;
using Ty.Services;
using Ty.ViewModels;
using Ty.Views;

namespace Ty
{
    public class TyWPFBaseModule : ModuleBase
    {
        public override void DependsOn()
        {
            AddDepend<TyUIBaseModule>();
        }
        public override Task ConfigureServices(IHostApplicationBuilder builder)
        {
            RxApp.DefaultExceptionHandler = new MyCoolObservableExceptionHandler();
            builder.Services.AddSingleton<IMessageBoxManager, MessageBoxManager>();

            builder.Services.AddSingletonView<LayoutViewModel, LayoutView>();
            builder.Services.AddTransientView<ModalDialogViewModel, ModalDialog>();
            builder.Services.AddTransientView<PromptDialogViewModel, PromptDialog>();
            builder.Services.AddTransient<MainWindow>();

            builder.Services.Configure<MenuOptions>(options =>
            {
                options.Menus.Add(new MenuInfo { DisplayName = "自定义页面", GroupName = "自定义页面", Name = "Menu.自定义页面" });
                options.Menus.Add(new MenuInfo { DisplayName = "编辑", GroupName = "自定义页面", Name = "Menu.自定义页面.编辑" });
                options.Menus.Add(new MenuInfo { DisplayName = "保存", GroupName = "自定义页面", Name = "Menu.自定义页面.保存" });
                options.Menus.Add(new MenuInfo { DisplayName = "标签", GroupName = "自定义页面", Name = "Menu.自定义页面.标签" });
                options.Menus.Add(new MenuInfo { DisplayName = "添加", GroupName = "自定义页面", Name = "Menu.自定义页面.标签.添加" });
                options.Menus.Add(new MenuInfo { DisplayName = "删除", GroupName = "自定义页面", Name = "Menu.自定义页面.标签.删除" });
                options.Menus.Add(new MenuInfo { DisplayName = "前移", GroupName = "自定义页面", Name = "Menu.自定义页面.标签.前移" });
                options.Menus.Add(new MenuInfo { DisplayName = "后移", GroupName = "自定义页面", Name = "Menu.自定义页面.标签.后移" });


            });
            return Task.CompletedTask;
        }
    }
}
