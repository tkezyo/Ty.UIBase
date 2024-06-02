using ReactiveUI;
using Ty.ViewModels.CustomPages;

namespace Ty.Views.CustomPages
{
    /// <summary>
    /// CustomPageView.xaml 的交互逻辑
    /// </summary>
    public partial class CustomPageView : ReactiveUserControl<CustomPageViewModel>
    {
        public CustomPageView()
        {
            InitializeComponent();
            this.WhenActivated(d => { });
        }
    }
}
