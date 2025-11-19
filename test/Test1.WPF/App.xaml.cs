using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Windows;
using Ty;
using Ty.Views;

namespace Test1.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

          
                try
                {
                    var mainWindow = TyApp.ServiceProvider.GetRequiredService<MainWindow>();
                    // Stop host when main window closes (host wired to stop app via StopApplication inside window)
                    mainWindow.Closed += (s, args) => { /* No-op here; window will call StopApplication */ };
                    MainWindow = mainWindow;
                    MainWindow.Show();

                    // When host is stopping, shutdown WPF
                    var hostAppLifetime = TyApp.ServiceProvider.GetService<IHostApplicationLifetime>();
                    hostAppLifetime?.ApplicationStopping.Register(() => Dispatcher.Invoke(() => Shutdown()));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError($"Failed to create MainWindow: {ex}");
                    throw;
                }
           
        }
    }
}
