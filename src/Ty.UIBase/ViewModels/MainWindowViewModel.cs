using ReactiveUI;

namespace Ty.ViewModels
{
    public class MainWindowViewModel : ReactiveObject, IScreen
    {
        public string Title { get; set; } = "Ty";
        public RoutingState Router { get; } = new RoutingState();
    }
}
