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

        public TestViewModel(IMessageBoxManager messageBoxManager)
        {
            this._messageBoxManager = messageBoxManager;
            ShowMessageCommand = ReactiveCommand.CreateFromTask(ShowMessage);
        }

        public ReactiveCommand<Unit, Unit> ShowMessageCommand { get;  }

        public async Task ShowMessage()
        {
            var r = await _messageBoxManager.Prompt.Handle(new PromptInfo("输入"));

            if (r.Ok)
            {
                await _messageBoxManager.Alert.Handle(new AlertInfo(r.Value));
            }
        }
    }
}
