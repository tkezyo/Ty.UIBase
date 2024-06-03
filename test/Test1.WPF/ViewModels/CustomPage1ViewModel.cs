using ReactiveUI.Fody.Helpers;
using Ty.Module.Configs;
using Ty.ViewModels;
using Ty.ViewModels.CustomPages;

namespace Test1.WPF.ViewModels
{
    public partial class CustomPage1ViewModel : ViewModelBase, ICustomPageViewModel
    {
        public static string Category => "123";
        public static string Name => "234";

        [Input]
        [Reactive]
        public int Number { get; set; }

    }
}
