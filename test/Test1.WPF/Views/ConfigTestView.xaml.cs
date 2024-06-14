using ReactiveUI;
using Test1.WPF.ViewModels;

namespace Test1.WPF.Views
{
    /// <summary>
    /// ConfigTestView.xaml 的交互逻辑
    /// </summary>
    public partial class ConfigTestView : ReactiveUserControl<ConfigTestViewModel>
    {
        public ConfigTestView()
        {
            InitializeComponent();
            this.WhenActivated(d => { });
        }
    }
}
