using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace Ty;


public abstract class ModuleBase : IModule
{
    public string Name => GetType().Name;
    public Dictionary<string, IModule> Modules { get; set; } = [];

    public virtual Task ConfigureServices(IHostApplicationBuilder builder)
    {
        return Task.CompletedTask;
    }

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

        Modules.TryAdd(typeof(T).Name, t);
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
    Task PreConfigureServices(IHostApplicationBuilder hostApplicationBuilder);
    Task PostConfigureServices(IServiceProvider serviceProvider);

    public static async Task<IHost?> CreateApplicationHost<T>(string[] args, string? dependsOnFolder = null, bool skipVerification = true)
        where T : IModule, new()
    {
        SkipVerification = skipVerification;
        List<IModule> modules = [];
        if (!string.IsNullOrEmpty(dependsOnFolder))
        {
            //读取 dlls文件夹下的所有 dll文件，然后加载
            var files = Directory.GetFiles(dependsOnFolder, "*.dll");
            foreach (var item in files)
            {
                FileInfo fileInfo = new(item);
                var assembly = Assembly.LoadFile(fileInfo.FullName);
                var types = assembly.GetTypes().Where(c => c.GetInterfaces().Contains(typeof(IModule)));
                foreach (var type in types)
                {
                    var module = (IModule?)Activator.CreateInstance(type);
                    if (module is not null)
                    {
                        module.DependsOn();
                        modules.Add(module);
                    }
                }
            }
        }

        var t = new T();
        t.DependsOn();
        modules.Add(t);

        var orderedModules = GetOrderedModules<T>(modules, skipVerification);


        var hostBuilder = Host.CreateApplicationBuilder(args);

        foreach (var item in orderedModules)
        {
            await item.Module.PreConfigureServices(hostBuilder);
        }
        foreach (var item in orderedModules)
        {
            await item.Module.ConfigureServices(hostBuilder);
        }

        //build host
        var host = hostBuilder.Build();

        foreach (var item in orderedModules)
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
    public static List<ModuleModel> GetOrderedModules<T>(List<IModule> moduleList, bool skipVerification = true)
        where T : IModule, new()
    {
        SkipVerification = skipVerification;
        List<ModuleModel> results = [];

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
            if (results.Any(c => c.Module.Name == moduleModel.Module.Name))
            {
                return;
            }
            results.Add(moduleModel);
        }

        foreach (var item in moduleList)
        {
            item.DependsOn();
            SetModules(item);
        }

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
        Order(results);
        return results;
    }

}
public class ModuleModel
{
    public List<string> PreModules { get; set; } = [];
    public required IModule Module { get; set; }
}
