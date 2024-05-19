using System.Drawing;
using System.Xml.Linq;

namespace Ty
{
    public class MenuOptions
    {
        public bool ShowThemeToggle { get; set; } = true;
        public List<MenuInfo> Menus { get; set; } = [];
    }

    public class MenuInfo
    {
        public required string Name { get; set; }
        public required string DisplayName { get; set; }
        public required string GroupName { get; set; }
        public string? Icon { get; set; }
        public Color? Color { get; set; }
        public bool Enable { get; set; }
        public bool Show { get; set; }
        /// <summary>
        /// 视图模型
        /// </summary>
        public Type? ViewModel { get; set; }
    }

}
