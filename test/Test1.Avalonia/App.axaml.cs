using Avalonia;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Hosting;
using System;
using Ty;

namespace Test1.Avalonia
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            base.OnFrameworkInitializationCompleted();
            var host = ApplicationHostBuilder.CreateApplicationHost<Test1AvaloniaModule>([]).GetAwaiter().GetResult();

            host.Run();
        }
    }
}