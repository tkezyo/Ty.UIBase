using Microsoft.Extensions.DependencyInjection;

namespace Ty.Module.Configs
{
    public interface IOptionProvider<T> : IOptionProvider
    {
    }
    public interface IOptionProvider : IOptionProviderInject
    {
        /// <summary>
        /// 名称
        /// </summary>
        static abstract string DisplayName { get; }
        static abstract string Name { get; }
        static abstract string Type { get; }
    }
    public interface IOptionProviderInject
    {
        IAsyncEnumerable<NameValue> GetOptions();
    }

    public class ConfigOption
    {
        /// <summary>
        /// 选项集
        /// </summary>
        public Dictionary<string, List<NameValue>> OptionProviders { get; set; } = [];

        public List<NameValue> GetOrAddType(string type)
        {
            if (!OptionProviders.TryGetValue(type, out var group))
            {
                group = [];
                OptionProviders.Add(type, group);
            }
            return group;
        }
    }

    public static partial class ConfigExtension
    {
        public static void AddConfigOptionProvider<T>(this IServiceCollection serviceDescriptors)
       where T : class, IOptionProvider
        {
            var f = T.Type + ":" + T.Name;
            serviceDescriptors.AddKeyedTransient<IOptionProviderInject, T>(T.Type + ":" + T.Name);
            serviceDescriptors.Configure<ConfigOption>(c =>
            {
                var group = c.GetOrAddType(T.Type);

                group.Add(new NameValue(T.DisplayName, T.Name));
            });
        }
    }
}
