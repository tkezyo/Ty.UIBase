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
        private readonly PermissionService _permissionService;

        public RoutingState Router { get; } = new RoutingState(RxApp.MainThreadScheduler);

        [Reactive]
        public string Title { get; set; }

        public LayoutViewModel(IOptions<MenuOptions> options, IOptions<PageOptions> options1, MenuService menuService, PermissionService permissionService)
        {
            _kHToolOptions = options.Value;
            ShowThemeToggle = _kHToolOptions.ShowThemeToggle;
            UrlPathSegment = "Layout";
            this._menuService = menuService;
            this._permissionService = permissionService;
            Title = options1.Value.Title ?? "测试";

            _menuService.CreateMenu("Menu", Navi, MenuExecute);
        }

        public override async Task Activate()
        {
            if (Navi.Count > 0)
            {
                await MenuExecute(Navi.First(c => c.IsVisible));
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
        public ObservableCollection<MenuViewModel> Navi { get; set; } = [];

        [Reactive]
        public ObservableCollection<MenuViewModel> SubNavi { get; set; } = [];

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

            var parent = MenuService.GetParent(levels, 2, levels[0] switch
            {
                "Tools" => Tools,
                _ => Navi
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
                    SubNavi = current.Children;
                }
            }
        }
    }

    public class MenuViewModel : ReactiveObject
    {
        private readonly ObservableAsPropertyHelper<bool> _isVisible;
        public bool IsVisible => _isVisible.Value;

        public ReactiveCommand<MenuViewModel, Unit> MenuExecuteCommand { get; }

        public MenuViewModel(MenuInfo menuInfo, PermissionService permissionService, Func<MenuViewModel, Task> reactiveCommand)
        {
            Name = menuInfo.Name.Split('.')[^1];
            FullName = menuInfo.Name;
            DisplayName = menuInfo.DisplayName;
            Icon = menuInfo.Icon;
            Enable = menuInfo.Enable;
            Show = menuInfo.Show;
            Color = menuInfo.Color;
            ViewModel = menuInfo.ViewModel;
            GroupName = menuInfo.GroupName;
            MenuExecuteCommand = ReactiveCommand.CreateFromTask(reactiveCommand);



            var hasPermission = (menuInfo.Permissions is null || menuInfo.Permissions.Length == 0)
                    ? Observable.Return(true)
                    : permissionService.HasPermission(menuInfo.Permissions!);
            var show = this.WhenAnyValue(x => x.Show);

            _isVisible = hasPermission.CombineLatest(show, (permission, show) => permission && show)
                .ToProperty(this, x => x.IsVisible);
        }

        /// <summary>
        /// 名称
        /// </summary>
        [Reactive]
        public string Name { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        [Reactive]
        public string GroupName { get; set; }

        [Reactive]
        public string FullName { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        [Reactive]
        public string? DisplayName { get; set; }
        /// <summary>
        /// 图标
        /// </summary>
        [Reactive]
        public string? Icon { get; set; }
        /// <summary>
        /// 启用
        /// </summary>
        [Reactive]
        public bool Enable { get; set; }
        /// <summary>
        /// 显示
        /// </summary>
        [Reactive]
        public bool Show { get; set; }
        [Reactive]
        public Color? Color { get; set; }

        /// <summary>
        /// 是否激活
        /// </summary>
        [Reactive]
        public bool Active { get; set; }

        public Type? ViewModel { get; set; }

        public ObservableCollection<MenuViewModel> Children { get; set; } = [];

    }
}
