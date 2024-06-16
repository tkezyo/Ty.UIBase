using Avalonia;
using Avalonia.Controls.Notifications;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using Avalonia.Styling;
using ReactiveUI;
using System.Reactive;
using Ty.Services;
using Ty.ViewModels;

namespace Ty.AvaloniaBase.Views;

public partial class LayoutView : ReactiveUserControl<LayoutViewModel>
{
    public LayoutView()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
        });
    }


    private void ToggleButton_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        var app = Application.Current;
        if (app is not null)
        {
            var theme = app.ActualThemeVariant;
            app.RequestedThemeVariant = theme == ThemeVariant.Dark ? ThemeVariant.Light : ThemeVariant.Dark;
        }
    }
}