using DynamicData;
using Microsoft.Extensions.Options;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Security;
using Ty.ViewModels;

namespace Ty.Services;

public class MenuService
{
    private readonly MenuOptions _option;
    private readonly PermissionService _permission;

    public MenuService(IOptions<MenuOptions> options, PermissionService permission)
    {
        _option = options.Value;
        foreach (var menu in _option.Menus)
        {
            Menus.AddOrUpdate(menu);
        }

        this._permission = permission;
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

    public IDisposable CreateMenu(string name, ObservableCollection<MenuViewModel> menus, Func<MenuViewModel, Task> reactiveCommand)
    {
        var names = name.Split('.');
        return Menus.Connect().Subscribe(c =>
          {
              foreach (var change in c)
              {
                  if (!change.Current.Name.StartsWith(name))
                  {
                      continue;
                  }

                  var levels = change.Current.Name.Split('.');
                  var parent = GetParent(levels, names.Length+1, menus);

                  switch (change.Reason)
                  {
                      case ChangeReason.Add:
                          parent.Add(new(change.Current, _permission, reactiveCommand));
                          // 处理添加事件
                          break;
                      case ChangeReason.Update:
                      case ChangeReason.Refresh:
                          {
                              var p = parent.FirstOrDefault(v => v.FullName == change.Current.Name);

                              if (p is not null)
                              {
                                  p.DisplayName = change.Current.DisplayName;
                                  p.Icon = change.Current.Icon;
                                  p.Color = change.Current.Color;
                                  p.Enable = change.Current.Enable;
                                  p.Show = change.Current.Show;
                              }
                          }
                          // 处理更新事件
                          break;
                      case ChangeReason.Remove:
                          {
                              var p = parent.FirstOrDefault(v => v.FullName == change.Current.Name);
                              if (p is not null)
                              {
                                  parent.Remove(p);
                              }
                          }

                          // 处理删除事件
                          break;
                          // 其他更改原因
                  }
              }
          });
    }
    public static ObservableCollection<MenuViewModel> GetParent(string[] levels, int level, ObservableCollection<MenuViewModel> list)
    {
        if (levels.Length == level)
        {
            return list;
        }
        else
        {
            var parent = list.FirstOrDefault(x => x.Name == levels[level - 1]);
            if (parent is not null)
            {
                return GetParent(levels, level + 1, parent.Children);
            }
            else
            {
                return list;
            }
        }
    }
}
