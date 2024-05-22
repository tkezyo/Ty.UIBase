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

        public TestViewModel(IMessageBoxManager messageBoxManager, MenuService menuService)
        {
            this._messageBoxManager = messageBoxManager;
            this._menuService = menuService;
            ShowMessageCommand = ReactiveCommand.CreateFromTask(ShowMessage);
        }

        public ReactiveCommand<Unit, Unit> ShowMessageCommand { get; }

        public async Task ShowMessage()
        {
            var r = await _messageBoxManager.Prompt.Handle(new PromptInfo("输入"));

            if (r.Ok)
            {
                _menuService.ChangeDisplayName(UrlPathSegment, r.Value);
            }
        }
    }
}
