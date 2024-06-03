using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveUI;
using System.Drawing;
using Ty.Services;
using Ty.Services.Configs;
using Ty.ViewModels.Configs;
using Ty.ViewModels.CustomPages;

namespace Ty;

public class TyUIBaseModule : ModuleBase
{
    public override Task ConfigureServices(IHostApplicationBuilder builder)
    {
        RxApp.DefaultExceptionHandler = new MyCoolObservableExceptionHandler();

        builder.Services.AddSingleton<PermissionService>();
        builder.Services.AddSingleton<MenuService>();
        builder.Services.AddSingleton<ConfigManager>();
        builder.Services.AddSingleton<ConfigEditViewModel>();

        builder.Services.Configure<MenuOptions>(options =>
        {
            options.Menus.Add(new MenuInfo { DisplayName = "自定义页面", GroupName = "自定义页面", Name = "Menu.自定义页面", ViewModel = typeof(CustomPageViewModel), Color = Color.Black });
            options.Menus.Add(new MenuInfo { DisplayName = "编辑", GroupName = "自定义页面", Name = "Menu.自定义页面.编辑" });
            options.Menus.Add(new MenuInfo { DisplayName = "保存", GroupName = "自定义页面", Name = "Menu.自定义页面.保存" });
            options.Menus.Add(new MenuInfo { DisplayName = "标签", GroupName = "自定义页面", Name = "Menu.自定义页面.标签" });
            options.Menus.Add(new MenuInfo { DisplayName = "返回", GroupName = "自定义页面", Name = "Menu.自定义页面.标签.返回" });
            options.Menus.Add(new MenuInfo { DisplayName = "添加", GroupName = "自定义页面", Name = "Menu.自定义页面.标签.添加" });
            options.Menus.Add(new MenuInfo { DisplayName = "删除", GroupName = "自定义页面", Name = "Menu.自定义页面.标签.删除" });
            options.Menus.Add(new MenuInfo { DisplayName = "前移", GroupName = "自定义页面", Name = "Menu.自定义页面.标签.前移" });
            options.Menus.Add(new MenuInfo { DisplayName = "后移", GroupName = "自定义页面", Name = "Menu.自定义页面.标签.后移" });
            options.Menus.Add(new MenuInfo { DisplayName = "添加盒子", GroupName = "自定义页面", Name = "Menu.自定义页面.添加盒子" });
            options.Menus.Add(new MenuInfo { DisplayName = "返回", GroupName = "自定义页面", Name = "Menu.自定义页面.盒子.返回" });
            options.Menus.Add(new MenuInfo { DisplayName = "删除盒子", GroupName = "自定义页面", Name = "Menu.自定义页面.盒子.删除" });
        });
        return Task.CompletedTask;
    }
}
