using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Reactive.Linq;
using DynamicData;
using Microsoft.Extensions.Options;

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
        }
    }

    public void ChangeShow(string name, bool show)
    {
        var menu = Menus.Lookup(name);
        if (menu.HasValue)
        {
            menu.Value.Show = show;
        }
    }

    public void ChangeColor(string name, Color color)
    {
        var menu = Menus.Lookup(name);
        if (menu.HasValue)
        {
            menu.Value.Color = color;
        }
    }

    public void ChangeIcon(string name, string icon)
    {
        var menu = Menus.Lookup(name);
        if (menu.HasValue)
        {
            menu.Value.Icon = icon;
        }
    }

    public void ChangeDisplayName(string name, string displayName)
    {
        var menu = Menus.Lookup(name);
        if (menu.HasValue)
        {
            menu.Value.DisplayName = displayName;
        }
    }

    public void ChangeViewModel(string name, Type? viewModel)
    {
        var menu = Menus.Lookup(name);
        if (menu.HasValue)
        {
            menu.Value.ViewModel = viewModel;
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
public class MenuViewModel : ReactiveObject
{
    public MenuViewModel(string name,
        IObservable<MenuInfo> changeEnable,
         Type? viewModel = null
        )
    {
        Name = name;
        ViewModel = viewModel;

        changeEnable.Select(c => c.Enable).ToPropertyEx(this, x => x.Enable);
        changeEnable.Select(c => c.Show).ToPropertyEx(this, x => x.Show);
        changeEnable.Select(c => c.Color)?.ToPropertyEx(this, x => x.Color);
        changeEnable.Select(c => c.Icon)?.ToPropertyEx(this, x => x.Icon);
        changeEnable.Select(c => c.DisplayName)?.ToPropertyEx(this, x => x.DisplayName);
    }


    /// <summary>
    /// 名称
    /// </summary>
    [Reactive]
    public string Name { get; set; }

    /// <summary>
    /// 显示名称
    /// </summary>
    [ObservableAsProperty]
    public string? DisplayName { get; }
    /// <summary>
    /// 图标
    /// </summary>
    [ObservableAsProperty]
    public string? Icon { get; }
    /// <summary>
    /// 启用
    /// </summary>
    [ObservableAsProperty]
    public bool Enable { get; }
    /// <summary>
    /// 显示
    /// </summary>
    [ObservableAsProperty]
    public bool Show { get; }
    [ObservableAsProperty]
    public Color Color { get; } = Color.Gray;

    public Type? ViewModel { get; set; }

    public ObservableCollection<MenuViewModel> Children { get; set; } = [];

}