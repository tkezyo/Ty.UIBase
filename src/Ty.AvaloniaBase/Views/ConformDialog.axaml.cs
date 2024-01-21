using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Ty.AvaloniaBase.Views;

public partial class ConformDialog : Window
{
    public ConformDialog()
    {
        InitializeComponent();
        ok.Click += Ok_Click;
        cancel.Click += Cancel_Click;
    }
    private void Cancel_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close(false);
    }

    private void Ok_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close(true);
    }
}