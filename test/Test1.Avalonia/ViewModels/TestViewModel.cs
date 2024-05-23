using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Ty.Services;
using Ty.ViewModels;

namespace Test1.Avalonia.ViewModels
{
    public class TestViewModel : ViewModelBase
    {
        private readonly IMessageBoxManager _messageBoxManager;
        private readonly MenuService _menuService;
        private readonly PermissionService _permissionService;

        public TestViewModel(IMessageBoxManager messageBoxManager, MenuService menuService, PermissionService permissionService)
        {
            this._messageBoxManager = messageBoxManager;
            this._menuService = menuService;
            this._permissionService = permissionService;
            ShowMessageCommand = ReactiveCommand.CreateFromTask(ShowMessage);
        }

        public ReactiveCommand<Unit, Unit> ShowMessageCommand { get; }

        public async Task ShowMessage()
        {
            var r = await _messageBoxManager.Prompt.Handle(new PromptInfo("输入"));

            if (r.Ok && !string.IsNullOrEmpty(r.Value))
            {
                _menuService.ChangeDisplayName(UrlPathSegment!, r.Value);
                _permissionService.AddPermission("11");
            }
        }
    }
}
