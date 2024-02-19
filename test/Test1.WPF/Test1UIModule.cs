using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ty;
using Ty.Views;

namespace Test1.WPF
{
    public class Test1UIModule : ModuleBase
    {
        public override Task ConfigureServices(IServiceCollection serviceDescriptors, IConfigurationRoot configurationRoot)
        {
            serviceDescriptors.AddSingleton<App>();
            serviceDescriptors.AddHostedService<WpfHostedService<App, MainWindow>>();
            serviceDescriptors.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

            serviceDescriptors.Configure<PageOptions>(options =>
            {
                options.Title = "配置编辑器";
            });

            return Task.CompletedTask;
        }

        public override void DependsOn()
        {
            AddDepend<TyWPFBaseModule>();
        }
    }
}
