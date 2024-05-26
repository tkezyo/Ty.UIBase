using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        Task WrapAsync(List<NameValue> inputs, CancellationToken cancellationToken);
        Task Load();
    }
    public interface ICustomPageInjectViewModel
    {
    }

}
