using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI.Avalonia;
using Test1.Avalonia.ViewModels;

namespace Test1.Avalonia;

public partial class TestView : ReactiveUserControl<TestViewModel>
{
    public TestView()
    {
        InitializeComponent();
    }
}