using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Reactive.Linq;
using DynamicData;
using Microsoft.Extensions.Options;
using System.Security.Principal;

namespace Ty.Services;

public class MenuService
{
    private readonly MenuOptions _option;
    public MenuService(IOptions<MenuOptions> options)
    {
        _option = options.Value;
        foreach (var menu in _option.Menus)
        {
            Menus.AddOrUpdate(menu);
        }
    }
    public SourceCache<MenuInfo, string> Menus { get; set; } = new(c => c.Name);

    public void ChangeEnable(string name, bool enable)
    {
        var menu = Menus.Lookup(name);
        if (menu.HasValue)
        {
            menu.Value.Enable = enable;
            Menus.AddOrUpdate(menu.Value);
        }
    }

    public void ChangeShow(string name, bool show)
    {
        var menu = Menus.Lookup(name);
        if (menu.HasValue)
        {
            menu.Value.Show = show;
            Menus.AddOrUpdate(menu.Value);
        }
    }

    public void ChangeColor(string name, Color color)
    {
        var menu = Menus.Lookup(name);
        if (menu.HasValue)
        {
            menu.Value.Color = color;
            Menus.AddOrUpdate(menu.Value);
        }
    }

    public void ChangeIcon(string name, string icon)
    {
        var menu = Menus.Lookup(name);
        if (menu.HasValue)
        {
            menu.Value.Icon = icon;
            Menus.AddOrUpdate(menu.Value);
        }
    }

    public void ChangeDisplayName(string name, string displayName)
    {
        var menu = Menus.Lookup(name);
        if (menu.HasValue)
        {
            menu.Value.DisplayName = displayName;
            Menus.AddOrUpdate(menu.Value);
        }
    }

    public void ChangeViewModel(string name, Type? viewModel)
    {
        var menu = Menus.Lookup(name);
        if (menu.HasValue)
        {
            menu.Value.ViewModel = viewModel;
            Menus.AddOrUpdate(menu.Value);
        }
    }

    public void AddOrUpdate(MenuInfo menu)
    {
        Menus.AddOrUpdate(menu);
    }

    public void Remove(string name)
    {
        Menus.Remove(name);
    }

    public void Clear()
    {
        Menus.Clear();
    }

}
public class MenuViewModel(MenuInfo menuInfo) : ReactiveObject
{
    /// <summary>
    /// 名称
    /// </summary>
    [Reactive]
    public string Name { get; set; } = menuInfo.Name;

    /// <summary>
    /// 显示名称
    /// </summary>
    [Reactive]
    public string? DisplayName { get; set; } = menuInfo.DisplayName;
    /// <summary>
    /// 图标
    /// </summary>
    [Reactive]
    public string? Icon { get; set; } = menuInfo.Icon;
    /// <summary>
    /// 启用
    /// </summary>
    [Reactive]
    public bool Enable { get; set; } = menuInfo.Enable;
    /// <summary>
    /// 显示
    /// </summary>
    [Reactive]
    public bool Show { get; set; } = menuInfo.Show;
    [Reactive]
    public Color? Color { get; set; } = menuInfo.Color;

    /// <summary>
    /// 是否激活
    /// </summary>
    [Reactive]
    public bool Active { get; set; }

    public Type? ViewModel { get; set; } = menuInfo.ViewModel;

    public ObservableCollection<MenuViewModel> Children { get; set; } = [];

}