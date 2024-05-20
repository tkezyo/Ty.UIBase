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
                var parent = list.FirstOrDefault(x => x.Name == levels[level]);
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

        public LayoutViewModel(IOptions<MenuOptions> options, MenuService menuService)
        {
            _kHToolOptions = options.Value;
            ShowThemeToggle = _kHToolOptions.ShowThemeToggle;
            MenuExecuteCommand = ReactiveCommand.CreateFromTask<MenuViewModel>(MenuExecute);
            UrlPathSegment = "Layout";
            this._menuService = menuService;

            _menuService.Menus.Connect().Subscribe(c =>
            {
                foreach (var change in c)
                {
                    var parent = GetParent(change.Current.Name.Split('.'), 1, Menus);

                    switch (change.Reason)
                    {
                        case ChangeReason.Add:
                            parent.Add(new(change.Current));
                            // 处理添加事件
                            break;
                        case ChangeReason.Update:
                        case ChangeReason.Refresh:
                            {
                                var p = parent.FirstOrDefault(v => v.Name == change.Current.Name);

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
                                var p = parent.FirstOrDefault(v => v.Name == change.Current.Name);
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

        public ReactiveCommand<MenuViewModel, Unit> MenuExecuteCommand { get; }
        public virtual async Task MenuExecute(MenuViewModel menu)
        {
            await Task.CompletedTask;
            MessageBus.Current.SendMessage(menu, "Menu");
            if (menu.ViewModel is not null)
            {
                var vm = Navigate(menu.ViewModel, this);
                await Router.Navigate.Execute(vm);
            }

            var levels = menu.Name.Split('.');

            //取消所有选中
            foreach (var item in Menus)
            {
                item.Active = false;
            }

            var parent = GetParent(levels, 1, Menus);
            var current = parent.FirstOrDefault(x => x.Name == menu.Name);
            if (current is not null)
            {
                current.Active = true;
            }

        }
    }
}
