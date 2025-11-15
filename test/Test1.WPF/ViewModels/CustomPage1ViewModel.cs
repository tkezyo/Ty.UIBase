using System.Reactive.Linq;
using Ty;
using Ty.Services;
using Ty.ViewModels;
using ReactiveUI.SourceGenerators;
using Ty.ViewModels.CustomPages;

namespace Test1.WPF.ViewModels
{
    public partial class CustomPage1ViewModel : ViewModelBase, ICustomPageViewModel
    {
        private readonly IMessageBoxManager _messageBoxManager;

        public static string Category => "123";
        public static string Name => "234";

        [Input]
        [Reactive]
        public int Number { get; set; }

        public CustomPage1ViewModel(IMessageBoxManager messageBoxManager)
        {
            this._messageBoxManager = messageBoxManager;
        }

        public override async Task Activate()
        {
            await _messageBoxManager.Notify.Handle(new NotifyInfo
            {
                Expiration = TimeSpan.FromSeconds(3),
                Level = NotifyLevel.Error,
                Message = "sdfwef",
                Title = "fwoiefj"
            });
        }
    }
}
