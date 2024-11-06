using Ty;
using Ty.Configs;

namespace Test1.WPF.Models
{
    public partial class StringProvider : IOptionProvider<string>
    {
        public static string DisplayName => "字符串";

        public async IAsyncEnumerable<NameValue> GetOptions()
        {
            await Task.CompletedTask;
            yield return new NameValue("123", "123");
            yield return new NameValue("345", "345");
        }
    }
}
