using DynamicData;
using Microsoft.Extensions.Options;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Reactive;
using System.Reactive.Linq;
using Ty.Services;

namespace Ty.ViewModels
{
    public class LayoutViewModel : ViewModelBase, IScreen
    {
        protected readonly MenuOptions _kHToolOptions;
        private readonly MenuService _menuService;

        public RoutingState Router { get; } = new RoutingState(RxApp.MainThreadScheduler);

        public ObservableCollection<MenuViewModel> GetParent(string[] levels, int level, ObservableCollection<MenuViewModel> list)
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

        [Reactive]
        public string Title { get; set; }

        public LayoutViewModel(IOptions<MenuOptions> options, IOptions<PageOptions> options1, MenuService menuService)
        {
            _kHToolOptions = options.Value;
            ShowThemeToggle = _kHToolOptions.ShowThemeToggle;
            MenuExecuteCommand = ReactiveCommand.CreateFromTask<MenuViewModel>(MenuExecute);
            UrlPathSegment = "Layout";
            this._menuService = menuService;
            Title = options1.Value.Title ?? "测试";

            _menuService.Menus.Connect().Subscribe(c =>
            {
                foreach (var change in c)
                {
                    var levels = change.Current.Name.Split('.');
                    var parent = GetParent(levels, 2, levels[0] switch
                    {
                        "Tools" => Tools,
                        _ => Menus
                    });

                    switch (change.Reason)
                    {
                        case ChangeReason.Add:
                            parent.Add(new(change.Current));
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

        public override async Task Activate()
        {
            if (Menus.Count > 0)
            {
                await MenuExecute(Menus.First());
            }
        }

        /// <summary>
        /// 显示切换主题按钮
        /// </summary>
        [Reactive]
        public bool ShowThemeToggle { get; set; }
        /// <summary>
        /// 工具栏
        /// </summary>
        [Reactive]
        public ObservableCollection<MenuViewModel> Tools { get; set; } = [];

        [Reactive]
        public ObservableCollection<MenuViewModel> Menus { get; set; } = [];

        [Reactive]
        public ObservableCollection<MenuViewModel> SubMenus { get; set; } = [];

        public ReactiveCommand<MenuViewModel, Unit> MenuExecuteCommand { get; }
        public virtual async Task MenuExecute(MenuViewModel menu)
        {
            await Task.CompletedTask;
            MessageBus.Current.SendMessage(menu, "Menu");
            if (menu.ViewModel is not null)
            {
                var vm = Navigate(menu.ViewModel, this);
                vm.UrlPathSegment = menu.FullName;
                await Router.Navigate.Execute(vm);
            }

            var levels = menu.FullName.Split('.');

            var parent = GetParent(levels, 2, levels[0] switch
            {
                "Tools" => Tools,
                _ => Menus
            });
            var current = parent.FirstOrDefault(x => x.Name == menu.Name);
            if (current is not null)
            {
                //取消所有选中
                foreach (var item in parent)
                {
                    item.Active = false;
                }
                current.Active = true;
                if (levels.Length == 2)
                {
                    SubMenus = current.Children;
                }
            }


        }
    }

    public class MenuViewModel(MenuInfo menuInfo) : ReactiveObject
    {
        /// <summary>
        /// 名称
        /// </summary>
        [Reactive]
        public string Name { get; set; } = menuInfo.Name.Split('.')[^1];

        [Reactive]
        public string FullName { get; set; } = menuInfo.Name;

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
}
