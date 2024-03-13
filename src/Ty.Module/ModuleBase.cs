using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Ty;


public abstract class ModuleBase : IModule
{
    public string Name => GetType().Name;
    public Dictionary<string, IModule> Modules { get; set; } = [];

    public abstract Task ConfigureServices(IHostApplicationBuilder builder);

    public virtual void DependsOn()
    {
    }

    public void AddDepend<T>()
        where T : IModule, new()
    {
        var t = IModule.AllModules.FirstOrDefault(c => c.Name == typeof(T).Name);
        if (t is null)
        {
            t = new T();
            IModule.AllModules.Add(t);
        }

        Modules.Add(typeof(T).Name, t);
        if (!IModule.SkipVerification && !IModule.Verification(t, this))
        {
            throw new Exception($"模块 {t.Name} 依赖于 {this.Name}，但是 {this.Name} 已经依赖于 {t.Name}，这将导致循环依赖");
        }
        t.DependsOn();
    }

    public virtual async Task PreConfigureServices(IHostApplicationBuilder builder)
    {
        await Task.CompletedTask;
    }

    public virtual async Task PostConfigureServices(IServiceProvider serviceProvider)
    {
        await Task.CompletedTask;
    }
}

public interface IModule
{
    string Name { get; }
    Dictionary<string, IModule> Modules { get; set; }
    void DependsOn();
    Task ConfigureServices(IHostApplicationBuilder hostApplicationBuilder);
    Task PreConfigureServices(IHostApplicationBuilder  hostApplicationBuilder);
    Task PostConfigureServices(IServiceProvider serviceProvider);

    public static async Task<IHost?> CreateApplicationHost<T>(string[] args, bool skipVerification = true)
        where T : IModule, new()
    {
        SkipVerification = skipVerification;
        var modules = GetOrderedModules<T>(skipVerification);

        var hostBuilder = Host.CreateApplicationBuilder(args);

        foreach (var item in modules)
        {
            await item.Module.PreConfigureServices(hostBuilder);
        }
        foreach (var item in modules)
        {
            await item.Module.ConfigureServices(hostBuilder);
        }

        //build host
        var host = hostBuilder.Build();

        foreach (var item in modules)
        {
            await item.Module.PostConfigureServices(host.Services);
        }
        return host;
    }
  
    public static bool SkipVerification { get; set; } = true;
    public static List<IModule> AllModules { get; set; } = [];
    public static bool Verification(IModule newPre, IModule source)
    {
        if (newPre.Modules.Any(c => c.Key == source.Name))
        {
            return false;
        }

        foreach (var item in newPre.Modules)
        {
            var action = AllModules.First(c => c.Name == item.Key);

            var r = Verification(action, source);
            if (!r)
            {
                return false;
            }
        }

        return true;
    }
    public static List<ModuleModel> GetOrderedModules<T>(bool skipVerification = true)
        where T : IModule, new()
    {
        SkipVerification = skipVerification;
        List<ModuleModel> modules = [];

        void SetModules(IModule module)
        {
            ModuleModel moduleModel = new()
            {
                Module = module,
            };
            foreach (var attr in module.Modules)
            {
                moduleModel.PreModules.Add(attr.Key);
                SetModules(attr.Value);
            }
            modules.Add(moduleModel);
        }
        var t = new T();
        t.DependsOn();
        SetModules(t);

        List<ModuleModel> Order(IEnumerable<ModuleModel> modules)
        {
            List<ModuleModel> result = [];

            List<string> total = modules.Select(c => c.Module.Name).ToList();

            List<(string, string)> temp = [];

            foreach (var action in modules)
            {
                foreach (var item in action.PreModules)
                {
                    temp.Add((action.Module.Name, item));
                }
            }

            while (total.Count != 0)
            {
                var has = temp.Select(c => c.Item2).Distinct().ToList();
                var ordered = total.Except(has);
                total = has;
                temp = temp.Where(c => !ordered.Contains(c.Item1)).ToList();
                result.AddRange(modules.Where(c => ordered.Contains(c.Module.Name)).ToList());
            }

            return result;
        }
        Order(modules);
        return modules;
    }

}
public class ModuleModel
{
    public List<string> PreModules { get; set; } = [];
    public required IModule Module { get; set; }
}
