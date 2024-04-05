using Microsoft.Extensions.Hosting;
using Ty;

namespace Test1
{
    public class Test1Module : ModuleBase
    {
        public override Task ConfigureServices(IHostApplicationBuilder builder)
        {
            Console.WriteLine("来了");

            return base.ConfigureServices(builder);
        }
    }
}
