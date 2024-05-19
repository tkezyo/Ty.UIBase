using DynamicData;
using Microsoft.Extensions.Options;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Reactive;
using System.Reactive.Linq;

namespace Ty.ViewModels
{
    public class LayoutViewModel : ViewModelBase, IScreen
    {
        protected readonly ToolOptions _kHToolOptions;

        public RoutingState Router { get; } = new RoutingState(RxApp.MainThreadScheduler);

        public LayoutViewModel(IOptions<ToolOptions> options)
        {
            _kHToolOptions = options.Value;
            ShowThemeToggle = _kHToolOptions.ShowThemeToggle;
            MenuExecuteCommand = ReactiveCommand.CreateFromTask<MenuViewModel>(MenuExecute);
            UrlPathSegment = "Layout";

            Router.CurrentViewModel.WhereNotNull().Subscribe(c =>
            {
                if (!string.IsNullOrWhiteSpace(c.UrlPathSegment))
                {
                    CurrentPage = c.UrlPathSegment;
                }
            });

            Menu.Connect().TransformToTree(x => x.ParentName ?? string.Empty)
                .Transform(node => node.Item)
                .Bind(out _menuViewModels)
                .DisposeMany()
                .Subscribe();
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
        public virtual async Task MenuExecute(MenuViewModel nameValue)
        {
            await Task.CompletedTask;
            MessageBus.Current.SendMessage(nameValue, "Menu");
            if (nameValue.ViewModel is not null)
            {
                var vm = Navigate(nameValue.ViewModel, this);
                await Router.Navigate.Execute(vm);
            }


        }

        public SourceCache<MenuViewModel, string> Menu { get; set; }

        private ReadOnlyObservableCollection<MenuViewModel> _menuViewModels;
        public ReadOnlyObservableCollection<MenuViewModel> MenuViewModels => _menuViewModels;


        /// <summary>
        /// 当前页面名称
        /// </summary>
        [Reactive]
        public string? CurrentPage { get; set; }
    }

    public class MenuViewModel : ReactiveObject
    {
        public MenuViewModel(string name,
            string? parentName,
            IObservable<bool> changeEnable,
            IObservable<bool> changeShow,
            IObservable<Color?>? changeColor = null,
            IObservable<string>? changeIcon = null,
            IObservable<string>? changeDisplayName = null,
             Type? viewModel = null
            )
        {
            Name = name;
            ViewModel = viewModel;

            changeEnable.ToPropertyEx(this, x => x.Enable);
            changeShow.ToPropertyEx(this, x => x.Show);
            changeColor?.ToPropertyEx(this, x => x.Color);
            changeIcon?.ToPropertyEx(this, x => x.Icon);
            changeDisplayName?.ToPropertyEx(this, x => x.DisplayName);
        }


        /// <summary>
        /// 名称
        /// </summary>
        [Reactive]
        public string Name { get; set; }

        [Reactive]
        public string? ParentName { get; set; }

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

        public void AddChild(string name,
            IObservable<bool> changeEnable,
            IObservable<bool> changeShow,
            IObservable<Color?>? changeColor = null,
            IObservable<string>? changeIcon = null,
            IObservable<string>? changeDisplayName = null,
             Type? viewModel = null)
        {
            //Children.Add(new MenuViewModel(name, Name + name, changeEnable, changeShow, changeColor, changeIcon, changeDisplayName, viewModel));
        }

        public ReadOnlyObservableCollection<MenuViewModel> Children => _children;
        private readonly ReadOnlyObservableCollection<MenuViewModel> _children;

    }
}
