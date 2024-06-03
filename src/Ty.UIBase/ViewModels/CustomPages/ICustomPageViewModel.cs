using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ty.ViewModels.CustomPages
{
    public interface ICustomPageViewModel : ICustomPageInjectViewModel, ITyRoutableViewModel
    {
        /// <summary>
        /// 类别
        /// </summary>
        static abstract string Category { get; }
        /// <summary>
        /// 名称
        /// </summary>
        static abstract string Name { get; }
        static abstract CustomViewDefinition GetDefinition();
        void SetCustomPageValue(List<NameValue> inputs);

        public static T? GetValue<T>(string name, List<NameValue> inputs)
        {
            var input = inputs.FirstOrDefault(c => c.Name == name);
            if (input is null)
            {
                return default;
            }
            if (typeof(T).FullName == "String")
            {
                //确保首尾都是"
                if (input.Value.StartsWith("\"") && input.Value.EndsWith("\""))
                {
                    return JsonSerializer.Deserialize<T>(input.Value);
                }
                return JsonSerializer.Deserialize<T>($"\"{input.Value}\"");
            }
            return JsonSerializer.Deserialize<T>(input.Value);
        }
    }
    public interface ICustomPageInjectViewModel
    {
    }

}
