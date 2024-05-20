using DynamicData;
using Microsoft.Extensions.Options;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
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

        //public IEnumerable<MenuViewModel> GetParent(MenuViewModel menu, IEnumerable<MenuViewModel> list)
        //{
        //    return list.FirstOrDefault(x => x.Name == menu.ParentName);
        //}

        public LayoutViewModel(IOptions<MenuOptions> options, MenuService menuService)
        {
            _kHToolOptions = options.Value;
            ShowThemeToggle = _kHToolOptions.ShowThemeToggle;
            MenuExecuteCommand = ReactiveCommand.CreateFromTask<MenuViewModel>(MenuExecute);
            UrlPathSegment = "Layout";
            this._menuService = menuService;


            menuService.Menus.Connect().Subscribe(c =>
            {
                foreach (var change in c)
                {
                    var parent = Menus.FirstOrDefault(x => x.Name == change.Current.Name);

                    switch (change.Reason)
                    {
                        case ChangeReason.Add:
                            Menus.Add(new(change.Current));
                            // 处理添加事件
                            break;
                        case ChangeReason.Update:
                            // 处理更新事件
                            break;
                        case ChangeReason.Remove:
                            // 处理删除事件
                            break;
                        case ChangeReason.Refresh:
                            // 处理刷新事件
                            break;
                            // 其他更改原因
                    }
                }
            });


            Router.CurrentViewModel.WhereNotNull().Subscribe(c =>
            {
                if (!string.IsNullOrWhiteSpace(c.UrlPathSegment))
                {
                    CurrentPage = c.UrlPathSegment;
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

        /// <summary>
        /// 当前页面名称
        /// </summary>
        [Reactive]
        public string? CurrentPage { get; set; }
    }


}
